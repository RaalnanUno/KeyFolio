using MailFolio.Console.Cli;
using MailFolio.Console.Config;
using MailFolio.Console.Reporting;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace MailFolio.Console.Mail;

public static class MailSender
{
    public sealed record ResolvedBody(string Source, bool IsHtml, string Content);

    public static ResolvedBody ResolveBody(MailFolioArgs args)
    {
        // If BodyFile specified: assume HTML
        if (!string.IsNullOrWhiteSpace(args.BodyFile))
        {
            var html = File.ReadAllText(args.BodyFile);
            return new ResolvedBody(Source: "BodyFile", IsHtml: true, Content: html);
        }

        // Default: plain text body
        if (!string.IsNullOrWhiteSpace(args.Body))
            return new ResolvedBody(Source: "Body", IsHtml: false, Content: args.Body);

        // Parser should prevent this, but keep it defensive
        throw new MailFolioException(ErrorCodes.MissingBody, "Either '--body' or '--bodyFile' must be provided.");
    }

    public static string? Send(
        MailFolioKeyFile keyFile,
        string smtpUsername,
        string smtpPassword,
        MailFolioArgs args,
        ResolvedBody body)
    {
        var message = BuildMessage(keyFile, args, body);

        var tlsMode = TlsModeParser.Parse(keyFile.TlsMode);
        var socketOpt = tlsMode switch
        {
            TlsMode.None => SecureSocketOptions.None,
            TlsMode.SslOnConnect => SecureSocketOptions.SslOnConnect,
            _ => SecureSocketOptions.StartTls
        };

        try
        {
            using var client = new SmtpClient();

            try
            {
                client.Connect(keyFile.Server, keyFile.Port, socketOpt);
            }
            catch (Exception ex)
            {
                throw new MailFolioException(ErrorCodes.SmtpConnectFailed, "Failed to connect to SMTP server.", ex);
            }

            try
            {
                client.Authenticate(smtpUsername, smtpPassword);
            }
            catch (Exception ex)
            {
                throw new MailFolioException(ErrorCodes.SmtpAuthFailed, "SMTP authentication failed.", ex);
            }

            try
            {
                // Returns server response string. Message-Id is in message.MessageId.
                client.Send(message);
                client.Disconnect(true);

                // Return a provider-ish identifier if useful
                return message.MessageId;
            }
            catch (Exception ex)
            {
                throw new MailFolioException(ErrorCodes.SmtpSendFailed, "Failed to send email.", ex);
            }
        }
        catch
        {
            // Let caller handle
            throw;
        }
    }

    private static MimeMessage BuildMessage(MailFolioKeyFile keyFile, MailFolioArgs args, ResolvedBody body)
    {
        var message = new MimeMessage();

        // From
        if (!string.IsNullOrWhiteSpace(keyFile.FromName))
            message.From.Add(new MailboxAddress(keyFile.FromName, keyFile.FromEmail));
        else
            message.From.Add(MailboxAddress.Parse(keyFile.FromEmail));

        // To
        message.To.Add(MailboxAddress.Parse(args.ToEmail));

        // Cc/Bcc/ReplyTo
        foreach (var cc in args.Cc) message.Cc.Add(MailboxAddress.Parse(cc));
        foreach (var bcc in args.Bcc) message.Bcc.Add(MailboxAddress.Parse(bcc));
        foreach (var rt in args.ReplyTo) message.ReplyTo.Add(MailboxAddress.Parse(rt));

        // Subject (used for sending, never stored)
        message.Subject = args.Subject ?? "";

        // Body + attachments
        var builder = new BodyBuilder();

        if (body.IsHtml || args.HtmlFlag)
            builder.HtmlBody = body.Content;
        else
            builder.TextBody = body.Content;

        foreach (var path in args.Attachments)
        {
            builder.Attachments.Add(path);
        }

        message.Body = builder.ToMessageBody();
        return message;
    }
}

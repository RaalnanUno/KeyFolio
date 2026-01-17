namespace MailFolio.Console.Cli;

public sealed class MailFolioArgs
{
    public string ToEmail { get; set; } = "";

    public string Subject { get; set; } = ""; // used for sending, but never stored in DB
    public string? Body { get; set; }
    public string? BodyFile { get; set; }

    public bool DryRun { get; set; }
    public bool HtmlFlag { get; set; } // optional override if you later want it
    public bool Verbose { get; set; }  // parsed but not used for console logs (kept for future)

    public List<string> Attachments { get; } = new();
    public List<string> Cc { get; } = new();
    public List<string> Bcc { get; } = new();
    public List<string> ReplyTo { get; } = new();
}

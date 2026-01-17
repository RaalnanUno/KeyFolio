using System.Text.Json;
using MailFolio.Console.Reporting;

namespace MailFolio.Console.Config;

public static class KeyFileLoader
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static MailFolioKeyFile Load(string keyFilePath)
    {
        if (string.IsNullOrWhiteSpace(keyFilePath))
            throw new MailFolioException(ErrorCodes.MissingKeyFile, "MAILFOLIO_KEYFILE is missing or empty.");

        if (!File.Exists(keyFilePath))
            throw new MailFolioException(ErrorCodes.MissingKeyFile, $"Key file not found: '{keyFilePath}'.");

        try
        {
            var json = File.ReadAllText(keyFilePath);
            var model = JsonSerializer.Deserialize<MailFolioKeyFile>(json, JsonOpts);
            if (model is null)
                throw new MailFolioException(ErrorCodes.InvalidKeyFile, "Key file JSON could not be parsed.");

            if (string.IsNullOrWhiteSpace(model.Server))
                throw new MailFolioException(ErrorCodes.InvalidKeyFile, "Key file missing 'Server'.");

            if (model.Port <= 0)
                throw new MailFolioException(ErrorCodes.InvalidKeyFile, "Key file has invalid 'Port'.");

            if (string.IsNullOrWhiteSpace(model.FromEmail))
                throw new MailFolioException(ErrorCodes.InvalidKeyFile, "Key file missing 'FromEmail'.");

            if (string.IsNullOrWhiteSpace(model.UsernameEnc) || string.IsNullOrWhiteSpace(model.PasswordEnc))
                throw new MailFolioException(ErrorCodes.InvalidKeyFile, "Key file missing 'UsernameEnc' and/or 'PasswordEnc'.");

            return model;
        }
        catch (MailFolioException) { throw; }
        catch (Exception ex)
        {
            throw new MailFolioException(ErrorCodes.InvalidKeyFile, "Key file JSON was invalid.", ex);
        }
    }
}

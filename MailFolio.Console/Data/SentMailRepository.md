using Microsoft.Data.Sqlite;
using MailFolio.Console.Reporting;

namespace MailFolio.Console.Data;

public static class SentMailRepository
{
    public static void EnsureCreated(string dbPath, string schemaSql)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        using var conn = new SqliteConnection($"Data Source={dbPath}");
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = schemaSql;
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Upserts by ErrorCode (1 row per code). Updates LastOccurredUtc, increments OccurrenceCount, and replaces payload/context.
    /// </summary>
    public static void UpsertFailure(
        string dbPath,
        string errorCode,
        DateTimeOffset lastOccurredUtc,
        bool dryRun,
        string? fromEmail,
        string? toEmail,
        string? server,
        int? port,
        string? tlsMode,
        byte[] resultJson)
    {
        try
        {
            using var conn = new SqliteConnection($"Data Source={dbPath}");
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO SentMail (
    ErrorCode, LastOccurredUtc, OccurrenceCount,
    FromEmail, ToEmail, Server, Port, TlsMode, DryRun,
    ResultJson
) VALUES (
    @ErrorCode, @LastOccurredUtc, 1,
    @FromEmail, @ToEmail, @Server, @Port, @TlsMode, @DryRun,
    @ResultJson
)
ON CONFLICT(ErrorCode) DO UPDATE SET
    LastOccurredUtc = excluded.LastOccurredUtc,
    OccurrenceCount = SentMail.OccurrenceCount + 1,
    FromEmail = excluded.FromEmail,
    ToEmail = excluded.ToEmail,
    Server = excluded.Server,
    Port = excluded.Port,
    TlsMode = excluded.TlsMode,
    DryRun = excluded.DryRun,
    ResultJson = excluded.ResultJson;
";

            cmd.Parameters.AddWithValue("@ErrorCode", errorCode);
            cmd.Parameters.AddWithValue("@LastOccurredUtc", lastOccurredUtc.UtcDateTime.ToString("O"));
            cmd.Parameters.AddWithValue("@DryRun", dryRun ? 1 : 0);

            cmd.Parameters.AddWithValue("@FromEmail", (object?)fromEmail ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ToEmail", (object?)toEmail ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Server", (object?)server ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Port", (object?)port ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TlsMode", (object?)tlsMode ?? DBNull.Value);

            cmd.Parameters.Add("@ResultJson", SqliteType.Blob).Value = resultJson;

            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            // Convert to stable error code for DB layer failures.
            throw new MailFolioException(ErrorCodes.DbWriteFailed, "Failed to write failure record to SQLite.", ex);
        }
    }
}

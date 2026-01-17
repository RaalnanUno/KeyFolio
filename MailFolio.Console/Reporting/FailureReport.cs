using System.Text.Json.Serialization;

namespace MailFolio.Console.Reporting;

public sealed record FailureReport
{
    public string ErrorCode { get; init; } = ErrorCodes.Unknown;

    public DateTimeOffset StartedUtc { get; init; }
    public DateTimeOffset FinishedUtc { get; init; }
    public long DurationMs { get; init; }

    public bool DryRun { get; init; }

    // Safe context (no subject/body, no username/password)
    public string? FromEmail { get; init; }
    public string? ToEmail { get; init; }
    public string? Server { get; init; }
    public int? Port { get; init; }
    public string? TlsMode { get; init; }

    public string? BodySource { get; init; } // "Body" | "BodyFile"
    public bool? IsHtml { get; init; }

    public string[] Attachments { get; init; } = Array.Empty<string>();

    public ErrorInfo Error { get; init; } = new();

    public sealed record ErrorInfo
    {
        public string Type { get; init; } = "";
        public string Message { get; init; } = "";
        public string? StackTrace { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ErrorInfo? Inner { get; init; }
    }
}

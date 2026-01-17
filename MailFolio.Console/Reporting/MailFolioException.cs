namespace MailFolio.Console.Reporting;

public sealed class MailFolioException : Exception
{
    public string ErrorCode { get; }

    public MailFolioException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public MailFolioException(string errorCode, string message, Exception inner)
        : base(message, inner)
    {
        ErrorCode = errorCode;
    }
}

namespace MailFolio.Console;

public static class Out
{
    public static void Info(string message) => System.Console.WriteLine(message);

    public static void Error(string message)
    {
        var prior = System.Console.ForegroundColor;
        try
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.Error.WriteLine(message);
        }
        finally
        {
            System.Console.ForegroundColor = prior;
        }
    }
}

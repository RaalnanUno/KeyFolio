using System.Diagnostics;
using System.Text;
using KeyFolio.ConsoleApp;
using KeyFolio.Core.Crypto;

namespace KeyFolio.ConsoleApp;

internal static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || IsHelp(args[0]))
            {
                PrintHelp();
                return 0;
            }

            var cmd = args[0].Trim().ToLowerInvariant();

            // ---------------------------
            // Examples: KeyFolio.Console examples pipe
            // ---------------------------
            if (cmd is "examples" or "example")
                return RunExamples(args);

            // ---------------------------
            // Encrypt/Decrypt
            // ---------------------------
            var input = args.Length >= 2
                ? string.Join(' ', args.Skip(1))
                : ReadAllStdIn();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.Error.WriteLine("No input provided. Pass a value as an argument or pipe via stdin.");
                return 2;
            }

            var keyfolio = new KeyFolio.Core.KeyFolio();
            var secretProvider = new EnvOrPromptSecretProvider("KEYFOLIO_SECRET");

            return cmd switch
            {
                "encrypt" or "enc" => RunEncrypt(keyfolio, secretProvider, input),
                "decrypt" or "dec" => RunDecrypt(keyfolio, secretProvider, input),
                _ => UnknownCommand(cmd)
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Fatal error:");
            Console.Error.WriteLine(ex.ToString());
            return 99;
        }
    }

    private static int RunExamples(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Missing example name.");
            Console.Error.WriteLine("Try: KeyFolio.Console examples pipe");
            return 2;
        }

        var which = args[1].Trim().ToLowerInvariant();
        return which switch
        {
            "pipe" or "pipeinput" => ExampleRunner.RunPipeInputExample(),
            _ => UnknownExample(which)
        };
    }

    private static int UnknownExample(string which)
    {
        Console.Error.WriteLine($"Unknown example: {which}");
        Console.Error.WriteLine("Available examples:");
        Console.Error.WriteLine("  pipe   - Runs RunExamples\\PipeInput.ps1");
        return 1;
    }

    private static int RunEncrypt(KeyFolio.Core.KeyFolio keyfolio, ISecretProvider secretProvider, string plaintext)
    {
        try
        {
            var encrypted = keyfolio.Encrypt(plaintext, secretProvider);
            Console.WriteLine(encrypted);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Encrypt failed:");
            Console.Error.WriteLine(ex.Message);
            return 10;
        }
    }

    private static int RunDecrypt(KeyFolio.Core.KeyFolio keyfolio, ISecretProvider secretProvider, string envelope)
    {
        try
        {
            var decrypted = keyfolio.Decrypt(envelope, secretProvider);
            Console.WriteLine(decrypted);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Decrypt failed:");
            Console.Error.WriteLine(ex.Message);
            return 11;
        }
    }

    private static int UnknownCommand(string cmd)
    {
        Console.Error.WriteLine($"Unknown command: {cmd}");
        PrintHelp();
        return 1;
    }

    private static bool IsHelp(string arg)
    {
        var a = arg.Trim().ToLowerInvariant();
        return a is "-h" or "--help" or "help" or "/?";
    }

    private static void PrintHelp()
    {
        Console.WriteLine(
@"KeyFolio.Console - Encrypt/decrypt portable strings using KeyFolio.Core (AES-GCM)

Usage:
  KeyFolio.Console encrypt ""plain text here""
  KeyFolio.Console decrypt ""keyfolio:v1:...""

Examples:
  KeyFolio.Console examples pipe

Piping:
  echo ""hello"" | KeyFolio.Console encrypt
  type .\message.txt | KeyFolio.Console encrypt
  type .\cipher.txt   | KeyFolio.Console decrypt

Secret:
  Uses environment variable KEYFOLIO_SECRET if set.
  If not set, prompts once per process (secret cached in-memory for the session).

Exit codes:
  0  success
  1  unknown command
  2  missing input / bad args
  10 encrypt failed
  11 decrypt failed
  99 fatal error
");
    }

    private static string ReadAllStdIn()
    {
        using var reader = new StreamReader(Console.OpenStandardInput(), Encoding.UTF8);
        return reader.ReadToEnd().TrimEnd('\r', '\n');
    }
}

/// <summary>
/// Reads KEYFOLIO_SECRET from env var; if missing, prompts in-console (masked).
/// Caches the secret in memory for this process.
/// </summary>
internal sealed class EnvOrPromptSecretProvider : ISecretProvider
{
    private readonly string _envVarName;
    private string? _cached;

    public EnvOrPromptSecretProvider(string envVarName)
    {
        _envVarName = envVarName;
    }

    public string GetSecret()
    {
        if (!string.IsNullOrWhiteSpace(_cached))
            return _cached!;

        var env = Environment.GetEnvironmentVariable(_envVarName);
        if (!string.IsNullOrWhiteSpace(env))
        {
            _cached = env;
            return env;
        }

        Console.Error.Write($"{_envVarName} not set. Enter passphrase: ");
        var secret = ReadPassword();

        if (string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException("No passphrase provided.");

        _cached = secret;
        Console.Error.WriteLine(); // move to next line after prompt
        return _cached!;
    }

    private static string ReadPassword()
    {
        var sb = new StringBuilder();

        while (true)
        {
            // ReadKey may throw in some redirected/non-interactive contexts.
            // In that case, you can fallback to Console.ReadLine().
            var key = Console.ReadKey(intercept: true);

            if (key.Key == ConsoleKey.Enter)
                break;

            if (key.Key == ConsoleKey.Backspace)
            {
                if (sb.Length > 0)
                {
                    sb.Length--;
                    Console.Error.Write("\b \b");
                }
                continue;
            }

            if (!char.IsControl(key.KeyChar))
            {
                sb.Append(key.KeyChar);
                Console.Error.Write("*");
            }
        }

        return sb.ToString();
    }
}

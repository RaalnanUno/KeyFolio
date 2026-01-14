using System.Diagnostics;

namespace KeyFolio.ConsoleApp;

internal static class ExampleRunner
{
    public static int RunPipeInputExample()
    {
        var exeDir = AppContext.BaseDirectory;
        var scriptPath = Path.Combine(exeDir, "RunExamples", "PipeInput.ps1");

        if (!File.Exists(scriptPath))
        {
            Console.Error.WriteLine("Example script not found:");
            Console.Error.WriteLine(scriptPath);
            Console.Error.WriteLine("Build may be missing CopyToOutputDirectory rules for RunExamples.");
            return 3;
        }

        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\"",
            UseShellExecute = false
        };

        using var p = Process.Start(psi);
        p!.WaitForExit();
        return p.ExitCode;
    }
}

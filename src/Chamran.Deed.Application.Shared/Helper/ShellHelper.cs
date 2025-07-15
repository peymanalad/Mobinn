using System;
using System.Diagnostics;
using System.Threading.Tasks;

public static class ShellHelper
{
    public static async Task RunShellCommandAsync(string command)
    {
        var processInfo = new ProcessStartInfo("/bin/bash", $"-c \"{command}\"")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processInfo };
        process.Start();

        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();

        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Shell command failed with exit code {process.ExitCode}: {error}");
        }
    }
}
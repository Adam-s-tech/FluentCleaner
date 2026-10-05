using System.Diagnostics;

namespace FluentCleaner.Services;

public enum SchedulerFrequency { Daily, Weekly, Logon }

// Creates the Windows task that starts this executable with /AUTO. Modern uses
// its own task name so installing a schedule never replaces the Classic one.
public static class TaskSchedulerService
{
    private const string TaskName = "FluentCleaner Modern AutoClean";

    public static bool Exists() => Run("/Query", "/TN", TaskName).ExitCode == 0;

    public static (bool Ok, string Message) CreateOrUpdate(
        SchedulerFrequency frequency, TimeSpan time, bool shutdownAfter)
    {
        var exePath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(exePath))
            return (false, ResourceService.Get("Scheduler_ErrorExecutablePath"));

        var taskRun = $"\"{exePath}\" /AUTO{(shutdownAfter ? " /SHUTDOWN" : "")}";
        var schedule = frequency switch
        {
            SchedulerFrequency.Weekly => new[] { "/SC", "WEEKLY", "/D", "MON", "/ST", time.ToString(@"hh\:mm") },
            SchedulerFrequency.Logon  => new[] { "/SC", "ONLOGON" },
            _                         => new[] { "/SC", "DAILY", "/ST", time.ToString(@"hh\:mm") },
        };

        var args = new List<string> { "/Create", "/TN", TaskName, "/TR", taskRun };
        args.AddRange(schedule);
        args.Add("/F");

        var result = Run(args.ToArray());
        return result.ExitCode == 0
            ? (true, ResourceService.Get("Scheduler_ResultCreated"))
            : (false, string.IsNullOrWhiteSpace(result.Output)
                ? ResourceService.Get("Scheduler_ErrorSchtasks")
                : result.Output.Trim());
    }

    public static (bool Ok, string Message) Delete()
    {
        if (!Exists())
            return (true, ResourceService.Get("Scheduler_ResultNothingToRemove"));

        var result = Run("/Delete", "/TN", TaskName, "/F");
        return result.ExitCode == 0
            ? (true, ResourceService.Get("Scheduler_ResultRemoved"))
            : (false, string.IsNullOrWhiteSpace(result.Output)
                ? ResourceService.Get("Scheduler_ErrorSchtasks")
                : result.Output.Trim());
    }

    private static (int ExitCode, string Output) Run(params string[] args)
    {
        try
        {
            var startInfo = new ProcessStartInfo("schtasks.exe")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            foreach (var arg in args)
                startInfo.ArgumentList.Add(arg);

            using var process = Process.Start(startInfo);
            if (process is null)
                return (-1, ResourceService.Get("Scheduler_ErrorSchtasks"));

            var output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
            process.WaitForExit();
            return (process.ExitCode, output);
        }
        catch (Exception ex)
        {
            return (-1, ex.Message);
        }
    }
}

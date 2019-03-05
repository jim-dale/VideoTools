
namespace VideoTools
{
    using System;
    using System.Diagnostics;
    using System.Text;

    public class ConsoleClient
    {
        public string Command { get; set; }
        public string Arguments { get; set; }

        public int ExitCode { get; set; }
        public string StandardOutput { get; set; }
        public string StandardError { get; set; }

        public ConsoleClient(string command)
        {
            Command = Environment.ExpandEnvironmentVariables(command);
        }

        public void Reset()
        {
            ExitCode = 0;
            StandardOutput = String.Empty;
            StandardError = String.Empty;
        }

        public void Run(params string[] args)
        {
            Arguments = string.Join(" ", args);

            Run();
        }

        public void Run(string args)
        {
            Arguments = args;

            Run();
        }

        public void Run()
        {
            Reset();

            using (var process = new Process())
            {
                process.StartInfo.FileName = Command;
                process.StartInfo.Arguments = Arguments;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.ErrorDialog = false;

                var sbOutput = new StringBuilder();
                var sbError = new StringBuilder();

                process.OutputDataReceived += (sender, eventArgs) =>
                {
                    if (eventArgs.Data != null)
                    {
                        sbOutput.AppendLine(eventArgs.Data);
                    }
                };
                process.ErrorDataReceived += (sender, eventArgs) =>
                {
                    if (eventArgs.Data != null)
                    {
                        sbError.AppendLine(eventArgs.Data);
                    }
                };

                if (process.Start() == false)
                {
                    StandardError = $"Error starting {Command}";
                }
                else
                {
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.WaitForExit();

                    ExitCode = process.ExitCode;
                    StandardOutput = sbOutput.ToString();
                    StandardError = sbError.ToString();
                }
            }
        }
    }
}

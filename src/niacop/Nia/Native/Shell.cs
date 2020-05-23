using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Nia.Native {
    public static class Shell {
        public enum ShellType {
            Generic,
            Windows,
            Unix
        }

        public static ShellType type;
        private static string? pathDelim;

        static Shell() {
            switch (Environment.OSVersion.Platform) {
                case PlatformID.Win32NT:
                    type = ShellType.Windows;
                    pathDelim = ";";
                    break;

                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    type = ShellType.Unix;
                    pathDelim = ":";
                    break;
            }
        }

        public struct ExecuteResult {
            public int exitCode;
            public string stdout;
            public string stderr;
        }

        public static ExecuteResult executeEval(string command) {
            var proc = shellExecute(command);
            proc.Start();
            proc.WaitForExit();
            return new ExecuteResult {
                exitCode = proc.ExitCode,
                stdout = proc.StandardOutput.ReadToEnd(),
                stderr = proc.StandardError.ReadToEnd()
            };
        }

        public static Process shellExecute(string command) {
            var shellProcess = new Process {
                StartInfo = new ProcessStartInfo {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                }
            };
            // escape command
            command = command.Replace("\"", "\\\"");
            switch (type) {
                case ShellType.Windows: {
                    shellProcess.StartInfo.FileName = which("cmd.exe");
                    shellProcess.StartInfo.Arguments =
                        $"/C {command}";
                    shellProcess.StartInfo.CreateNoWindow = true;
                    break;
                }
                case ShellType.Unix: {
                    shellProcess.StartInfo.FileName = "sh";
                    shellProcess.StartInfo.Arguments =
                        $"-c \"{command}\"";
                    shellProcess.StartInfo.CreateNoWindow = true;
                    break;
                }
                default:
                    throw new PlatformNotSupportedException();
            }

            return shellProcess;
        }

        public static string? which(string fileName, params string[] extraPaths) {
            if (File.Exists(fileName))
                return Path.GetFullPath(fileName);

            var paths = new List<string>(extraPaths);
            var pathVar = Environment.GetEnvironmentVariable("PATH")?.Split(pathDelim);
            if (pathVar != null) {
                paths.AddRange(new List<string>(pathVar));
            }

            foreach (var path in paths) {
                var fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                    return fullPath;
            }

            return null;
        }
    }
}
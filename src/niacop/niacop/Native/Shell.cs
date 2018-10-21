using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace niacop.Native {
    public static class Shell {
        public enum ShellType {
            Generic,
            Windows,
            Unix
        }

        public static ShellType type;
        private static string _pathDelimiter;

        static Shell() {
            switch (Environment.OSVersion.Platform) {
                case PlatformID.Win32NT:
                    type = ShellType.Windows;
                    _pathDelimiter = ";";
                    break;

                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    type = ShellType.Unix;
                    _pathDelimiter = ":";
                    break;
            }
        }

        public struct ExecuteResult {
            public int exitCode { get; set; }
            public string output { get; set; }
            public string errorOutput { get; set; }
        }

        public static Process shellExecute(string command) {
            var shellProc = new Process {
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
                    shellProc.StartInfo.FileName = which("cmd.exe");
                    shellProc.StartInfo.Arguments =
                        $"/C {command}";
                    shellProc.StartInfo.CreateNoWindow = true;
                    break;
                }
                case ShellType.Unix: {
                    shellProc.StartInfo.FileName = "sh";
                    shellProc.StartInfo.Arguments =
                        $"-c \"{command}\"";
                    shellProc.StartInfo.CreateNoWindow = true;
                    break;
                }
                default:
                    throw new PlatformNotSupportedException();
            }

            return shellProc;
        }
        
        public static string which(string fileName, params string[] extraPaths) {
            if (File.Exists(fileName))
                return Path.GetFullPath(fileName);

            var paths = new List<string>(extraPaths);
            paths.AddRange(new List<string>(Environment.GetEnvironmentVariable("PATH")?.Split(_pathDelimiter)));

            foreach (var path in paths) {
                var fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                    return fullPath;
            }

            return null;
        }
    }
}
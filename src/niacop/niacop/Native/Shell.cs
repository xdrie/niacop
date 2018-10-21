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

        public static ExecuteResult executeShellCommand(string commandName, string args) {
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            var exitCode = executeShellCommand(commandName, args,
                (s, e) => {
                    outputBuilder.Append(e.Data);
                },
                (s, e) => {
                    errorBuilder.Append(e.Data);
                },
                false, "");

            return new ExecuteResult() {
                exitCode = exitCode,
                output = outputBuilder.ToString().Trim(),
                errorOutput = errorBuilder.ToString().Trim()
            };
        }

        public static Process launchCommand(string commandName, string args, Action<object, DataReceivedEventArgs>
                outputReceivedCallback, Action<object, DataReceivedEventArgs> errorReceivedCallback = null,
            bool resolveExecutable = true,
            string workingDirectory = "", bool executeInShell = true, bool includeSystemPaths = true,
            params string[] extraPaths) {
            var shellProc = new Process {
                StartInfo = new ProcessStartInfo {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    WorkingDirectory = workingDirectory,
                }
            };

            if (!includeSystemPaths) {
                shellProc.StartInfo.Environment["PATH"] = "";
            }


            foreach (var extraPath in extraPaths) {
                if (extraPath != null) {
                    shellProc.StartInfo.Environment["PATH"] += $"{_pathDelimiter}{extraPath}";
                }
            }

            if (executeInShell) {
                if (type == ShellType.Windows) {
                    shellProc.StartInfo.FileName = resolveFullExecutablePath("cmd.exe");
                    shellProc.StartInfo.Arguments =
                        $"/C {(resolveExecutable ? resolveFullExecutablePath(commandName, true, extraPaths) : commandName)} {args}";
                    shellProc.StartInfo.CreateNoWindow = true;
                } else if (type == ShellType.Unix) {
                    shellProc.StartInfo.FileName = "sh";
                    shellProc.StartInfo.Arguments =
                        $"-c \"{(resolveExecutable ? resolveFullExecutablePath(commandName) : commandName)} {args}\"";
                    shellProc.StartInfo.CreateNoWindow = true;
                }
            } else {
                shellProc.StartInfo.FileName = (resolveExecutable
                    ? resolveFullExecutablePath(commandName, true, extraPaths)
                    : commandName);
                shellProc.StartInfo.Arguments = args;
                shellProc.StartInfo.CreateNoWindow = true;
            }

            shellProc.OutputDataReceived += (s, a) => outputReceivedCallback(s, a);

            if (errorReceivedCallback != null) {
                shellProc.ErrorDataReceived += (s, a) => errorReceivedCallback(s, a);
            }

            shellProc.EnableRaisingEvents = true;

            shellProc.Start();

            shellProc.BeginOutputReadLine();
            shellProc.BeginErrorReadLine();

            return shellProc;
        }

        public static string[] getSystemPaths() {
            switch (type) {
                case ShellType.Unix:
                    var result = executeShellCommand("/bin/bash", "-l -c 'echo $PATH'");
                    return result.output.Split(_pathDelimiter);
                default:
                    return new string[0];
            }
        }


        public static int executeShellCommand(string commandName, string args, Action<object, DataReceivedEventArgs>
                outputReceivedCallback, Action<object, DataReceivedEventArgs> errorReceivedCallback = null,
            bool resolveExecutable = true,
            string workingDirectory = "", bool executeInShell = true, bool includeSystemPaths = true,
            params string[] extraPaths) {
            using (var shellProc = new Process {
                StartInfo = new ProcessStartInfo {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    WorkingDirectory = workingDirectory
                }
            }) {
                if (!includeSystemPaths) {
                    shellProc.StartInfo.Environment["PATH"] = "";
                }

                foreach (var extraPath in extraPaths) {
                    if (extraPath != null) {
                        shellProc.StartInfo.Environment["PATH"] += $"{_pathDelimiter}{extraPath}";
                    }
                }

                if (executeInShell) {
                    if (type == ShellType.Windows) {
                        shellProc.StartInfo.FileName = resolveFullExecutablePath("cmd.exe");
                        shellProc.StartInfo.Arguments =
                            $"/C {(resolveExecutable ? resolveFullExecutablePath(commandName, true, extraPaths) : commandName)} {args}";
                        shellProc.StartInfo.CreateNoWindow = true;
                    } else //Unix
                    {
                        shellProc.StartInfo.FileName = "sh";
                        var commandString = resolveExecutable ? resolveFullExecutablePath(commandName) : commandName;
                        commandString += $" {args}";
                        shellProc.StartInfo.Arguments =
                            $"-c \"{(commandString.Replace("\"", "\\\""))}\"";
                        shellProc.StartInfo.CreateNoWindow = true;
                    }
                } else {
                    shellProc.StartInfo.FileName = (resolveExecutable
                        ? resolveFullExecutablePath(commandName, true, extraPaths)
                        : commandName);
                    shellProc.StartInfo.Arguments = args;
                    shellProc.StartInfo.CreateNoWindow = true;
                }

                shellProc.OutputDataReceived += (s, a) => outputReceivedCallback(s, a);

                if (errorReceivedCallback != null) {
                    shellProc.ErrorDataReceived += (s, a) => errorReceivedCallback(s, a);
                }

                shellProc.Start();

                shellProc.BeginOutputReadLine();
                shellProc.BeginErrorReadLine();

                shellProc.WaitForExit();

                return shellProc.ExitCode;
            }
        }

        /// <summary>
        /// Checks whether a script executable is available in the user's shell
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool checkExecutableAvailability(string fileName, params string[] extraPaths) {
            return resolveFullExecutablePath(fileName, true, extraPaths) != null;
        }

        /// <summary>
        /// Attempts to locate the full path to a script
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string resolveFullExecutablePath(string fileName, bool returnNullOnFailure = true,
            params string[] extraPaths) {
            if (File.Exists(fileName))
                return Path.GetFullPath(fileName);

            if (type == ShellType.Windows) {
                var values = new List<string>(extraPaths);
                values.AddRange(new List<string>(Environment.GetEnvironmentVariable("PATH")?.Split(_pathDelimiter)));

                foreach (var path in values) {
                    var fullPath = Path.Combine(path, fileName);
                    if (File.Exists(fullPath))
                        return fullPath;
                }
            } else {
                //Use the which command
                var outputBuilder = new StringBuilder();
                executeShellCommand("which", $"\"{fileName}\"", (s, e) => {
                    outputBuilder.Append(e.Data);
                }, (s, e) => { }, false);
                var procOutput = outputBuilder.ToString();
                if (string.IsNullOrWhiteSpace(procOutput)) {
                    return returnNullOnFailure ? null : fileName;
                }

                return procOutput.Trim();
            }

            return returnNullOnFailure ? null : fileName;
        }
    }
}
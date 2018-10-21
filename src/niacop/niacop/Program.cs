using System;
using System.IO;
using System.Runtime.Loader;
using System.Threading;
using niacop.Configuration;
using niacop.Native;
using niacop.Services;

namespace niacop {
    class Program {
        public const string CONFIG_FILE_NAME = "niacop.conf";

        static void Main(string[] args) {
            Console.WriteLine("[niacop]");

            if (args.Length < 1) {
                Console.WriteLine("no subcommand specified.");
            }

            // parse options
            var configFilePath = Path.Combine(DataPaths.configBase, CONFIG_FILE_NAME);
            if (File.Exists(configFilePath)) {
                Logger.log($"loading configuration from {configFilePath}", Logger.Level.Trace);
                var configFileContent = File.ReadAllText(configFilePath);
                var optionParser = new OptionParser();
                optionParser.parse(configFileContent);
                Options.load(optionParser);
            }

            var subcommand = args[0];

            switch (subcommand) {
                case "activity":
                    subcommandActivity();
                    break;
                case "book":
                    subcommandBook();
                    break;
            }
        }

        private static void subcommandBook() {
            var bookInteractive = new BookInteractive();
            bookInteractive.initialize();
            bookInteractive.run();
            bookInteractive.destroy();
        }

        private static void subcommandActivity() {
            Console.WriteLine("running activity tracker");
            var activityDaemonTokenSource = new CancellationTokenSource();
            var activityDaemon = new ActivityTracker();

            // prepare exit handler
            var unloadHandler = new Action(() => {
                if (!activityDaemonTokenSource.IsCancellationRequested) {
                    Logger.log("recieved exit signal, cleaning up", Logger.Level.Warning);
                    activityDaemonTokenSource.Cancel();
                    activityDaemon.destroy();
                }
            });

            AssemblyLoadContext.Default.Unloading += (context) => unloadHandler();
            Console.CancelKeyPress += (s, e) => unloadHandler();

            // prepare and run daemon
            activityDaemon.initialize();
            activityDaemon.run(activityDaemonTokenSource.Token);
        }
    }
}
using System;
using System.IO;
using System.Threading;
using niacop.Configuration;
using niacop.Native;
using niacop.Services;

namespace niacop {
    class Program {
        public const string CONFIG_FILE_NAME = "niacop.conf";

        static void Main(string[] args) {
            Console.WriteLine("[niacop]");
            Console.WriteLine("running activity tracker");

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
            }
        }

        private static void subcommandActivity() {
            var activityDaemonTokenSource = new CancellationTokenSource();
            var activityDaemon = new ActivityTracker();

            // prepare exit handler
            AppDomain.CurrentDomain.ProcessExit += (s, e) => {
                Logger.log("recieved exit signal, cleaning up", Logger.Level.Warning);
                activityDaemonTokenSource.Cancel();
                activityDaemon.save();
                activityDaemon.destroy();
            };

            // prepare and run daemon
            activityDaemon.initialize();
            activityDaemon.run(activityDaemonTokenSource.Token);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using niacop.Configuration;
using niacop.Extensibility;
using niacop.Native;
using niacop.Services;

namespace niacop {
    class Program {
        public const string CONFIG_FILE_NAME = "niacop.conf";
        public const string VERSION = "v0.3.3.0";

        static void Main(string[] args) {
            Console.WriteLine($"[niacop] {VERSION}");

            if (args.Length < 1) {
                Console.WriteLine("no subcommand specified.");
                return;
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

            // load plugins
            Logger.log($"loading plugins", Logger.Level.Trace);
            var plugins = new List<INiaPlugin>();
            var defaultPlugins = ExtensibilityService.loader.loadFrom(Assembly.GetExecutingAssembly());
            plugins.AddRange(defaultPlugins);
            foreach (var pluginContainer in Options.plugins) {
                var asm = Assembly.Load(pluginContainer);
                var asmPlugins = ExtensibilityService.loader.loadFrom(asm);
                plugins.AddRange(asmPlugins);
            }

            foreach (var plugin in plugins) {
                // register plugin types
                plugin.beforeActivation(ExtensibilityService.registry);
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using niacop.Configuration;
using niacop.Extensibility;
using niacop.Native;
using niacop.Services;
using HumanDateParser = Chronic.Core.Parser;

namespace niacop {
    class Program {
        public const string CONFIG_FILE_NAME = "niacop.conf";
        public const string VERSION = "0.4.1";

        static int Main(string[] args) {
            Global.log.info($"[niacop] v{VERSION}");

            if (args.Length < 1) {
                Global.log.err("no subcommand specified.");
                return 1;
            }

            // parse options
            var configFilePath = Path.Combine(DataPaths.configBase, CONFIG_FILE_NAME);
            if (File.Exists(configFilePath)) {
                Global.log.trace($"loading configuration from {configFilePath}");
                var configFileContent = File.ReadAllText(configFilePath);
                var config = new Config();
                config.load(configFileContent);
                Global.config = config;
            }
            else {
                Global.log.err($"config file does not exist at {configFilePath}");
                return 2;
            }

            // apply config
            Global.log.verbosity = Global.config.log.verbosity;

            // load plugins
            Global.log.trace($"loading plugins");
            var plugins = new List<INiaPlugin>();
            var defaultPlugins = Services.Extensibility.loader.LoadFrom(Assembly.GetExecutingAssembly());
            plugins.AddRange(defaultPlugins);
            foreach (var pluginContainer in Global.config.ext.paths) {
                var asm = Assembly.Load(pluginContainer);
                var asmPlugins = Services.Extensibility.loader.LoadFrom(asm);
                plugins.AddRange(asmPlugins);
            }

            foreach (var plugin in plugins) {
                // register plugin types
                plugin.BeforeActivation(Services.Extensibility.jar);
            }

            var subcommand = args[0];
            var subArgs = args.Skip(1).ToArray();

            switch (subcommand) {
                case "activity":
                    return subcommandActivity(subArgs);
                case "book":
                    return subcommandBook(subArgs);
                case "timemachine":
                    return subcommandTimeMachine(subArgs);
                default:
                    Global.log.err($"unrecognized subcommand: {subcommand}");
                    return 3;
            }
        }

        private static int subcommandTimeMachine(string[] args) {
            if (args.Length < 1) {
                Global.log.err("missing args");
                return 1;
            }
            // get query datestamp
            var parser = new HumanDateParser();
            var parsedDate = parser.Parse(args[0]);
            var date = parsedDate.ToTime();
            Global.log.info($"query TIME MACHINE at {date}");
            
            return 0;
        }

        private static int subcommandBook(string[] args) {
            var bookInteractive = new BookInteractive();
            bookInteractive.initialize();
            bookInteractive.run();
            bookInteractive.destroy();
            
            return 0;
        }

        private static int subcommandActivity(string[] args) {
            Global.log.info("running activity tracker");
            var cts = new CancellationTokenSource();
            var activityDaemon = new ActivityTracker();

            // prepare exit handler
            var unloadHandler = new Action(() => {
                if (!cts.IsCancellationRequested) {
                    Global.log.err("recieved exit signal, cleaning up");
                    cts.Cancel();
                    activityDaemon.destroy();
                }
            });

            AssemblyLoadContext.Default.Unloading += (context) => unloadHandler();
            Console.CancelKeyPress += (s, e) => unloadHandler();

            // prepare and run daemon
            activityDaemon.initialize();
            activityDaemon.run(cts.Token);

            return 0;
        }
    }
}
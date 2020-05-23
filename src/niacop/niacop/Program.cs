﻿using System;
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
        public const string VERSION = "0.4.0";

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

            switch (subcommand) {
                case "activity":
                    subcommandActivity();
                    break;
                case "book":
                    subcommandBook();
                    break;
            }

            return 0;
        }

        private static void subcommandBook() {
            var bookInteractive = new BookInteractive();
            bookInteractive.initialize();
            bookInteractive.run();
            bookInteractive.destroy();
        }

        private static void subcommandActivity() {
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
        }
    }
}
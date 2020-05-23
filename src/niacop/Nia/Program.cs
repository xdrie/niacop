using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CommandLine;
using Nia.CLI;
using Nia.Extensibility;
using Nia.Native;
using CLIParser = CommandLine.Parser;

namespace Nia {
    class Program {
        public static int Main(string[] args) {
            // parse config file
            var configFilePath = Path.Combine(DataPaths.configBase, Config.CONFIG_FILE);
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

            // load plugins
            var plugins = new List<INiaPlugin>();
            var defaultPlugins = Services.Extensibility.loader.LoadFrom(Assembly.GetExecutingAssembly());
            plugins.AddRange(defaultPlugins);
            foreach (var pluginContainer in Global.config!.ext.paths) {
                Global.log.trace($"loading plugins from {pluginContainer}");
                var asm = Assembly.Load(pluginContainer);
                var asmPlugins = Services.Extensibility.loader.LoadFrom(asm);
                plugins.AddRange(asmPlugins);
            }

            foreach (var plugin in plugins) {
                // register plugin types
                plugin.BeforeActivation(Services.Extensibility.jar);
            }

            var parserResult =
                CLIParser.Default.ParseArguments(args, new[] {
                    typeof(ActivityDaemonRunner.Options),
                    typeof(BookRunner.Options),
                    typeof(TimeMachineRunner.Options),
                    typeof(LastRunner.Options)
                });
            var parsedArgs = (parserResult as Parsed<object>)?.Value;
            switch (parsedArgs) {
                case ActivityDaemonRunner.Options options: {
                    using var runner = new ActivityDaemonRunner();
                    return runner.run(options);
                }
                case TimeMachineRunner.Options options: {
                    using var runner = new TimeMachineRunner();
                    return runner.run(options);
                }
                case BookRunner.Options options: {
                    using var runner = new BookRunner();
                    return runner.run(options);
                }
                case LastRunner.Options options: {
                    using var runner = new LastRunner();
                    return runner.run(options);
                }
                default:
                    return -1;
            }
        }
    }
}
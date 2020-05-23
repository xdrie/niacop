using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CommandLine;
using niacop.CLI;
using niacop.Extensibility;
using niacop.Native;
using niacop.Services;
using CLIParser = CommandLine.Parser;

namespace niacop {
    class Program {
        public class Options { }

        public static int Main(string[] args) {
            Global.log.info($"niacop v{Config.VERSION}");

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
                    typeof(ActivityDaemonRunner.Options)
                });
            var parsedArgs = (parserResult as Parsed<object>)?.Value;
            switch (parsedArgs) {
                case ActivityDaemonRunner.Options options:
                    return new ActivityDaemonRunner().run(options);
                case TimeMachineRunner.Options options:
                    return new TimeMachineRunner().run(options);
                case BookRunner.Options options:
                    return new BookRunner().run(options);
                default:
                    return 1;
            }
        }
    }
}
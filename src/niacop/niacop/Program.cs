using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using niacop.Extensibility;
using niacop.Models;
using niacop.Native;
using niacop.Services;
using HumanDateParser = Chronic.Core.Parser;

namespace niacop {
    class Program {
        public const string CONFIG_FILE_NAME = "niacop.conf";
        public const string VERSION = "0.4.2";

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
            } else {
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

        private static ActivityTracker createTracker() {
            var tracker = new ActivityTracker();
            tracker.initialize();

            return tracker;
        }

        private static int subcommandTimeMachine(string[] args) {
            if (args.Length < 1) {
                Global.log.err("missing args");
                return 1;
            }

            // get query datestamp
            var parser = new HumanDateParser();
            var parsedDate = parser.Parse(args[0]);
            var queryDate = parsedDate.ToTime();
            Global.log.info($"query TIME MACHINE at {queryDate}");

            // convert time to a stamp to query
            var queryDateOffset = (DateTimeOffset) DateTime.SpecifyKind(queryDate, DateTimeKind.Local);
            var queryTimestamp = queryDateOffset.ToUnixTimeMilliseconds();

            var tracker = createTracker();

            // find candidates
            var sessionTable = tracker.database.Table<Session>();

            // we want any sessions that BOTH
            // - start before
            // - end after
            var matchedSessions = sessionTable.Where(x =>
                    x.startTime <= queryTimestamp && x.endTime >= queryTimestamp)
                .ToList();
            
            var preciseSess = matchedSessions.FirstOrDefault();
            if (preciseSess != null) {
                // precise matches
                Global.log.info($"found {matchedSessions.Count} precise match.");
                // print the session nicely
                Global.log.info(preciseSess.prettyFormat());
            } else {
                // find close matches
                Global.log.info($"no precise matches found.");
                // find session immediately before and after (2x 12h range)
                var beforeCutoff = queryDateOffset.AddHours(-12).ToUnixTimeMilliseconds();
                var beforeSessions = sessionTable.Where(x =>
                    x.startTime >= beforeCutoff
                    && x.endTime <= queryTimestamp);
                var afterCutoff = queryDateOffset.AddHours(12).ToUnixTimeMilliseconds();
                var afterSessions = sessionTable.Where(x =>
                    x.startTime >= queryTimestamp
                    && x.endTime <= afterCutoff);
                var immBefore = beforeSessions.LastOrDefault();
                var immAfter = afterSessions.FirstOrDefault();

                // compare their distance, and take the closest one
                var closest = default(Session);
                var closestDist = 0L;
                if (immBefore == null && immAfter == null) {
                    Global.log.info($"no sessions found within 12 hours of requested time.");
                } else {
                    var beforeDist = Math.Abs((queryTimestamp - immBefore?.endTime) ?? long.MaxValue);
                    var afterDist = Math.Abs((queryTimestamp - immAfter?.startTime) ?? long.MaxValue);
                    // pick the closer one
                    if (beforeDist <= afterDist) {
                        closest = immBefore;
                        closestDist = beforeDist;
                    } else {
                        closest = immAfter;
                        closestDist = afterDist;
                    }

                    // show closest session
                    var distTimespan = TimeSpan.FromMilliseconds(closestDist);
                    Global.log.info($"found closest session ({distTimespan}) away.");
                    Global.log.info(closest.prettyFormat());
                }
            }
            
            // summarize that time period (1 hour)
            var periodStart = queryDateOffset.AddHours(-0.5).ToUnixTimeMilliseconds();
            var periodEnd = queryDateOffset.AddHours(0.5).ToUnixTimeMilliseconds();
            var aroundSessions = sessionTable.Where(x => 
                    x.startTime >= periodStart && x.endTime <= periodEnd)
                .ToList();
            // get the top-N apps
            var topN = 8;

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
            var activityDaemon = createTracker();

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
            activityDaemon.run(cts.Token);

            return 0;
        }
    }
}
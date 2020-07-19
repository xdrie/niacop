using System;
using System.Linq;
using CommandLine;
using Humanizer;
using Nia.Models;
using Nia.Services;

namespace Nia.CLI {
    public class LastRunner : Runner<LastRunner.Options> {
        [Verb("last", HelpText = "Query last interaction with an application.")]
        public class Options {
            [Value(0, Required = true, MetaName = "app", HelpText = "The application for which to query sessions.")]
            public string? application { get; set; }

            [Option('z', "fuzz", Required = false, Default = false, HelpText = "Whether to fuzzy search sessions.")]
            public bool fuzz { get; set; }
        }

        public override int run(Options options) {
            Global.log.info($"query LAST SESSION for: {options.application}");

            var tracker = new ActivityTracker();
            tracker.initialize();

            // find candidates
            var sessionTable = tracker.database!.Table<Session>();

            var normQuery = options.application!.ToLower();
            var matchedSessions = sessionTable.Where(x => x.application!.ToLower().Contains(normQuery));
            if (!matchedSessions.Any()) {
                Global.log.warn("no matches found by application, searching by title");
                matchedSessions = sessionTable.Where(x => x.windowTitle!.ToLower().Contains(normQuery));
            }

            if (options.fuzz && !matchedSessions.Any()) {
                Global.log.warn("no matches found by application, fuzzy searching all");
                throw new NotImplementedException();
            }

            if (matchedSessions.Any()) {
                Global.log.info($"found {matchedSessions.Count()} matching entries.");
                var lastSess = matchedSessions.Last();
                // print the session nicely
                var distTimespan = DateTime.Now - Utils.timestampToLocal(lastSess.endTime);
                Global.log.info($"most recent session ({distTimespan.Humanize(2)} ago):\n{lastSess.prettyFormat()}");
            }
            else {
                Global.log.err("no matching sessions.");
            }
            
            return 0;
        }
    }
}
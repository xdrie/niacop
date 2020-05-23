using System.Linq;
using CommandLine;
using Nia.Models;
using Nia.Services;

namespace Nia.CLI {
    public class LastRunner : Runner<LastRunner.Options> {
        [Verb("last", HelpText = "Query last interaction with an application.")]
        public class Options {
            [Value(0, Required = true, MetaName = "app", HelpText = "The application for which to query sessions.")]
            public string? application { get; set; }
        }

        public override int run(Options options) {
            Global.log.info($"query LAST SESSION for: {options.application}");

            var tracker = new ActivityTracker();
            tracker.initialize();

            // find candidates
            var sessionTable = tracker.database!.Table<Session>();

            var normQuery = options.application!.ToLower();
            var matchedSessions = sessionTable.Where(x => x.application!.ToLower().Contains(normQuery));

            if (matchedSessions.Any()) {
                Global.log.info($"found {matchedSessions.Count()} matching entries.");
                var lastSess = matchedSessions.Last();
                // print the session nicely
                Global.log.info($"most recent session:\n{lastSess.prettyFormat()}");
            }
            else {
                Global.log.err("no matching sessions.");
            }
            
            return 0;
        }
    }
}
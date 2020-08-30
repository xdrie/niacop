using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using FuzzySharp;
using Nia.Models;
using Nia.Services;

namespace Nia.CLI {
    public class LastRunner : Runner<LastRunner.Options> {
        [Verb("last", HelpText = "Query last interaction with an application.")]
        public class Options {
            [Value(0, Required = true, MetaName = "app", HelpText = "The application for which to query sessions.")]
            public string? query { get; set; }

            [Option('z', "fuzz", Required = false, Default = false, HelpText = "Whether to fuzzy search sessions.")]
            public bool fuzz { get; set; }

            [Option('n', "entries", Required = false, HelpText = "The number of recent entries to find.")]
            public int lastN { get; set; } = 1;

            [Option('s', "skip", Required = false, HelpText = "The number of entries to skip.")]
            public int skipN { get; set; } = 0;
        }

        public override int run(Options options) {
            Global.log.info($"query LAST SESSION for: {options.query}");

            var tracker = new ActivityTracker();
            tracker.initialize();

            // find candidates
            var sessionTable = tracker.database!.Table<Session>();

            var query = options.query!.ToLower();
            IEnumerable<Session> matchedSessions = sessionTable.Where(x => x.application!.ToLower().Contains(query));
            if (!matchedSessions.Any()) {
                Global.log.warn("no matches found by application, searching by title");
                matchedSessions = sessionTable.Where(x => x.windowTitle!.ToLower().Contains(query));
            }

            if (options.fuzz && !matchedSessions.Any()) {
                Global.log.warn("no matches found by application, fuzzy searching all");
                // get fuzzy match scores for all sessions
                var threshold = 50;
                var matches = new List<(Session, int)>();
                foreach (var session in sessionTable) {
                    var appRatio = Fuzz.WeightedRatio(query, session.application);
                    var titleRatio = Fuzz.WeightedRatio(query, session.windowTitle);
                    var sessRatio = Math.Max(appRatio, titleRatio);
                    if (sessRatio > threshold) {
                        matches.Add((session, sessRatio));
                    }
                }

                // sort to get best matches
                var sortedMatches = matches.OrderByDescending(x => x.Item2);
                Global.log.warn($"best fuzzy match: {sortedMatches.First().Item2}%");
                matchedSessions = sortedMatches.Select(x => x.Item1).ToList();
            }

            if (matchedSessions.Any()) {
                Global.log.info(
                    $"found {matchedSessions.Count()} matching entries (displaying {options.lastN} starting at pos {options.skipN}).");
                // get a range of the sessions with most recent first
                var listedSessions = matchedSessions.OrderByDescending(x => x.endTime);
                foreach (var sess in listedSessions.Skip(0).Take(options.lastN)) {
                    // print the session nicely
                    var distTimespan = DateTime.Now - Utils.timestampToLocal(sess.endTime);
                    Global.log.info($"session ({Utils.timeAgo(distTimespan)}):\n{sess.prettyFormat()}");
                }
            }
            else {
                Global.log.err("no matching sessions.");
            }

            return 0;
        }
    }
}
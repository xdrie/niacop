using System;
using System.Collections.Generic;
using CommandLine;
using Nia.Services;
using Nia.Util;
using HumanDateParser = Chronic.Core.Parser;

namespace Nia.CLI {
    public class SummaryRunner : Runner<SummaryRunner.Options> {
        [Verb("summary", HelpText = "show a summary of activity categories.")]
        public class Options {
            [Option('p', "period", Required = false,
                HelpText = "The period of time (in days) to show usage summary for.")]
            public float period { get; set; } = 7f; // days
        }

        public override int run(Options options) {
            var startDateOffset = DateTimeOffset.Now - TimeSpan.FromDays(options.period);

            var tracker = new ActivityTracker();
            tracker.initialize();
            var tagger = new ActivityTagger(tracker);

            // tag sessions
            var stats = tagger.tagAllSessions(startDateOffset);

            var tagged = tagger.timePerTag;
            var tagTime = new List<(string, TimeSpan)>();
            // configured tags
            foreach (var tag in Global.config.tracker.tags) {
                if (tagged.ContainsKey(tag.name)) {
                    tagTime.Add((tag.name, tagged[tag.name]));
                }
            }

            // now unknown tag
            tagTime.Add((ActivityTagger.UNKNOWN_TAG, tagged[ActivityTagger.UNKNOWN_TAG]));

            // print fancy summary
            var printer = new ReportPrinter();
            printer.header("NIACOP", "SUMMARY MODE");
            printer.header(
                $"{(int) options.period} DAYS // {stats.SessionCount} SESSIONS // TOTAL {FormatHelper.formatTimeHM(stats.TotalTime)}");
            printer.line();
            printer.header("TOTAL TIME PER AREA");
            printer.line();
            var graphData = new List<(string, long)>();
            foreach (var (tag, time) in tagTime) {
                graphData.Add(($"{tag} [{FormatHelper.formatTimeHM(time)}]", (long) time.TotalMilliseconds));
            }
            printer.ratioGraph(graphData);
            printer.line();

            return 0;
        }
    }
}
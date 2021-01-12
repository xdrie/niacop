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
            var tagResult = tagger.tagAllSessions(startDateOffset, DateTimeOffset.Now);

            // condense the dictionary into a list of time-tagged categories
            var categoryTimeList = new List<(string, TimeSpan)>();
            // configured tags
            foreach (var tag in Global.config.tracker.tags) {
                if (tagResult.TimePerTag.ContainsKey(tag.name)) {
                    categoryTimeList.Add((tag.name, tagResult.TimePerTag[tag.name]));
                }
            }

            // now unknown tag
            categoryTimeList.Add((ActivityTagger.UNKNOWN_TAG, tagResult.TimePerTag[ActivityTagger.UNKNOWN_TAG]));

            // print fancy summary
            var printer = new ReportPrinter();
            printer.header("NIACOP", "SUMMARY MODE");
            printer.header(
                $"{(int) options.period} DAYS // {tagResult.SessionCount} SESSIONS // TOTAL {FormatHelper.formatTimeHM(tagResult.TotalTime)}");
            printer.line();
            printer.header("TOTAL TIME PER AREA");
            printer.line();
            var graphData = new List<(string, long)>();
            foreach (var (tag, time) in categoryTimeList) {
                graphData.Add(($"{tag} [{FormatHelper.formatTimeHM(time)}]", (long) time.TotalMilliseconds));
            }
            printer.ratioGraph(graphData);
            printer.line();

            return 0;
        }
    }
}
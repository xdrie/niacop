using System;
using System.Collections.Generic;
using CommandLine;
using Nia.Analysis;
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

            var summarizer = new SessionRangeSummarizer(tracker);
            var (tagResult, graphData) = summarizer.summarizeSessionRange(startDateOffset, DateTimeOffset.Now);

            summarizer.printSummaryReport(graphData,
                $"{(int) options.period} DAYS // {tagResult.SessionCount} SESSIONS // TOTAL {FormatHelper.formatTimeHM(tagResult.TotalTime)}",
                "SUMMARY MODE");

            return 0;
        }
    }
}
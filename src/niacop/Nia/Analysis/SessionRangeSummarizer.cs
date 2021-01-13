using System;
using System.Collections.Generic;
using Nia.Util;

namespace Nia.Services {
    public class SessionRangeSummarizer {
        private readonly ActivityTracker tracker;
        private SessionTagger tagger;

        public SessionRangeSummarizer(ActivityTracker tracker) {
            this.tracker = tracker;
            tagger = new SessionTagger(tracker);
        }

        public (SessionTagger.TaggedSessionResults tagResult, List<(string, long)> graphData) summarizeSessionRange(DateTimeOffset startDate, DateTimeOffset endDate) {
            // tag sessions
            var tagResult = tagger.tagAllSessions(startDate, endDate);

            // condense the dictionary into a list of time-tagged categories
            var categoryTimeList = new List<(string, TimeSpan)>();
            // configured tags
            foreach (var tag in Global.config!.tracker.tags) {
                if (tagResult.TimePerTag.ContainsKey(tag.name)) {
                    categoryTimeList.Add((tag.name, tagResult.TimePerTag[tag.name]));
                }
            }

            // now unknown tag
            categoryTimeList.Add((SessionTagger.UNKNOWN_TAG, tagResult.TimePerTag[SessionTagger.UNKNOWN_TAG]));

            // generate graph data
            var graphData = new List<(string, long)>();
            foreach (var (tag, time) in categoryTimeList) {
                graphData.Add(($"{tag} [{FormatHelper.formatTimeHM(time)}]", (long) time.TotalMilliseconds));
            }

            return (tagResult, graphData);
        }

        public void printSummaryReport(List<(string, long)> graphData, string title, string subtitle) {
            // print fancy summary
            var printer = new ReportPrinter();
            printer.header("NIACOP", subtitle);
            printer.header(title);
            printer.line();
            printer.header("TOTAL TIME PER AREA");
            printer.line();
            printer.ratioGraph(graphData);
            printer.line();
        }
    }
}
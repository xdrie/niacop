using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using niacop.Models;
using niacop.Services;
using HumanDateParser = Chronic.Core.Parser;

namespace niacop.CLI {
    public class TimeMachineRunner : Runner<TimeMachineRunner.Options> {
        [Verb("timemachine", HelpText = "access the Time Machine.")]
        public class Options {
            [Value(0, MetaName = "query", HelpText = "Time query.")]
            public string? timeQuery { get; set; }
        }

        public override int run(Options options) {
            // get query datestamp
            var parser = new HumanDateParser();
            var parsedDate = parser.Parse(options.timeQuery);
            if (parsedDate == null) {
                Global.log.err($"could not understand time query: {options.timeQuery}");
                return 2;
            }

            var queryDate = parsedDate.ToTime();
            Global.log.info($"query TIME MACHINE at {queryDate}");

            // convert time to a stamp to query
            var queryDateOffset = (DateTimeOffset) DateTime.SpecifyKind(queryDate, DateTimeKind.Local);
            var queryTimestamp = queryDateOffset.ToUnixTimeMilliseconds();

            var tracker = new ActivityTracker();
            tracker.initialize();

            // find candidates
            var sessionTable = tracker.database!.Table<Session>();

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
            }
            else {
                // find close matches
                Global.log.info($"no precise matches found.");
                // find session immediately before and after (within range)
                var roughMatchDist = Global.config!.timeMachine.roughMatchRange / 2f;
                var beforeCutoff = queryDateOffset.AddHours(-roughMatchDist).ToUnixTimeMilliseconds();
                var beforeSessions = sessionTable.Where(x =>
                    x.startTime >= beforeCutoff
                    && x.endTime <= queryTimestamp);
                var afterCutoff = queryDateOffset.AddHours(roughMatchDist).ToUnixTimeMilliseconds();
                var afterSessions = sessionTable.Where(x =>
                    x.startTime >= queryTimestamp
                    && x.endTime <= afterCutoff);
                var immBefore = beforeSessions.LastOrDefault();
                var immAfter = afterSessions.FirstOrDefault();

                // compare their distance, and take the closest one
                var closest = default(Session);
                var closestDist = 0L;
                if (immBefore == null && immAfter == null) {
                    Global.log.info(
                        $"no sessions found within {Global.config!.timeMachine.roughMatchRange:F0} hours of requested time.");
                }
                else {
                    var beforeDist = Math.Abs((queryTimestamp - immBefore?.endTime) ?? long.MaxValue);
                    var afterDist = Math.Abs((queryTimestamp - immAfter?.startTime) ?? long.MaxValue);
                    // pick the closer one
                    if (beforeDist <= afterDist) {
                        closest = immBefore;
                        closestDist = beforeDist;
                    }
                    else {
                        closest = immAfter;
                        closestDist = afterDist;
                    }

                    // show closest session
                    var distTimespan = TimeSpan.FromMilliseconds(closestDist);
                    Global.log.info($"found closest session {distTimespan:hh\\:mm\\:ss} away.");
                    Global.log.info(closest!.prettyFormat());
                }
            }

            // summarize that time period (1 hour)
            var periodStartDate = queryDateOffset.AddHours(-Global.config!.timeMachine.period / 2);
            var periodStart = periodStartDate.ToUnixTimeMilliseconds();
            var periodEndDate = queryDateOffset.AddHours(Global.config!.timeMachine.period / 2);
            var periodEnd = periodEndDate.ToUnixTimeMilliseconds();

            var aroundSessions = sessionTable.Where(x =>
                    x.startTime >= periodStart && x.endTime <= periodEnd)
                .ToList();

            // create usages
            var usages = new Dictionary<string, AppUsage>();
            foreach (var sess in aroundSessions) {
                if (!usages.ContainsKey(sess.application!)) {
                    usages[sess.application!] = new AppUsage(sess.application!);
                }

                // update entry
                usages[sess.application!].time += sess.endTime - sess.startTime;
                usages[sess.application!].keyEvents += sess.keyEvents;
            }

            // get the top-N usages
            var topUsages = usages.Values.OrderByDescending(x => x.time)
                .Take(Global.config!.timeMachine.topN);

            var sb = new StringBuilder();
            sb.AppendLine(
                $"usage summary (top {Global.config!.timeMachine.topN} within {Global.config!.timeMachine.period:F0}h) from {periodStartDate.DateTime:HH:mm}-{periodEndDate.DateTime:HH:mm}");
            foreach (var usage in topUsages) {
                var humanTime = TimeSpan.FromMilliseconds(usage.time);
                sb.AppendLine(
                    $"{usage.application,-32} {humanTime,6:mm\\:ss} // {Utils.formatNumberSI(usage.keyEvents),8:F0} keys");
            }

            Global.log.info(sb.ToString());

            return 0;
        }
    }
}
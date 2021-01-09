using System;
using System.Collections.Generic;
using Nia.Models;
using Nia.Util;
using SQLite;

namespace Nia.Services {
    public class ActivityTagger {
        private readonly ActivityTracker tracker;
        private TableQuery<Session> sessionTable;
        public Dictionary<string, TimeSpan> timePerTag { get; } = new();

        public const string UNKNOWN_TAG = "OTHR";

        public ActivityTagger(ActivityTracker tracker) {
            this.tracker = tracker;
            sessionTable = tracker.database!.Table<Session>();
        }

        public record TagStats {
            public int SessionCount { get; init; }
            public TimeSpan TotalTime { get; init; }
        }

        public TagStats tagAllSessions(DateTimeOffset startDate) {
            var startTimestamp = startDate.ToUnixTimeMilliseconds();
            // filter sessions by date
            var sessions = sessionTable.Where(x =>
                x.startTime >= startTimestamp).ToList();

            // start building
            addTime(UNKNOWN_TAG, 0);

            // get tags
            var tags = Global.config.tracker.tags;

            var totalTime = 0L;
            var sessCount = 0;

            foreach (var sess in sessions) {
                sessCount++;
                var sessDur = sess.getDuration();
                totalTime += sessDur;
                // check tags in order for matches
                bool foundMatch = false;
                foreach (var tag in tags) {
                    foreach (var pattern in tag.match) {
                        if (matchesSession(pattern, sess)) {
                            // this session matched
                            addTime(tag.name, sessDur);
                            foundMatch = true;
                            break;
                        }
                    }

                    if (foundMatch) break;
                }

                if (foundMatch) continue;

                // no matches, add to unknown
                addTime(UNKNOWN_TAG, sessDur);
            }

            return new TagStats {SessionCount = sessCount, TotalTime = TimeSpan.FromMilliseconds(totalTime)};
        }

        private void addTime(string tag, long time) {
            if (!timePerTag.ContainsKey(tag)) timePerTag[tag] = TimeSpan.Zero;
            timePerTag[tag] += TimeSpan.FromMilliseconds(time);
        }

        private bool matchesSession(string pattern, Session sess) {
            bool match = false;
            Func<string, string, bool> matcher;
            if (Wildcard.isRaw(pattern)) {
                matcher = (p, s) => s.Contains(p);
            } else {
                matcher = Wildcard.match;
            }

            // check all fields
            match = match || matcher(pattern, sess.application ?? "");
            match = match || matcher(pattern, sess.processName ?? "");

            return match;
        }
    }
}
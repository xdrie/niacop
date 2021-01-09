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

        public void tagAllSessions(DateTimeOffset startDate) {
            var startTimestamp = startDate.ToUnixTimeMilliseconds();
            // filter sessions by date
            var sessions = sessionTable.Where(x =>
                x.startTime >= startTimestamp).ToList();

            // start building
            addTime(UNKNOWN_TAG, 0);

            // get tags
            var tags = Global.config.tracker.tags;

            foreach (var sess in sessions) {
                var sessDur = sess.getDuration();
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
        }

        private void addTime(string tag, long time) {
            if (!timePerTag.ContainsKey(tag)) timePerTag[tag] = TimeSpan.Zero;
            timePerTag[tag] += TimeSpan.FromMilliseconds(time);
        }

        private bool matchesSession(string pattern, Session sess) {
            bool match = false;
            if (Wildcard.isRaw(pattern)) {
                match = sess.application?.Contains(pattern) ?? false;
            } else {
                match = Wildcard.match(pattern, sess.application ?? string.Empty);
            }

            return match;
        }
    }
}
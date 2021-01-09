using System;
using System.Collections.Generic;
using Iri.Glass.Config;
using Iri.Glass.Logging;
using Tomlyn.Model;

namespace Nia {
    public class Config : TomlConfig {
        public const string CONFIG_FILE = "niacop.conf";

        public class Profile {
            public string name = "default";
        }

        public Profile profile = new();

        public class Tracker {
            /// <summary>
            /// tracker idle time in ms
            /// </summary>
            public int idle = (int) TimeSpan.FromMinutes(5).TotalMilliseconds;

            /// <summary>
            /// tracker polling interval in ms
            /// </summary>
            public int poll = (int) TimeSpan.FromSeconds(2).TotalMilliseconds;

            /// <summary>
            /// whether to count keystrokes
            /// </summary>
            public bool keycounter = false;

            public List<TagRule> tags = new();

            public class TagRule {
                public string name;
                public List<string> match = new();
            }
        }

        public Tracker tracker = new();

        public class Book {
            public int browseEntries = 10;
        }

        public Book book = new();

        public class Log {
            public Logger.Verbosity verbosity = Logger.Verbosity.Information;
        }

        public Log log = new();

        public class Ext {
            public List<string> paths = new();
        }

        public Ext ext = new();

        protected override void load(TomlTable tb) {
            var profileTb = tb.getTable(nameof(profile));
            profileTb.autoBind(profile);

            var trackerTb = tb.getTable(nameof(tracker));
            trackerTb.autoBind(tracker);

            var bookTb = tb.getTable(nameof(book));
            bookTb.autoBind(book);

            var logTb = tb.getTable(nameof(log));
            logTb.autoBind(log);

            var extTb = tb.getTable(nameof(ext));
            extTb.autoBind(ext);

            // apply config
            Global.log.verbosity = log.verbosity;
        }
    }
}
using System;
using System.Collections.Generic;
using Iri.Glass.Config;
using Tomlyn.Model;

namespace niacop.Configuration {
    public class Config : TomlConfig {
        public class Profile {
            public string name = "default";
        }

        public Profile profile;

        public class Tracker {
            public int idle = (int) TimeSpan.FromMinutes(5).TotalMilliseconds;
            public int windowPoll = (int) TimeSpan.FromSeconds(2).TotalMilliseconds;
            public bool keylogger = false;
        }

        public Tracker tracker;

        public class Book {
            public int browseEntries = 10;
        }

        public Book book;

        public class Ext {
            public List<string> paths = new List<string>();
        }

        public Ext ext;

        protected override void load(TomlTable tb) {
            var profileTb = tb.getTable(nameof(profile));
            profileTb.autoBind(profile);

            var trackerTb = tb.getTable(nameof(tracker));
            trackerTb.autoBind(tracker);
            
            var bookTb = tb.getTable(nameof(book));
            trackerTb.autoBind(book);
            
            var extTb = tb.getTable(nameof(ext));
            trackerTb.autoBind(ext);
        }
    }
}
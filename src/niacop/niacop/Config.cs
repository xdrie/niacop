using System;
using System.Collections.Generic;
using Iri.Glass.Config;
using Iri.Glass.Logging;
using Tomlyn.Model;

namespace niacop {
    public class Config : TomlConfig {
        public const string CONFIG_FILE = "niacop.conf";
        
        public class Profile {
            public string name = "default";
        }

        public Profile profile = new Profile();

        public class Tracker {
            public int idle = (int) TimeSpan.FromMinutes(5).TotalMilliseconds;
            public int windowPoll = (int) TimeSpan.FromSeconds(2).TotalMilliseconds;
            public bool keylogger = false;
        }

        public Tracker tracker = new Tracker();

        public class TimeMachine {
            public float period = 1f; // hours
            public float roughMatchRange = 24f; // hours
            public int topN = 8; // top entry count
        }
        public TimeMachine timeMachine = new TimeMachine();

        public class Book {
            public int browseEntries = 10;
        }

        public Book book = new Book();

        public class Log {
            public Logger.Verbosity verbosity = Logger.Verbosity.Information;
        }

        public Log log = new Log();

        public class Ext {
            public List<string> paths = new List<string>();
        }

        public Ext ext = new Ext();

        protected override void load(TomlTable tb) {
            var profileTb = tb.getTable(nameof(profile));
            profileTb.autoBind(profile);

            var trackerTb = tb.getTable(nameof(tracker));
            trackerTb.autoBind(tracker);
            
            var timeMachineTb = tb.getTable(nameof(timeMachine));
            timeMachineTb.autoBind(timeMachine);
            
            var bookTb = tb.getTable(nameof(book));
            bookTb.autoBind(book);
            
            var extTb = tb.getTable(nameof(ext));
            extTb.autoBind(ext);
            
            var logTb = tb.getTable(nameof(log));
            logTb.autoBind(log);
            
            // apply config
            Global.log.verbosity = log.verbosity;
        }
    }
}
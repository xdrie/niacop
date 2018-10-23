using niacop.Extensibility.Tracker;
using niacop.Native;
using niacop.Services;
using SQLite;

namespace niacop.Plugins.Tracker {
    public class WindowTitleEventLogger : SessionEventLogger {
        private string lastTitle;

        public class WindowTitleEvent {
            [PrimaryKey, AutoIncrement]
            public int id { get; set; }

            public long timestamp { get; set; }
            public string title { get; set; }

            public int sessionId { get; set; }
        }

        public override void initialize(SQLiteConnection database) {
            base.initialize(database);

            database.CreateTable<WindowTitleEvent>();
        }

        public override void update(SessionContext sc) {
            if (sc.window.title != lastTitle) {
                // log new title event
                Logger.log("        <ev [title change]", Logger.Level.Trace);
                database.Insert(new WindowTitleEvent {
                    timestamp = Platform.timestamp(),
                    title = sc.window.title,
                    sessionId = sc.session.id
                });
            }

            lastTitle = sc.window.title;
        }
    }
}
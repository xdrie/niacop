using Nia.Extensibility;
using Nia.Models;
using SQLite;

namespace Nia.Plugins {
    public class WindowTitleWatcher : SessionWatcher {
        private string? lastTitle;

        public class WindowTitleEvent {
            [PrimaryKey, AutoIncrement]
            public int id { get; set; }
            public long timestamp { get; set; }
            public string? title { get; set; }
            public int sessionId { get; set; }
        }

        public override void initialize(SQLiteConnection database) {
            base.initialize(database);
            database.CreateTable<WindowTitleEvent>();
        }

        public override void update(SessionContext sc) {
            if (sc.window.title != lastTitle) {
                // log new title event
                sc.log("win_title", "updated");
                database!.Insert(new WindowTitleEvent {
                    timestamp = Utils.timestamp(),
                    title = sc.window.title,
                    sessionId = sc.session.id
                });
            }

            lastTitle = sc.window.title;
        }
    }
}
using Nia.Models;
using SQLite;

namespace Nia.Extensibility {
    public interface ISessionWatcher {
        void initialize(SQLiteConnection database);
        void update(SessionContext sc);
    }

    public abstract class SessionWatcher : ISessionWatcher {
        protected SQLiteConnection? database;

        public virtual void initialize(SQLiteConnection database) {
            this.database = database;
        }

        public abstract void update(SessionContext sc);
    }
}
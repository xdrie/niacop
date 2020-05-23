using SQLite;

namespace Nia.Extensibility.Tracker {
    public interface ISessionEventLogger {
        void initialize(SQLiteConnection database);
        void update(SessionContext sc);
    }

    public abstract class SessionEventLogger : ISessionEventLogger {
        protected SQLiteConnection? database;

        public virtual void initialize(SQLiteConnection database) {
            this.database = database;
        }

        public abstract void update(SessionContext sc);
    }
}
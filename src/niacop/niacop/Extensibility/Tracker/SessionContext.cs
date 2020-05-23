using niacop.Models;
using niacop.Native.WindowManagers;
using niacop.Services;

namespace niacop.Extensibility.Tracker {
    public class SessionContext {
        public Session session;
        public Window window;

        public SessionContext(Session session, Window window) {
            this.session = session;
            this.window = window;
        }
    }
}
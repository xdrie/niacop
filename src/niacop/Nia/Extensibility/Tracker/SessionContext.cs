using Nia.Models;
using Nia.Native.WindowManagers;
using Nia.Services;

namespace Nia.Extensibility.Tracker {
    public class SessionContext {
        public Session session;
        public Window window;

        public SessionContext(Session session, Window window) {
            this.session = session;
            this.window = window;
        }
    }
}
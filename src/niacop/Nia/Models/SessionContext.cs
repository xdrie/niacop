using Nia.Native.WindowManagers;

namespace Nia.Models {
    public class SessionContext {
        public Session session;
        public Window window;

        public SessionContext(Session session, Window window) {
            this.session = session;
            this.window = window;
        }
    }
}
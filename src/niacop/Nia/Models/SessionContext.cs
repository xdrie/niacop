using Nia.Native;

namespace Nia.Models {
    public struct SessionContext {
        public Session session;
        public Window window;

        public SessionContext(Session session, Window window) {
            this.session = session;
            this.window = window;
        }

        public void log(string source, string message) {
            Global.log.trace($"  [{source}] {message}");
        }
    }
}
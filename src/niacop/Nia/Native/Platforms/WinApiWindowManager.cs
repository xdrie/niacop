using System;

namespace Nia.Native.WindowManagers {
    public class WinApiWindowManager : IWindowManager {
        public void initialize() {
            throw new NotImplementedException();
        }

        public void deinitialize() {
            throw new NotImplementedException();
        }

        public int getIdleTime() {
            // GetLastInputInfo
            throw new NotImplementedException();
        }

        public Window getActiveWindow() {
            throw new NotImplementedException();
        }

        public void hookUserEvents(Action<KeyboardEvent> callback) {
            throw new NotImplementedException();
        }
    }
}
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
            // https://stackoverflow.com/a/11901730/13240621
            throw new NotImplementedException();
        }

        public Window getActiveWindow() {
            // GetForegroundWindow
            // https://stackoverflow.com/a/115905/13240621
            throw new NotImplementedException();
        }

        public void hookUserEvents(Action<KeyboardEvent> callback) {
            // https://stackoverflow.com/a/604417/13240621
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Threading;

namespace niacop.Native.WindowManagers {
    public class WinApiWindowManager : IWindowManager {
        public void initialize() {
            throw new NotImplementedException();
        }

        public int getIdleTime() {
            // GetLastInputInfo
            throw new NotImplementedException();
        }

        public Window getActiveWindow() {
            throw new NotImplementedException();
        }

        public void hookUserEvents(Action<KeyboardEvent> callback, CancellationToken cancelToken) {
            throw new NotImplementedException();
        }
    }
}
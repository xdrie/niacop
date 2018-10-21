using System;
using System.Threading;

namespace niacop.Native.WindowManagers {
    public interface IWindowManager {
        /// <summary>
        /// initialize the window manager reader
        /// </summary>
        void initialize();
        
        /// <summary>
        /// get idle time in milliseconds
        /// </summary>
        /// <returns></returns>
        int getIdleTime();

        /// <summary>
        /// get information about the active window
        /// </summary>
        /// <returns></returns>
        Window getActiveWindow();

        void hookUserEvents(Action<KeyboardEvent> callback);
    }
}
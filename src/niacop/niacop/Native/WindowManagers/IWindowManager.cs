namespace niacop.Native.WindowManagers {
    public interface IWindowManager {
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
    }
}
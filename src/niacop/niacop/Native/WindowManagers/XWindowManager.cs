namespace niacop.Native.WindowManagers {
    public class XWindowManager : IWindowManager {
        public int getIdleTime() {
            // xprintidle
            var xIdleTime = Shell.executeShellCommand("xprintidle", string.Empty);
            return int.Parse(xIdleTime.output);
        }

        public Window getActiveWindow() {
            var wClass = Shell.executeShellCommand("xprop",
                "-id $(xdotool getwindowfocus) WM_CLASS | cut -d \'\"\' -f4");
            var wTitle = Shell.executeShellCommand("xprop",
                "-id $(xdotool getwindowfocus) _NET_WM_NAME | cut -d \'\"\' -f2");
            var wPid = Shell.executeShellCommand("xprop",
                "-id $(xdotool getwindowfocus) _NET_WM_PID | cut -d \' \' -f3");
            // TODO: proper error handling for missing xdotool
            return new Window {
                application = wClass.output,
                processId = int.Parse(wPid.output),
                title = wTitle.output
            };
        }
    }
}
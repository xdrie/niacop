using System;
using System.IO;
using niacop.Services;

namespace niacop.Native.WindowManagers {
    public class XWindowManager : IWindowManager {
        public void initialize() {
            // ensure that both required commands are available
            var xprintidle = Shell.executeEval("xprintidle");
            if (xprintidle.exitCode != 0) throw new FileNotFoundException("xprintidle was not found");
            var xprop = Shell.executeEval("xprop -version");
            if (xprop.exitCode != 0) throw new FileNotFoundException("xprop was not found");
            var xdotool = Shell.executeEval("xdotool version");
            if (xdotool.exitCode != 0) throw new FileNotFoundException("xdotool was not found");
        }

        public int getIdleTime() {
            // xprintidle
            var xIdleTime = Shell.executeEval("xprintidle");
            return int.Parse(xIdleTime.stdout);
        }

        public Window getActiveWindow() {
            var wClass = Shell.executeEval("xprop -id $(xdotool getwindowfocus) WM_CLASS | cut -d \'\"\' -f4");
            var wTitle = Shell.executeEval("xprop -id $(xdotool getwindowfocus) _NET_WM_NAME | cut -d \'\"\' -f2");
            var wPid = Shell.executeEval("xprop -id $(xdotool getwindowfocus) _NET_WM_PID | cut -d \' \' -f3");
            try {
                return new Window {
                    application = wClass.stdout,
                    processId = int.Parse(wPid.stdout),
                    title = wTitle.stdout
                };
            } catch (FormatException e) {
                Logger.log($"format exception: {wPid} - {e}", Logger.Level.Error);
                return null;
            }
        }
    }
}
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
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
            var xinput = Shell.executeEval("xinput list");
            if (xinput.exitCode != 0) throw new FileNotFoundException("xinput was not found");
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

        public void hookUserEvents(Action<KeyboardEvent> callback, CancellationToken cancelToken) {
            // global key hook: https://unix.stackexchange.com/a/129171
            // xinput list | grep -Po 'id=\K\d+(?=.*slave\s*keyboard)' | xargs -P0 -n1 xinput test | awk 'BEGIN{while (("xmodmap -pke" | getline) > 0) k[$2]=$4}{print $0 "[" k[$NF] "]"}'
            var globalHookCommand =
                "xinput list | grep -Po \'id=\\K\\d+(?=.*slave\\s*keyboard)\' | xargs -P0 -n1 xinput test | awk \'BEGIN{while ((\"xmodmap -pke\" | getline) > 0) k[$2]=$4}{print $0 \"[\" k[$NF] \"]\"}\'";
//            var keyEventRegex = new Regex(@"state\s0x1(\d).*keycode\s([0-9]+)\s\(keysym\s0x[0-9]+,\s(\w)");
            var keyEventRegex = new Regex(@"key\s(\w+)\s+([0-9]+)\s\[(.)\]");
            var keyHookProc = Shell.shellExecute(globalHookCommand);
            while (!cancelToken.IsCancellationRequested) {
                var line = keyHookProc.StandardOutput.ReadLine();
                if (line == null) break;
                var lineMatch = keyEventRegex.Match(line);
                callback(new KeyboardEvent {
                    pressed = lineMatch.Groups[1].Value == "press",
                    keycode = int.Parse(lineMatch.Groups[2].Value),
                    keyChar = lineMatch.Groups[3].Value[0]
                });
            }
        }
    }
}
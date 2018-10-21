using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using niacop.Services;

namespace niacop.Native.WindowManagers {
    public class XWindowManager : IWindowManager {
        private Dictionary<int, string> xmodmap = new Dictionary<int, string>();

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
            
            // store keyboard mappings
            var xmodmapRaw = Shell.executeEval("xmodmap -pke");
            foreach (var line in xmodmapRaw.stdout.Split('\n')) {
                var cols = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (cols.Length < 4) continue; // missing keysym
                var keyId = int.Parse(cols[1]);
                var keyName = cols[3];
                xmodmap[keyId] = keyName;
            }
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
                    application = wClass.stdout.Trim(),
                    processId = int.Parse(wPid.stdout.Trim()),
                    title = wTitle.stdout.Trim()
                };
            } catch (FormatException e) {
                Logger.log($"format exception: {wPid.stdout} - {e}", Logger.Level.Error);
                return null;
            }
        }

        public void hookUserEvents(Action<KeyboardEvent> callback) {
            // global key hook: https://unix.stackexchange.com/a/129171
            var globalHookCommand =
                "xinput list | grep -Po \'id=\\K\\d+(?=.*slave\\s*keyboard)\' | xargs -P0 -n1 xinput test";
            var keyEventRegex = new Regex(@"key\s(\w+)\s+([0-9]+)");
            var keyHookProc = Shell.shellExecute(globalHookCommand);
            keyHookProc.Start();
            keyHookProc.BeginOutputReadLine();
            keyHookProc.OutputDataReceived += (sender, args) => {
                var line = args.Data;
                if (line != null) {
                    var lineMatch = keyEventRegex.Match(line);
                    var sysKeycode = int.Parse(lineMatch.Groups[2].Value);
                    var keysym = xmodmap[sysKeycode];
                    callback(new KeyboardEvent {
                        pressed = lineMatch.Groups[1].Value == "press",
                        key = keysym
                    });
                }
            };
        }
    }
}
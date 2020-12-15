using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace Nia.Native.Platforms {
    public class XWindowManager : IWindowManager {
        private Dictionary<int, string> keymap = new Dictionary<int, string>();

        private Process? keyHookProc;

        public void initialize() {
            // ensure that all required commands are available
            var xprintidle = Shell.executeEval("xprintidle");
            if (xprintidle.exitCode != 0) throw new FileNotFoundException($"{nameof(xprintidle)} was not found");
            var xprop = Shell.executeEval("xprop -version");
            if (xprop.exitCode != 0) throw new FileNotFoundException($"{nameof(xprop)} was not found");
            var xdotool = Shell.executeEval("xdotool version");
            if (xdotool.exitCode != 0) throw new FileNotFoundException($"{nameof(xdotool)} was not found");
            var xinput = Shell.executeEval("xinput list");
            if (xinput.exitCode != 0) throw new FileNotFoundException($"{nameof(xinput)} was not found");
            var xmodmap = Shell.executeEval("xmodmap");
            if (xmodmap.exitCode != 0) throw new FileNotFoundException($"{nameof(xmodmap)} was not found");

            // store keyboard mappings
            var rawKeymap = Shell.executeEval("xmodmap -pke");
            foreach (var line in rawKeymap.stdout.Split('\n')) {
                var cols = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (cols.Length < 4) continue; // missing keysym
                var keyId = int.Parse(cols[1]);
                var keyName = cols[3];
                keymap[keyId] = keyName;
            }
        }

        public int getIdleTime() {
            // xprintidle
            var xIdleTime = Shell.executeEval("xprintidle");
            return int.Parse(xIdleTime.stdout);
        }

        public Window getActiveWindow() {
            var winClass = Shell.executeEval("xprop -id $(xdotool getwindowfocus) WM_CLASS | cut -d \'\"\' -f4");
            var winName = Shell.executeEval("xprop -id $(xdotool getwindowfocus) WM_NAME | cut -d \'\"\' -f2");
            var winPid = Shell.executeEval("xdotool getwindowfocus getwindowpid");
            var processId = 0;
            int.TryParse(winPid.stdout.Trim(), out processId);
            var window = new Window(winClass.stdout.Trim(), winName.stdout.Trim(), processId);

            return window;
        }

        public void hookUserEvents(Action<KeyboardEvent> callback) {
            // global key hook: https://unix.stackexchange.com/a/129171
            var globalHookCommand =
                "xinput list | grep -Po \'id=\\K\\d+(?=.*slave\\s*keyboard)\' | xargs -P0 -n1 xinput test";
            var keyEventRegex = new Regex(@"key\s(\w+)\s+([0-9]+)");
            keyHookProc = Shell.shellExecute(globalHookCommand);
            keyHookProc.Start();
            keyHookProc.BeginOutputReadLine();
            keyHookProc.OutputDataReceived += (sender, args) => {
                var line = args.Data;
                if (line != null) {
                    try {
                        var lineMatch = keyEventRegex.Match(line);
                        var sysKeycode = int.Parse(lineMatch.Groups[2].Value);
                        var keysym = keymap[sysKeycode];
                        callback(new KeyboardEvent(keysym, lineMatch.Groups[1].Value == "press"));
                    }
                    catch (Exception ex) {
                        Global.log.err($"xinput keyhook event could not be parsed: {ex}");
                    }
                }
            };
        }

        public void deinitialize() {
            if (keyHookProc != null && !keyHookProc.HasExited) {
                keyHookProc.Kill();
                keyHookProc.WaitForExit();
            }
        }
    }
}
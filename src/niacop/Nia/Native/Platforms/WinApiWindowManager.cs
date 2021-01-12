using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Nia.Native.Platforms {
    public class WinApiWindowManager : IWindowManager {
        public class Api {
            struct LastInputInfo {
                public uint cbSize;

                public uint dwTime;
            }

            [DllImport("user32.dll")]
            private static extern bool GetLastInputInfo(ref LastInputInfo plii);

            [DllImport("kernel32.dll")]
            private static extern uint GetLastError();

            /// <summary>
            /// https://stackoverflow.com/a/11901730/13240621
            /// get idle time in millis
            /// </summary>
            /// <returns></returns>
            public static uint getIdleTime() {
                LastInputInfo lastInput = new LastInputInfo();
                lastInput.cbSize = (uint) Marshal.SizeOf(lastInput);
                GetLastInputInfo(ref lastInput);

                return ((uint) Environment.TickCount - lastInput.dwTime);
            }

            [DllImport("user32.dll")]
            public static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll")]
            private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

            /// <summary>
            /// https://stackoverflow.com/questions/115868/how-do-i-get-the-title-of-the-current-active-window-using-c/115905#115905
            /// </summary>
            /// <returns></returns>
            public static string? getWindowTitle(IntPtr windowHandle) {
                const int bufSize = 256;
                var buf = new StringBuilder(bufSize);

                if (GetWindowText(windowHandle, buf, bufSize) > 0) {
                    return buf.ToString();
                }

                return null;
            }
        }

        public void initialize() {
            // -
        }

        public void deinitialize() {
            // -
        }

        public int getIdleTime() {
            return (int) Api.getIdleTime();
        }

        public Window getActiveWindow() {
            // GetForegroundWindow
            var windowHandle = Api.GetForegroundWindow();
            var winTitle = Api.getWindowTitle(windowHandle);
            Api.GetWindowThreadProcessId(windowHandle, out var winPid);
            // https://stackoverflow.com/a/115905/13240621
            return new Window("", winTitle ?? "", (int) winPid);
        }

        public void hookUserEvents(Action<KeyboardEvent> callback) {
            // https://stackoverflow.com/a/604417/13240621
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Diagnostics;
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

                var diff = (uint) Environment.TickCount - lastInput.dwTime;
                return diff;
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

                if (GetWindowText(windowHandle, buf, bufSize) > 0)
                    return buf.ToString();

                return null;
            }

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

            public static string? getWindowClassName(IntPtr windowHandle) {
                const int bufSize = 256;
                var buf = new StringBuilder(bufSize);
                if (GetClassName(windowHandle, buf, bufSize) > 0)
                    return buf.ToString();

                return null;
            }

            private const int WH_KEYBOARD_LL = 13;

            public static IntPtr setHook(LowLevelKeyboardProc proc) {
                using (Process curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess.MainModule!) {
                    return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                        GetModuleHandle(curModule!.ModuleName!), 0);
                }
            }

            public delegate IntPtr LowLevelKeyboardProc(
                int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr SetWindowsHookEx(int idHook,
                LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
                IntPtr wParam, IntPtr lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr GetModuleHandle(string lpModuleName);
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
            var pid = 0;
            var appName = default(string);
            Api.GetWindowThreadProcessId(windowHandle, out var winPid);
            if (winPid > 0) {
                pid = (int) winPid;
                appName = Process.GetProcessById((int) pid).MainModule?.ModuleName;
            }
            else {
                pid = -1; // failed to get PID
            }

            var winClass = Api.getWindowClassName(windowHandle);
            // https://stackoverflow.com/a/115905/13240621
            return new Window(appName ?? "", winTitle ?? "", pid);
        }

        public void hookUserEvents(Action<KeyboardEvent> callback) {
            // https://stackoverflow.com/a/604417/13240621
            IntPtr hookId = IntPtr.Zero;
            const int WM_KEYDOWN = 0x0100;
            const int WM_KEYUP = 0x0101;

            IntPtr hookCallback(
                int nCode, IntPtr wParam, IntPtr lParam) {
                Global.log.trace($"keyevent! cd: {nCode}");
                if (nCode >= 0) {
                    int code = Marshal.ReadInt32(lParam);
                    var keyStr = code.ToString();
                    if (wParam == (IntPtr) WM_KEYDOWN) {
                        callback(new KeyboardEvent(keyStr, true));
                    } else if (wParam == (IntPtr) WM_KEYUP) {
                        callback(new KeyboardEvent(keyStr, false));
                    }
                }

                return Api.CallNextHookEx(hookId, nCode, wParam, lParam);
            }

            hookId = Api.setHook(hookCallback);
            Global.log.trace($"win32 key event hook set");
        }
    }
}
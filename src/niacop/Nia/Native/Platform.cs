using System;
using System.Runtime.InteropServices;
using Nia.Native.Platforms;

namespace Nia.Native {
    public class Platform {
        public IWindowManager? wm;

        public Platform() {
            if (OperatingSystem.IsLinux()) {
                wm = new XWindowManager();
            } else if (OperatingSystem.IsMacOS()) {
                wm = new CocoaWIndowManager();
            } else if (OperatingSystem.IsWindows()) {
                wm = new WinApiWindowManager();
            } else {
                throw new PlatformNotSupportedException(
                    $"The platform {Environment.OSVersion.Platform} ({RuntimeInformation.OSDescription}) is not supported.");
            }

            Global.log.info($"using window manager: {wm.GetType().Name}");

            wm!.initialize();
        }

        public void destroy() {
            wm?.deinitialize();
        }
    }
}
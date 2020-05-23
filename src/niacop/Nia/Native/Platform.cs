﻿using System;
using Nia.Native.WindowManagers;

namespace Nia.Native {
    public class Platform {
        public IWindowManager wm;

        public Platform() {
            switch (Environment.OSVersion.Platform) {
                case PlatformID.Unix:
                    wm = new XWindowManager();
                    break;
                case PlatformID.Win32NT:
                    wm = new WinApiWindowManager();
                    break;
                default:
                    throw new PlatformNotSupportedException($"The platform {Environment.OSVersion.Platform} is not supported.");
            }
            wm.initialize();
        }

        public void destroy() {
            wm?.deinitialize();
        }
    }
}
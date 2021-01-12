#if NIACOP_OSX
using System;

namespace Nia.Native.Platforms {
    public class CocoaWIndowManager : IWindowManager {
        public void initialize() {
            throw new NotImplementedException();
        }

        public void deinitialize() {
            throw new NotImplementedException();
        }

        public int getIdleTime() {
            // https://stackoverflow.com/questions/53559121/how-to-detect-user-inactivity-in-os-x-writing-in-swift-cocoa/53562220#53562220
            // https://stackoverflow.com/questions/5039788/best-way-to-know-if-application-is-inactive-in-cocoa-mac-osx/17656688#17656688
            throw new NotImplementedException();
        }

        public Window getActiveWindow() {
            // https://stackoverflow.com/questions/51646915/get-active-window-information-using-c-sharp-on-mac
            throw new NotImplementedException();
        }

        public void hookUserEvents(Action<KeyboardEvent> callback) {
            throw new NotImplementedException();
        }
    }
}
#else
using System;

namespace Nia.Native.Platforms {
    /// <summary>
    /// NOTE: this is a STUB! This provides a dummy implementation. Restore with `dotnet restore -r osx-x64` to use the real version of this class
    /// </summary>
    public class CocoaWIndowManager : IWindowManager {
        private const string COMPAT_MSG =
            "Cocoa support not available on this (cross-platform/generic) runtime. Please run 'dotnet restore -r osx-x64' to install the correct packages.";

        public void initialize() {
            throw new PlatformNotSupportedException(COMPAT_MSG);
        }

        public void deinitialize() {
            throw new PlatformNotSupportedException(COMPAT_MSG);
        }

        public int getIdleTime() {
            // https://stackoverflow.com/questions/53559121/how-to-detect-user-inactivity-in-os-x-writing-in-swift-cocoa/53562220#53562220
            // https://stackoverflow.com/questions/5039788/best-way-to-know-if-application-is-inactive-in-cocoa-mac-osx/17656688#17656688
            throw new PlatformNotSupportedException(COMPAT_MSG);
        }

        public Window getActiveWindow() {
            // https://stackoverflow.com/questions/51646915/get-active-window-information-using-c-sharp-on-mac
            throw new PlatformNotSupportedException(COMPAT_MSG);
        }

        public void hookUserEvents(Action<KeyboardEvent> callback) {
            throw new PlatformNotSupportedException(COMPAT_MSG);
        }
    }
}
#endif
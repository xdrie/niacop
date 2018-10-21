using System;

namespace niacop.Platform {
    public static class Shell {
        public enum ShellExecutorType
        {
            Generic,
            Windows,
            Unix
        }
        
        private static ShellExecutorType executorType;
        
        static Shell()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    executorType = ShellExecutorType.Windows;
                    break;

                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    executorType = ShellExecutorType.Unix;
                    break;
            }
        }
    }
}
using System;

namespace niacop.Platform {
    public static class DataPaths {
        public static string configBase;
        
        static DataPaths() {
            configBase = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }
    }
}
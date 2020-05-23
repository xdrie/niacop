using System;
using System.IO;
using niacop.Configuration;

namespace niacop.Native {
    public static class DataPaths {
        public static string configBase;
        public static string dataBase;

        public static string dataId = "niacop";
        
        static DataPaths() {
            configBase = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), dataId);
            dataBase = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), dataId);
        }

        public static string profilePath => Path.Combine(dataBase, $"profile_{Global.config.profile.name}");
    }
}
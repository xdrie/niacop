using System;
using System.IO;

namespace Nia.Native {
    public static class DataPaths {
        public static string configPath;
        public static string dataPath;

        public static string dataId = "niacop";
        
        static DataPaths() {
            configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), dataId);
            dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), dataId);
        }

        public static string profilePath => Path.Combine(dataPath, $"profile_{Global.config!.profile.name}");
    }
}
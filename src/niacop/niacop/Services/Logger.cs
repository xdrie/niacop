using System;

namespace niacop.Services {
    public static class Logger {
        public enum Level {
            Trace,
            Warning,
            Error,
            Critical
        }

        public static Level verbosity;

        public static void log(string data, Level level = Level.Trace) {
            if (level >= verbosity) {
                var timestamp = DateTime.Now.ToString("HH:mm:ss.f");
                Console.WriteLine($"[{timestamp}] [{level.ToString()}] {data}");
            }
        }
    }
}
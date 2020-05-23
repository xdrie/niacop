using System.Collections.Generic;
using Iri.Glass.Logging;

namespace niacop {
    public static class Global {
        public static Config config;
        public static Logger log = new Logger(Logger.Verbosity.Information) {
            sinks = new List<Logger.ILogSink> {
                new Logger.ConsoleSink()
            }
        };
    }
}
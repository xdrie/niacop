using System;

namespace niacop.Configuration {
    public static class Options {
        public static string profile = "default";
        
        // tracker
        public static int idleThreshold = 5 * 60 * 1000;
        public static int windowPoll = 2 * 1000;
        public static bool keylogger = false;
        
        // book
        public static int browseEntries = 10;
        
        // plugins
        public static string[] plugins = new string[0]; 

        public static void load(OptionParser optionParser) {
            profile = optionParser.get("profile");
            
            idleThreshold = int.Parse(optionParser.get("tracker.idleThreshold"));
            windowPoll = int.Parse(optionParser.get("tracker.windowPoll"));
            keylogger = bool.Parse(optionParser.get("tracker.keylogger"));

            browseEntries = int.Parse(optionParser.get("book.browseEntries"));

            plugins = optionParser.get("plugins.plugins").Split(",", StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
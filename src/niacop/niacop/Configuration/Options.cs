using System;

namespace niacop.Configuration {
    public static class Options {
        public static string profile;

        // tracker
        public static int idleThreshold;
        public static int windowPoll;
        public static bool keylogger;

        // book
        public static int browseEntries;

        // plugins
        public static string[] plugins = new string[0];

        public static int getInt(this OptionParser opt, string id, int def) {
            return int.Parse(opt.get(id) ?? def.ToString());
        }

        public static bool getBool(this OptionParser opt, string id, bool def) {
            return bool.Parse(opt.get(id) ?? def.ToString());
        }

        public static string getStr(this OptionParser opt, string id, string def) {
            return opt.get(id) ?? def;
        }

        public static void load(OptionParser optionParser) {
            profile = optionParser.getStr("profile", "default");

            idleThreshold = optionParser.getInt("tracker.idleThreshold", 5 * 60 * 1000);
            windowPoll = optionParser.getInt("tracker.windowPoll", 2 * 1000);
            keylogger = optionParser.getBool("tracker.keylogger", false);

            browseEntries = optionParser.getInt("book.browseEntries", 10);

            plugins = optionParser.getStr("plugins.plugins", ",").Split(",", StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
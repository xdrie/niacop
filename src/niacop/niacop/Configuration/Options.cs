namespace niacop.Configuration {
    public static class Options {
        public static string profile = "default";
        
        // tracker
        public static int idleThreshold = 5 * 60 * 1000;
        public static int windowPoll = 2 * 1000;
        
        // book
        public static int browseEntries = 10;

        public static void load(OptionParser optionParser) {
            profile = optionParser.get("profile");
            
            idleThreshold = int.Parse(optionParser.get("tracker.idleThreshold"));
            windowPoll = int.Parse(optionParser.get("tracker.windowPoll"));

            browseEntries = int.Parse(optionParser.get("book.browseEntries"));
        }
    }
}
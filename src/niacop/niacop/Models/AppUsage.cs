namespace niacop.Models {
    public class AppUsage {
        public string application;
        public long time = 0;
        public long keyEvents = 0;

        public AppUsage(string application) {
            this.application = application;
        }
    }
}
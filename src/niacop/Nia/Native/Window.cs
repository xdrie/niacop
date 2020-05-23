namespace Nia.Native {
    public class Window {
        public string application;
        public string title;
        public int processId;

        public Window(string application, string title, int processId = 0) {
            this.application = application;
            this.title = title;
            this.processId = processId;
        }
    }
}
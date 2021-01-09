namespace Nia.Util {
    public class ReportPrinter {
        private readonly int width;

        public const int DEFAULT_WIDTH = 80;

        public ReportPrinter(int width = DEFAULT_WIDTH) {
            this.width = width;
        }
    }
}
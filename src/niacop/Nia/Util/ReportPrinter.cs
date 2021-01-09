using System;

namespace Nia.Util {
    public class ReportPrinter {
        private readonly int width;

        public const int DEFAULT_WIDTH = 80;

        public ReportPrinter(int width = DEFAULT_WIDTH) {
            this.width = width;
        }

        private void print(string s) {
            Console.Write(s);
        }

        private void println(string s) {
            Console.WriteLine(s);
        }

        public void header(string title, string subtitle) {
            var row = $"{title} :: {subtitle}";
            printWithSeparator(row);
        }

        public void header(string title) {
            var row = $"{title}";
            printWithSeparator(row);
        }

        private void printWithSeparator(string text) {
            var sepLen = DEFAULT_WIDTH - text.Length - 1;
            var sep = new string('―', sepLen);
            println($"{text} {sep}");
        }


        public void line() {
            println("");
        }

        public void line(string str) {
            println(str);
        }
    }
}
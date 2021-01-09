using System;
using System.Collections.Generic;
using System.Linq;

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

        public void ratioGraph(List<(string, long)> data) {
            // 1. find the biggest value
            var maxVal = data.OrderByDescending(x => x.Item2).First().Item2;
            var startX = data.OrderByDescending(x => x.Item1.Length).First().Item1.Length + 1;
            // 2. print
            foreach (var (lb, val) in data) {
                var ratio = (float) val / maxVal;
                var barLen = (int) ((width - startX) * ratio);
                var bar = new string('⋯', barLen);
                var lineStr = $"{lb} {bar}";
                line(lineStr);
            }
        }
    }
}
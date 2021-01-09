using System;
using System.Collections.Generic;
using System.Linq;

namespace Nia.Util {
    public class ReportPrinter {
        private readonly int width;

        public const int DEFAULT_WIDTH = 80;
        public ConsoleColor background;

        public ReportPrinter(int width = DEFAULT_WIDTH) {
            this.width = width;
            background = Console.BackgroundColor;
        }

        private void print(string s, ConsoleColor col) {
            Console.ResetColor();
            Console.ForegroundColor = col;
            Console.BackgroundColor = background;
            Console.Write(s);
            Console.ResetColor();
        }

        private void println(string s, ConsoleColor col) {
            print(s + "\n", col);
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
            var sep = new string('═', sepLen);
            println($"{text} {sep}", ConsoleColor.White);
        }


        public void line() {
            println(new string(' ', width), ConsoleColor.White);
        }

        public void ratioGraph(List<(string, long)> data) {
            // 1. find the biggest value
            var maxVal = data.OrderByDescending(x => x.Item2).First().Item2;
            var startX = data.OrderByDescending(x => x.Item1.Length).First().Item1.Length + 1;
            // 2. print
            foreach (var (lb, val) in data) {
                var ratio = (float) val / maxVal;
                var maxBarLen = width - startX;
                var barLen = (int) ((maxBarLen) * ratio);
                var bar = new string('▃', barLen);
                var barPad = new string(' ', maxBarLen - barLen);
                print($"{lb} ", ConsoleColor.White);
                println(bar + barPad, ConsoleColor.Cyan);
            }
        }
    }
}
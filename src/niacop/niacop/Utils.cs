using System;

namespace niacop {
    public class Utils {
        public static long timestamp() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public static DateTimeOffset parseTimestamp(long timestamp) =>
            DateTimeOffset.FromUnixTimeMilliseconds(timestamp);

        public struct SINumber : IFormattable {
            public char? prefix;
            public double value;

            public SINumber(char? prefix, double value) {
                this.prefix = prefix;
                this.value = value;
            }

            public override string ToString() {
                return ToString(null);
            }

            public string ToString(string? format, IFormatProvider? formatProvider = null) {
                return value.ToString(format) + prefix;
            }
        }

        public static SINumber formatNumberSI(double d, string format = null) {
            char[] incPrefixes = new[] {'k', 'M', 'G', 'T', 'P', 'E', 'Z', 'Y'};
            char[] decPrefixes = new[] {'m', '\u03bc', 'n', 'p', 'f', 'a', 'z', 'y'};

            int degree = (int) Math.Floor(Math.Log10(Math.Abs(d)) / 3);
            double scaled = d * Math.Pow(1000, -degree);

            char? prefix = null;
            switch (Math.Sign(degree)) {
                case 1:
                    prefix = incPrefixes[degree - 1];
                    break;
                case -1:
                    prefix = decPrefixes[-degree - 1];
                    break;
            }

            // return scaled.ToString(format) + prefix;
            return new SINumber(prefix, scaled);
        }
    }
}
using System;

namespace Nia {
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

        public static SINumber formatNumberSI(double d) {
            var incPrefixes = new[] {'k', 'M', 'G', 'T', 'P', 'E', 'Z', 'Y'};
            var decPrefixes = new[] {'m', '\u03bc', 'n', 'p', 'f', 'a', 'z', 'y'};

            var degree = (int) Math.Floor(Math.Log10(Math.Abs(d)) / 3);
            var scaled = d * Math.Pow(1000, -degree);

            char? prefix = null;
            if (Math.Abs(scaled) >= double.Epsilon) {
                 prefix = Math.Sign(degree) switch {
                    1 => incPrefixes[degree - 1],
                    -1 => decPrefixes[-degree - 1],
                    _ => null
                };
            }

            return new SINumber(prefix, scaled);
        }
    }
}
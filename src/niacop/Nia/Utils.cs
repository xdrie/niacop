using System;

namespace Nia {
    public class Utils {
        public static long timestamp() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public static DateTimeOffset parseTimestamp(long timestamp) =>
            DateTimeOffset.FromUnixTimeMilliseconds(timestamp);

        public static DateTime timestampToLocal(long timestamp) => parseTimestamp(timestamp).LocalDateTime;

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

        /// <summary>
        /// adapted from https://dotnetthoughts.net/time-ago-function-for-c/
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static string timeAgo(TimeSpan timeSpan) {
            string result = string.Empty;

            if (timeSpan <= TimeSpan.FromSeconds(60)) {
                result = $"{timeSpan.Seconds} seconds ago";
            }
            else if (timeSpan <= TimeSpan.FromMinutes(60)) {
                result = timeSpan.Minutes > 1
                    ? $"about {timeSpan.Minutes} minutes ago"
                    : "about a minute ago";
            }
            else if (timeSpan <= TimeSpan.FromHours(24)) {
                result = timeSpan.Hours > 1
                    ? $"about {timeSpan.Hours} hours ago"
                    : "about an hour ago";
            }
            else if (timeSpan <= TimeSpan.FromDays(30)) {
                result = timeSpan.Days > 1 ? $"about {timeSpan.Days} days ago" : "yesterday";
            }
            else if (timeSpan <= TimeSpan.FromDays(365)) {
                result = timeSpan.Days > 30
                    ? $"about {timeSpan.Days / 30} months ago"
                    : "about a month ago";
            }
            else {
                result = timeSpan.Days > 365
                    ? $"about {timeSpan.Days / 365} years ago"
                    : "about a year ago";
            }

            return result;
        }
    }
}
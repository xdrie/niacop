using System;

namespace niacop {
    public class Utils {
        public static long timestamp() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public static DateTimeOffset parseTimestamp(long timestamp) =>
            DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
    }
}
namespace Nia.Util {
    public static class Wildcard {
        public static bool match(string pattern, string str) {
            return System.Text.RegularExpressions.Regex.IsMatch(str,
                "^" + System.Text.RegularExpressions.Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") +
                "$");
        }

        public static bool isRaw(string pattern) {
            return !pattern.Contains('?') && !pattern.Contains('*');
        }
    }
}
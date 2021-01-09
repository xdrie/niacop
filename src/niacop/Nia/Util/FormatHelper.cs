using System;

namespace Nia.Util {
    public static class FormatHelper {
        public static string formatTimeHM(TimeSpan timeSpan) {
            var remainingTime = timeSpan;
            var hours = (int) remainingTime.TotalHours;
            remainingTime -= TimeSpan.FromHours(hours);
            var mins = (int) remainingTime.TotalMinutes;

            return $"{hours:D2}h{mins:D2}m";
        }
    }
}
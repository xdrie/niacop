using System.Text;
using SQLite;

namespace Nia.Models {
    public class Session {
        [PrimaryKey, AutoIncrement] public int id { get; set; }

        [Indexed] public string? application { get; set; }

        public string? windowTitle { get; set; }
        public int processId { get; set; }
        public string? processName { get; set; }
        public string? processPath { get; set; }
        [Indexed] public long startTime { get; set; }
        [Indexed] public long endTime { get; set; }
        public long keyEvents { get; set; }
        public long getDuration() => endTime - startTime;

        public override string ToString() => $"application({getDuration()})";

        public string prettyFormat() {
            var sb = new StringBuilder();
            sb.AppendLine($"id:             {id}");
            sb.AppendLine($"application:    {application}");
            sb.AppendLine($"title:          {windowTitle}");
            sb.AppendLine($"processId:      {processId}");
            sb.AppendLine($"processPath:    {processPath}");
            sb.AppendLine($"startTime:      {Utils.timestampToLocal(startTime)}");
            sb.AppendLine($"endTime:        {Utils.timestampToLocal(endTime)}");
            sb.AppendLine($"keyEvents:      {keyEvents}");
            return sb.ToString();
        }
    }
}
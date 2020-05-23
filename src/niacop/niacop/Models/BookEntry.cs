using SQLite;

namespace niacop.Models {
    public class BookEntry {
        [PrimaryKey, AutoIncrement] public int id { get; set; }

        [Indexed] public long timestamp { get; set; }

        public string? words { get; set; }

        public override string ToString() => timestamp.ToString();
    }
}
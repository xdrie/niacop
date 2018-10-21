using System;
using System.IO;
using niacop.Native;
using SQLite;

namespace niacop.Services {
    public class BookInteractive {
        private Platform _plat;
        public string bookDataPath;
        public string bookDatabaseFile;
        public SQLiteConnection database;

        public class BookEntry {
            [PrimaryKey, AutoIncrement]
            public int id { get; set; }

            [Indexed]
            public long timestamp { get; set; }

            public string words;

            public override string ToString() => timestamp.ToString();
        }

        public BookInteractive() {
            _plat = new Platform();
        }

        public void initialize() {
            bookDataPath = Path.Combine(DataPaths.profilePath, "book");
            bookDatabaseFile = Path.Combine(bookDataPath, "data.db");
            Directory.CreateDirectory(Path.GetDirectoryName(bookDatabaseFile));
            database = new SQLiteConnection(bookDatabaseFile);
            database.CreateTable<BookEntry>();
        }

        public void run() {
            Console.WriteLine("$ book");
            Console.WriteLine("type a few words to describe your state");
            var rawWords = Console.ReadLine();
            if (string.IsNullOrEmpty(rawWords)) {
                Console.WriteLine("entry aborted");
                return;
            }

            // clean up words
            rawWords = rawWords.Replace(',', ' ');
            var words = rawWords.Split(' ');
            var timestamp = _plat.timestamp();
            var entry = new BookEntry {timestamp = timestamp, words = string.Join(',', words)};
            database.Insert(entry);
            Console.WriteLine($"saved entry[{words.Length}] for {timestamp}");
            database.Dispose();
        }
    }
}
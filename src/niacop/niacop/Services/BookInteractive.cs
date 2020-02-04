using System;
using System.IO;
using System.Linq;
using niacop.Configuration;
using niacop.Native;
using SQLite;

namespace niacop.Services {
    public class BookInteractive {
        private Platform _plat;
        public string bookDataPath;
        public string bookDatabaseFile;
        public SQLiteConnection database;

        public class BookEntry {
            [PrimaryKey, AutoIncrement] public int id { get; set; }

            [Indexed] public long timestamp { get; set; }

            public string words { get; set; }

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
            while (true) {
                Console.WriteLine("$ book");
                Console.WriteLine("1. new entry");
                Console.WriteLine("2. browse");
                Console.WriteLine("x. exit");
                var resp = Console.ReadLine();
                switch (resp) {
                    case "1": {
                        Console.WriteLine("type a few words to describe your state");
                        var rawWords = Console.ReadLine();
                        if (string.IsNullOrEmpty(rawWords)) {
                            Console.WriteLine("entry aborted");
                            continue;
                        }

                        // clean up words
                        rawWords = rawWords.Replace(',', ' ');
                        var words = rawWords.Split(' ');
                        var timestamp = Platform.timestamp();
                        var entry = new BookEntry {timestamp = timestamp, words = string.Join(',', words)};
                        database.Insert(entry);
                        Console.WriteLine($"saved entry[{words.Length}] for {DateTime.Now}/{timestamp}");
                    }
                        break;
                    case "2": {
                        var entries = database.Table<BookEntry>();
                        var entryCount = entries.Count();
                        Console.WriteLine(
                            $"showing [{Math.Min(Options.browseEntries, entryCount)}/{entryCount}] entries:");
                        var recentEntries = entries.OrderByDescending(x => x.timestamp)
                            .Take(Options.browseEntries);
                        foreach (var entry in recentEntries) {
                            var localTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(entry.timestamp).ToLocalTime();
                            Console.WriteLine($"== {localTimestamp:yyyy/MM/dd HH:mm:ss.f} ==");
                            Console.WriteLine($"    {entry.words}");
                        }

                        Console.WriteLine();
                    }
                        break;
                    default:
                        return;
                }
            }
        }

        public void destroy() {
            database.Dispose();
            _plat.destroy();
        }
    }
}
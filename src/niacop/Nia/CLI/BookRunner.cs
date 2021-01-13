using System;
using System.IO;
using CommandLine;
using Nia.Models;
using Nia.Native;
using SQLite;

namespace Nia.CLI {
    public class BookRunner : Runner<BookRunner.Options> {
        [Verb("book", HelpText = "interact with the Book.")]
        public class Options { }

        public string bookDataPath;
        public string bookDatabaseFile;
        public SQLiteConnection? database;

        public BookRunner() {
            bookDataPath = Path.Combine(DataPaths.profilePath, "book");
            bookDatabaseFile = Path.Combine(bookDataPath, "data.db");
        }


        public override int run(Options options) {
            // ensure 'book' exists in profile
            Directory.CreateDirectory(Path.GetDirectoryName(bookDatabaseFile)!);
            database = new SQLiteConnection(bookDatabaseFile);
            database.CreateTable<BookEntry>();

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
                        var timestamp = Utils.timestamp();
                        var entry = new BookEntry {timestamp = timestamp, words = string.Join(',', words)};
                        database!.Insert(entry);
                        Console.WriteLine($"saved entry[{words.Length}] for {DateTime.Now}/{timestamp}");
                    }
                        break;
                    case "2": {
                        var entries = database!.Table<BookEntry>();
                        var entryCount = entries.Count();
                        Console.WriteLine(
                            $"showing [{Math.Min(Global.config!.book.browseEntries, entryCount)}/{entryCount}] entries:");
                        var recentEntries = entries.OrderByDescending(x => x.timestamp)
                            .Take(Global.config!.book.browseEntries);
                        foreach (var entry in recentEntries) {
                            var localTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(entry.timestamp).ToLocalTime();
                            Console.WriteLine($"== {localTimestamp:yyyy/MM/dd HH:mm:ss.f} ==");
                            Console.WriteLine($"    {entry.words}");
                        }

                        Console.WriteLine();
                    }
                        break;
                    default:
                        return 0;
                }
            }
        }

        public override void Dispose() {
            base.Dispose();
            database!.Dispose();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using niacop.Configuration;
using niacop.Extensibility.Tracker;
using niacop.Native;
using niacop.Native.WindowManagers;
using SQLite;

namespace niacop.Services {
    public class ActivityTracker {
        private Platform _plat;
        private string trackerDataPath;
        private string trackerDatabaseFile;
        private SQLiteConnection database;

        public class Session {
            [PrimaryKey, AutoIncrement]
            public int id { get; set; }

            [Indexed]
            public string application { get; set; }

            public string windowTitle { get; set; }
            public int processId { get; set; }
            public string processName { get; set; }
            public string processPath { get; set; }
            public long startTime { get; set; }
            public long duration { get; set; }
            public long keyEvents { get; set; }

            public override string ToString() => $"application({duration})";
        }

        public List<Session> sessions = new List<Session>();
        public Session current;

        public ActivityTracker() {
            _plat = new Platform();
        }

        public void initialize() {
            trackerDataPath = Path.Combine(DataPaths.profilePath, "tracker");
            trackerDatabaseFile = Path.Combine(trackerDataPath, "activity.db");
            Directory.CreateDirectory(Path.GetDirectoryName(trackerDatabaseFile));
            database = new SQLiteConnection(trackerDatabaseFile);
            database.CreateTable<Session>();
        }

        public void run(CancellationToken token) {
            if (Options.keylogger) {
                runKeylogger();
            }

            while (!token.IsCancellationRequested) {
                pollSession();
                // wait 5 seconds between session polls
                Thread.Sleep(Options.windowPoll);
            }
        }

        private void pollSession() {
            var idleTime = _plat.wm.getIdleTime();
            if (idleTime > Options.idleThreshold) {
                if (current != null) {
                    Logger.log($"session entered idle state ({idleTime})", Logger.Level.Trace);
                    endSession();
                }

                return;
            }

            var window = _plat.wm.getActiveWindow();
            if (window == null) return;

            if (current == null) {
                gatherSession(window);
            }
            // current can still be null, if gather fails

            if (current != null) {
                lock (current) {
                    if (current.processId != window.processId) {
                        // window was changed, this session is over
                        endSession();
                        gatherSession(window);
                    } else {
                        // activity events?
                        var sc = new SessionContext {
                            session = current,
                            window = window
                        };
                    }
                }
            }
        }

        private void endSession() {
            lock (current) {
                current.duration = _plat.timestamp() - current.startTime;
                Logger.log($"    ended session ({current.duration}ms/{current.keyEvents}ks)",
                    Logger.Level.Trace);
                sessions.Add(current);
                database.Insert(current); // save to database
                current = null; // unset current
            }
        }

        private void gatherSession(Window window) {
            if (window == null) return;
            // detect process path
            try {
                var proc = Process.GetProcessById(window.processId);
                current = new Session {
                    application = window.application,
                    windowTitle = window.title,
                    processId = window.processId,
                    processName = proc.ProcessName,
                    processPath = proc.MainModule?.FileName,
                    startTime = _plat.timestamp()
                };
                Logger.log($"started new[{sessions.Count}] session ({current.processName}/{current.application})",
                    Logger.Level.Trace);
            } catch (ArgumentException) {
                Logger.log($"process {window.processId} did not exist ({window.application})", Logger.Level.Warning);
            }
        }

        private void runKeylogger() {
            _plat.wm.hookUserEvents(globalKeyEvent);
        }

        private void globalKeyEvent(KeyboardEvent kev) {
            if (current != null) {
                lock (current) {
                    if (current != null) {
                        current.keyEvents += 1;
                    }
                }
            }
        }

        public void destroy() {
            database.Dispose();
            _plat.destroy();
        }
    }
}
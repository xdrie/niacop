using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Iri.Glass.Logging;
using Nia.Extensibility;
using Nia.Models;
using Nia.Native;
using SQLite;

namespace Nia.Services {
    public class ActivityTracker {
        private Platform platform;
        private string trackerDataPath;
        private string trackerDatabaseFile;
        public SQLiteConnection? database;
        private IEnumerable<ISessionWatcher> watchers;

        // public List<Session> sessions = new List<Session>();
        public Session? current;

        public ActivityTracker() {
            platform = new Platform();
            trackerDataPath = Path.Combine(DataPaths.profilePath, "tracker");
            trackerDatabaseFile = Path.Combine(trackerDataPath, "activity.db");
            watchers = Extensibility.jar.ResolveAll<ISessionWatcher>();
        }

        public void initialize() {
            Directory.CreateDirectory(Path.GetDirectoryName(trackerDatabaseFile));
            database = new SQLiteConnection(trackerDatabaseFile);
            database.CreateTable<Session>();
            foreach (var eventLogger in watchers) {
                eventLogger.initialize(database);
            }
        }

        public void run(CancellationToken token) {
            if (Global.config!.tracker.keylogger) {
                runKeylogger();
            }

            while (!token.IsCancellationRequested) {
                pollSession();
                // wait 5 seconds between session polls
                Thread.Sleep(Global.config!.tracker.windowPoll);
            }
        }

        private void pollSession() {
            var idleTime = platform.wm.getIdleTime();
            if (idleTime > Global.config!.tracker.idle) {
                if (current != null) {
                    Global.log.trace($"session entered idle state ({idleTime})");
                    endSession();
                }

                return;
            }

            var window = platform.wm.getActiveWindow();
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
                    }
                    else {
                        // call each watcher
                        var sc = new SessionContext(current, window);
                        foreach (var watcher in watchers) {
                            watcher.update(sc);
                        }

                        // update entry
                        current.endTime = Utils.timestamp();
                        database!.Update(current);
                    }
                }
            }
        }

        private void endSession() {
            lock (current!) {
                current.endTime = Utils.timestamp();
                var humanDuration = TimeSpan.FromMilliseconds(current.getDuration());
                Global.log.trace($"  ended session ({humanDuration.TotalSeconds:N2}s/{current.keyEvents}ks)\n");
                // sessions.Add(current);
                database!.Update(current); // save to database
                current = null; // unset current
            }
        }

        private void gatherSession(Window window) {
            if (window == null) return;
            // detect process path
            try {
                var timestampNow = Utils.timestamp();
                var procName = default(string);
                var procPath = default(string);
                if (window.processId > 0) {
                    var proc = Process.GetProcessById(window.processId);
                    procName = proc.ProcessName;
                    procPath = proc.MainModule?.FileName;
                }

                // assign new session 
                var newSession = new Session {
                    application = window.application,
                    windowTitle = window.title,
                    processId = window.processId,
                    processName = procName,
                    processPath = procPath,
                    startTime = timestampNow,
                    endTime = timestampNow,
                };
                if (current == null) {
                    current = newSession;
                }
                else {
                    lock (current) {
                        current = newSession;
                    }
                }

                lock (current!) {
                    database!.Insert(current!);
                    Global.log.trace($"started session ({current!.application}/{current!.processName})");
                }
            }
            catch (ArgumentException) {
                Global.log.warn($"process {window.processId} did not exist ({window.application})");
            }
        }

        private void runKeylogger() {
            platform.wm.hookUserEvents(globalKeyEvent);
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
            database!.Dispose();
            platform.destroy();
        }
    }
}
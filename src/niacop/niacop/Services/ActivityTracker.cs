using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using niacop.Configuration;
using niacop.Native;
using niacop.Native.WindowManagers;
using SQLite;

namespace niacop.Services {
    public class ActivityTracker {
        private Platform _plat;
        private string trackerDataPath;
        private string trackerRunFile;
        private Stream outputStream;

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

            public override string ToString() => $"application({duration})";
        }

        public List<Session> sessions = new List<Session>();
        public Session current;

        public ActivityTracker() {
            _plat = new Platform();
        }

        public void initialize() {
            trackerDataPath = Path.Combine(DataPaths.profilePath, "tracker");
            trackerRunFile = Path.Combine(trackerDataPath, "activity.db");
            Directory.CreateDirectory(Path.GetDirectoryName(trackerRunFile));
        }

        public void run(CancellationToken token) {
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

            if (current == null) {
                gatherSession(window);
            }
            // current can still be null, if gather fails

            if (current != null && current.processId != window.processId) {
                endSession();
                gatherSession(window);
            }
        }

        private void endSession() {
            current.duration = _plat.timestamp() - current.startTime;
            sessions.Add(current);
            current = null; // unset current
        }

        private void gatherSession(Window window) {
            // detect process path
            try {
                var proc = Process.GetProcessById(window.processId);
                current = new Session {
                    application = window.application,
                    windowTitle = window.title,
                    processId = window.processId,
                    processName = proc.ProcessName,
                    processPath = proc.MainModule.FileName,
                    startTime = _plat.timestamp()
                };
                Logger.log($"started new[{sessions.Count}] session ({current.processName}/{current.application})",
                    Logger.Level.Trace);
            } catch (ArgumentException) {
                Logger.log($"process {window.processId} did not exist ({window.application})", Logger.Level.Warning);
            }
        }

        public void destroy() { }
    }
}
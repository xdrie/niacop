using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using niacop.Configuration;
using niacop.Native;
using niacop.Native.WindowManagers;

namespace niacop.Services {
    public class ActivityTracker {
        private Platform _plat;
        private string trackerDataPath;
        private string trackerRunId;
        private string trackerRunFile;
        private Stream outputStream;

        public class Session {
            public string application;
            public string windowTitle;
            public int processId;
            public string processName;
            public string processPath;
            public long startTime;
            public long duration;

            public override string ToString() => $"application({duration})";
        }

        public List<Session> sessions = new List<Session>();
        public Session current;

        public ActivityTracker() {
            _plat = new Platform();
        }

        public void initialize() {
            trackerDataPath = Path.Combine(DataPaths.profilePath, "tracker");
            trackerRunId = $"run_{_plat.timestamp()}.nia";
            trackerRunFile = Path.Combine(trackerDataPath, trackerRunId);
            Directory.CreateDirectory(Path.GetDirectoryName(trackerRunFile));
            outputStream = File.Open(trackerRunFile, FileMode.Create, FileAccess.Write);
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

        public void save() {
            using (var bw = new BinaryWriter(outputStream)) {
                // write header
                bw.Write(new byte[] {0xFD, 0xCF});
                // write version ID
                bw.Write(02);
                // write sessions
                bw.Write(sessions.Count);
                foreach (var session in sessions) {
                    bw.Write(session.application);
                    bw.Write(session.windowTitle);
                    bw.Write(session.processId);
                    bw.Write(session.processName);
                    bw.Write(session.processPath);
                    bw.Write(session.startTime);
                    bw.Write(session.duration);
                }

                bw.Flush();
                Logger.log($"saved tracker run file sessions({sessions.Count}) [{outputStream.Position}B] to {trackerRunFile}",
                    Logger.Level.Trace);
            }
        }

        public void destroy() {
            outputStream.Dispose();
        }
    }
}
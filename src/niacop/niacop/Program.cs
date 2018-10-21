using System;
using System.Runtime.Loader;
using System.Threading;
using niacop.Services;

namespace niacop {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("[niacop]");
            Console.WriteLine("running activity tracker");

            var activityDaemonTokenSource = new CancellationTokenSource();
            var activityDaemon = new ActivityTracker();
            
            // prepare exit handler
            AppDomain.CurrentDomain.ProcessExit += (s, e) => {
                Logger.log("recieved exit signal, cleaning up", Logger.Level.Warning);
                activityDaemonTokenSource.Cancel();
                activityDaemon.save();
                activityDaemon.destroy();
            };
            
            // prepare and run daemon
            activityDaemon.initialize();
            activityDaemon.run(activityDaemonTokenSource.Token);
        }
    }
}
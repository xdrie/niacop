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
            activityDaemon.initialize();
            activityDaemon.run(activityDaemonTokenSource.Token);

            AssemblyLoadContext.Default.Unloading += (context) => {
                Logger.log("recieved unloading signal, cleaning up", Logger.Level.Warning);
                activityDaemonTokenSource.Cancel();
                activityDaemon.save();
                activityDaemon.destroy();
            };
        }
    }
}
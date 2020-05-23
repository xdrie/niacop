using System;
using System.Runtime.Loader;
using System.Threading;
using niacop.Services;

namespace niacop.CLI {
    public class ActivityDaemonRunner : Runner<ActivityDaemonRunner.Options> {
        public class Options {
            
        }

        public override int run(Options options) {
            Global.log.info("running activity tracker");
            var cts = new CancellationTokenSource();
            var tracker = new ActivityTracker();
            tracker.initialize();

            // prepare exit handler
            var unloadHandler = new Action(() => {
                if (!cts.IsCancellationRequested) {
                    Global.log.err("recieved exit signal, cleaning up");
                    cts.Cancel();
                    tracker.destroy();
                }
            });

            AssemblyLoadContext.Default.Unloading += (context) => unloadHandler();
            Console.CancelKeyPress += (s, e) => unloadHandler();

            // prepare and run daemon
            tracker.run(cts.Token);
            
            return 0;
        }
    }
}
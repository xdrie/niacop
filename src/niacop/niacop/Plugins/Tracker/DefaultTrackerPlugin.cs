using CookieIoC;
using niacop.Extensibility;
using niacop.Extensibility.Tracker;

namespace niacop.Plugins.Tracker {
    public class DefaultTrackerPlugin : INiaPlugin {
        public string name => "default trackers";
        
        public void beforeActivation(CookieJar container) {
            container.register<ISessionEventLogger>(new WindowTitleEventLogger());
        }
    }
}
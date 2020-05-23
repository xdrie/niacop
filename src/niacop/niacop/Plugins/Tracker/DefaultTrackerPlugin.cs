using Iri.IoC;
using niacop.Extensibility;
using niacop.Extensibility.Tracker;

namespace niacop.Plugins.Tracker {
    public class DefaultTrackerPlugin : INiaPlugin {
        public string name => "default trackers";
        
        public void BeforeActivation(CookieJar jar) {
            jar.Register<ISessionEventLogger>(new WindowTitleEventLogger());
        }
    }
}
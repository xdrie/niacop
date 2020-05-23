using Iri.IoC;
using Nia.Extensibility;
using Nia.Extensibility.Tracker;

namespace Nia.Plugins.Tracker {
    public class DefaultTrackerPlugin : INiaPlugin {
        public string name => "default trackers";
        
        public void BeforeActivation(CookieJar jar) {
            jar.Register<ISessionWatcher>(new WindowTitleWatcher());
        }
    }
}
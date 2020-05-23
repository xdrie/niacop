using Iri.IoC;
using Nia.Extensibility;

namespace Nia.Plugins {
    public class DefaultWatchersPlugin : INiaPlugin {
        public string name => "default trackers";

        public void BeforeActivation(CookieJar jar) {
            jar.Register<ISessionWatcher>(new WindowTitleWatcher());
        }
    }
}
using Iri.IoC;
using Iri.Plugin;
using Nia.Extensibility;

namespace Nia.Services {
    public static class Extensibility {
        public static PluginLoader<INiaPlugin> loader = new PluginLoader<INiaPlugin>();
        public static CookieJar jar = new CookieJar();
    }
}
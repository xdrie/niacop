using Iri.IoC;
using Iri.Plugin;
using niacop.Extensibility;

namespace niacop.Services {
    public static class Extensibility {
        public static PluginLoader<INiaPlugin> loader = new PluginLoader<INiaPlugin>();
        public static CookieJar jar = new CookieJar();
    }
}
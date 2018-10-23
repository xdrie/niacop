using CookieIoC;
using niacop.Extensibility;
using Osmium.PluginEngine;

namespace niacop.Services {
    public static class ExtensibilityService {
        public static PluginLoader<INiaPlugin> loader = new PluginLoader<INiaPlugin>();
        public static CookieJar registry = new CookieJar();
    }
}
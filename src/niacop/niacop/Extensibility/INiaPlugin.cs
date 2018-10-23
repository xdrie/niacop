using Osmium.PluginEngine.Types;

namespace niacop.Extensibility {
    public interface INiaPlugin : IOsmiumPlugin {
        string name { get; }
    }
}
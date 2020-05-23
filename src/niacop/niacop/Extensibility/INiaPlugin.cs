using Iri.Plugin.Types;

namespace niacop.Extensibility {
    public interface INiaPlugin : IPlugin {
        string name { get; }
    }
}
using Iri.Plugin.Types;

namespace Nia.Extensibility {
    public interface INiaPlugin : IPlugin {
        string name { get; }
    }
}
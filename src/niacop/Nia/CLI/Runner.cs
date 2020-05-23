using System;

namespace Nia.CLI {
    public abstract class Runner<TOptions> : IDisposable {
        public abstract int run(TOptions options);
        public virtual void Dispose() { }
    }
}
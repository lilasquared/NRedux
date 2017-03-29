using System;

namespace NRedux {
    public static partial class Redux {
        internal class MiddlewareApi<TState> : IStoreBase<TState> {
            public TState State { get; internal set; }
            public Dispatcher Dispatch { get; internal set; }
        }
    }
}
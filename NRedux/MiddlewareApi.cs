using System;

namespace NRedux {
    public static partial class Redux {
        internal class MiddlewareApi<TState> : IStoreBase<TState> {
            public Func<TState> GetState { get; internal set; }
            public Dispatcher Dispatch { get; internal set; }
        }
    }
}
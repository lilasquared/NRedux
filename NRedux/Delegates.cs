using System;

namespace NRedux {
    public delegate Object Dispatcher(Object action);

    public delegate CreateStoreDelegate<TState> EnhancerDelegate<TState>(CreateStoreDelegate<TState> createStore);

    public delegate Func<Dispatcher, Dispatcher> Middleware<TState>(IStoreBase<TState> store);
}
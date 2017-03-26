using System;

namespace NRedux {
    /// <summary>
    /// Represents a method that dispatches an action
    /// </summary>
    /// <param name="action">The action to dispatch</param>
    /// <returns></returns>
    public delegate Object Dispatcher(Object action);

    /// <summary>
    /// Represents a method that is used to update the state tree.
    /// </summary>
    /// <typeparam name="TState">The state tree type.</typeparam>
    /// <param name="previousState">The previus state tree.</param>
    /// <param name="action">The action to be applied to the state tree</param>
    /// <returns>The updated state tree.</returns>
    public delegate TState Reducer<TState>(TState previousState, Object action);

    /// <summary>
    /// Represents a method that is used to subscribe to state changes
    /// </summary>
    /// <param name="listener">Callback to execute when the state changes</param>
    /// <returns>Unsubscribe</returns>
    public delegate Action Subscriber(Action listener);

    /// <summary>
    /// Represents a method that is used to create middleware
    /// </summary>
    /// <typeparam name="TState">The application state type</typeparam>
    /// <param name="middlewareApi">The <see cref="IStoreBase{TState}"/> store api that the middleware will have access to once created.</param>
    /// <returns></returns>
    public delegate Func<Dispatcher, Dispatcher> Middleware<TState>(IStoreBase<TState> middlewareApi);

    /// <summary>
    /// Represents a method that is used to create a store
    /// </summary>
    /// <typeparam name="TState">The type of the state tree</typeparam>
    /// <param name="reducer">The root reducer</param>
    /// <param name="preLoadedState">The initial state</param>
    /// <param name="enhancer">The store enhancer</param>
    /// <returns></returns>
    public delegate IStore<TState> StoreCreator<TState>(Reducer<TState> reducer, TState preLoadedState, StoreEnhancer<TState> enhancer = null);

    /// <summary>
    /// Represents a method that is used to enhance the store
    /// </summary>
    /// <typeparam name="TState">The state tree type.</typeparam>
    /// <param name="createStore">The store creator method to be enhanced</param>
    /// <returns>The enhanced store creator</returns>
    public delegate StoreCreator<TState> StoreEnhancer<TState>(StoreCreator<TState> createStore);
}
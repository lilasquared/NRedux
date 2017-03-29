namespace NRedux {
    internal class InitAction { }
    public static partial class Redux {
        /// <summary>
        /// Creates a NRedux store that holds the state tree
        /// The only way to change the data in the store is to call `Dispatch()` on it
        /// 
        /// There should only be a single store in your app.  To specify how different
        /// parts of the state tree respond to actions you may combine several reducers
        /// into a single reducer function by using `CombineReducers()`
        /// </summary>
        /// <typeparam name="TState">The type of the state tree</typeparam>
        /// <param name="reducer">
        ///     A function that returns the next state tree, given 
        ///     the current state tree and the action to handle
        /// </param>
        /// <param name="preLoadedState">
        ///     The inital state.  You may optionally specify it to hydrate the state from
        ///     the server or to restore a previously serialized user session.  If you do 
        ///     not wish to provide an initial state you must pass null.
        /// </param>
        /// <param name="enhancer">
        ///     The store enhancer.  you may optionally specify it to enhance the store with
        ///     third-party capabilities such as middleware, time travel, persistence, etc.
        ///     The only store enhancer that ships with NRedux is `ApplyMiddleware()`
        /// </param>
        /// <returns>
        ///     A NRedux store that lets you read teh state, dispatch actions and subscrive
        ///     to changes
        /// </returns>
        public static IStore<TState> CreateStore<TState>(Reducer<TState> reducer, TState preLoadedState = default(TState), StoreEnhancer<TState> enhancer = null) {
            if (enhancer != null) {
                return enhancer(CreateStore)(reducer, preLoadedState);
            }
            return new Store<TState>(reducer, preLoadedState);
        }
    }
}
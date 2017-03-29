using System.Linq;

namespace NRedux {
    public static partial class Redux {
        /// <summary>
        /// Creates a store enhancer that applies middleware to the dispatch method
        /// of the NRedux store.  This is handy for a variety of tasks, such as expressing
        /// asynchronous actions in a concise manner, or logging every action payload.
        /// 
        /// See `NRedux.Thunk` package as an example of the NRedux middleware
        /// 
        /// Because middleware is potentially asynchronous, this should be the first 
        /// store enhancer in the composition chain.
        /// 
        /// Note that each middleware will be given an <see cref="IStoreBase{TState}" /> parameter.
        /// 
        /// </summary>
        /// <param name="middlewares">The middleware chain to be applied</param>
        /// <returns>A store enhancer applying the middleware</returns>
        public static StoreEnhancer<TState> ApplyMiddleware<TState>(params Middleware<TState>[] middlewares) {
            return createStore => (reducer, preLoadedState, enhancer) => {
                var store = createStore(reducer, preLoadedState, enhancer);
                var dispatch = store.Dispatch;

                var middlewareApi = new MiddlewareApi<TState> {
                    State = store.State,
                    Dispatch = action => dispatch(action)
                };

                var chain = middlewares.Select(middleware => middleware(middlewareApi)).ToArray();
                dispatch = Util.Compose(chain)(store.Dispatch);

                return new Store<TState>(dispatch, store);
            };
        }
    }
}
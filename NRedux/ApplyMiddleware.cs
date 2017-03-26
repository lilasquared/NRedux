using System;
using System.Linq;

namespace NRedux {
    public delegate CreateStoreDelegate<TState> EnhancerDelegate<TState>(CreateStoreDelegate<TState> createStore);
    public delegate Func<Func<Object, Object>, Func<Object, Object>> Middleware<TState>(IStoreBase<TState> store);
    public static partial class Redux<TState> {
        public static EnhancerDelegate<TState> ApplyMiddleware(params Middleware<TState>[] middlewares) {
            return createStore => (reducer, preLoadedState, enhancer) => {
                var store = createStore(reducer, preLoadedState, enhancer);
                var dispatch = store.Dispatch;

                var middlewareApi = new MiddlewareApi {
                    GetState = store.GetState,
                    Dispatch = (action) => dispatch(action)
                };

                var chain = middlewares.Select(middleware => middleware(middlewareApi)).ToArray();
                dispatch = Compose(chain)(store.Dispatch);

                return new Store<TState>(dispatch, store.Subscribe, store.GetState, store.ReplaceReducer);
            };
        }

        internal class MiddlewareApi : IStoreBase<TState> {
            public Func<TState> GetState { get; internal set; }
            public Func<Object, Object> Dispatch { get; internal set; }
        }

        internal static Func<Func<Object, Object>, Func<Object, Object>> Compose(params Func<Func<Object, Object>, Func<Object, Object>>[] funcs) {
            if (funcs.Length == 0) {
                return arg => arg;
            }

            if (funcs.Length == 1) {
                return funcs[0];
            }

            return funcs.Aggregate((a, b) => arg =>  a(b(arg)));
        }
    }
}
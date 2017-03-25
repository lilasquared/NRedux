using System;
using System.Linq;

namespace NRedux {
    internal class InitAction { }

    public delegate Store<TState> CreateStoreDelegate<TState>(Reducer<TState> reducer, TState preLoadedState);

    public static partial class Redux {
        public static Store<TState> CreateStore<TState>(Reducer<TState> reducer, TState preLoadedState) {
            var currentReducer = reducer;
            var currentState = preLoadedState;
            var currentListeners = new Action[0];
            var nextListeners = currentListeners.ToList();
            var isDispatching = false;

            Action ensureCanMutateNextListeners = () => {
                if (nextListeners.Equals(currentListeners)) {
                    nextListeners = currentListeners.ToList();
                }
            };

            Func<Reducer<TState>, TState, Store<TState>> createStore = CreateStore;

            Func<TState> getState = () => currentState;

            Func<Action, Action> subscribe = listener => {
                if (listener == null) {
                    throw new Exception("Expected listener to not be null");
                }

                var isSubscribed = true;

                ensureCanMutateNextListeners();
                nextListeners.Add(listener);

                Action unsubscribe = () => {
                    if (!isSubscribed) {
                        return;
                    }

                    isSubscribed = false;

                    ensureCanMutateNextListeners();
                    nextListeners.Remove(listener);
                };

                return unsubscribe;
            };

            Func<Object, Object> dispatch = action => {
                if (Util.IsPrimitiveOrNull(action)) {
                    throw new Exception("Actions must be objects. Use custom middleware for async actions");
                }

                if (isDispatching) {
                    throw new Exception("Reducers may not dispatch actions");
                }

                try {
                    isDispatching = true;
                    currentState = currentReducer(currentState, action);
                }
                finally {
                    isDispatching = false;
                }

                var listeners = currentListeners = nextListeners.ToArray();
                foreach (var listener in listeners) {
                    listener();
                }

                return action;
            };

            Action<Reducer<TState>> replaceReducer = nextReducer => {
                currentReducer = nextReducer;
                dispatch(new InitAction());
            };

            dispatch(new InitAction());

            return new Store<TState>(dispatch, subscribe, getState, replaceReducer);
        }
    }
}
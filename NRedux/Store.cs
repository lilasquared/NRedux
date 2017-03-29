using System;
using System.Collections.Generic;
using System.Linq;

namespace NRedux {
    public interface IStoreBase<out TState> {
        TState State { get; }
        Dispatcher Dispatch { get; }
    }

    public interface IStore<TState> : IStoreBase<TState> {
        event StateChangeHandler<TState> StateChanged;

        Action<Reducer<TState>> ReplaceReducer { get; }
    }

    public class Store<TState> : IStore<TState> {
        private readonly Object _syncroot = new Object();
        private readonly Queue<StateChangeHandler<TState>> _subscribeQueue = new Queue<StateChangeHandler<TState>>();
        private readonly Queue<StateChangeHandler<TState>> _unsubscribeQueue = new Queue<StateChangeHandler<TState>>();

        private Boolean _isDispatching;
        private Reducer<TState> _reducer;
        private StateChangeHandler<TState> _stateChangeHandler;

        internal Store(Reducer<TState> reducer, TState preLoadedState = default(TState)) {
            Dispatch = _innerDispatch;
            ReplaceReducer = _innerReplaceReducer;
            _reducer = reducer;
            State = preLoadedState;
            Dispatch(new InitAction());
        }

        internal Store(Dispatcher dispatch, IStore<TState> innerStore) {
            Dispatch = dispatch;
            State = innerStore.State;
            ReplaceReducer = innerStore.ReplaceReducer;
        }

        public TState State { get; private set; }

        public event StateChangeHandler<TState> StateChanged {
            add {
                if (_isDispatching) {
                    throw new Exception("You  may not add an event handler while the reducer is executing.");
                }

                _stateChangeHandler += value;
                value(State);
                //_subscribeQueue.Enqueue(value);
            }
            remove {
                _stateChangeHandler -= value; 
                //_unsubscribeQueue.Enqueue(value); 
            }
        }

        public Dispatcher Dispatch { get; }

        public Action<Reducer<TState>> ReplaceReducer { get; }

        private Object _innerDispatch(Object action) {
            if (Util.IsPrimitiveOrNull(action)) {
                throw new Exception("Actions must be objects. Use custom middleware for async actions");
            }

            lock (_syncroot) {
                if (_isDispatching) {
                    throw new Exception("Reducers may not dispatch actions");
                }

                try {
                    _isDispatching = true;
                    State = _reducer(State, action);
                }
                finally {
                    _isDispatching = false;
                }

                //_updateSubscriptions();
                _stateChangeHandler?.Invoke(State);
            }

            return action;
        }

        private void _innerReplaceReducer(Reducer<TState> newReducer) {
            _reducer = newReducer;
            Dispatch(new InitAction());
        }

        private void _updateSubscriptions() {
            while (_subscribeQueue.Any()) {
                _stateChangeHandler += _subscribeQueue.Dequeue();
            }
            while (_unsubscribeQueue.Any()) {
                _stateChangeHandler -= _unsubscribeQueue.Dequeue();
            }
        }
    }
}
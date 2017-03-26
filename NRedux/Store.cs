using System;

namespace NRedux {
    public interface IStoreBase<TState> {
        Func<TState> GetState { get; }
        Dispatcher Dispatch { get; }
    }

    public interface IStore<TState> : IStoreBase<TState> {
        Subscriber Subscribe { get; }
        Action<Reducer<TState>> ReplaceReducer { get; }
    }

    public class Store<TState> : IStore<TState> {
        public Store(Dispatcher dispatch, Subscriber subscribe, Func<TState> getState, Action<Reducer<TState>> replaceReducer) {
            GetState = getState;
            Subscribe = subscribe;
            Dispatch = dispatch;
            ReplaceReducer = replaceReducer;
        }
        public Func<TState> GetState { get; }
        public Subscriber Subscribe { get; }
        public Dispatcher Dispatch { get; }
        public Action<Reducer<TState>> ReplaceReducer { get; }
    }
}
using System;
using System.Linq;
using System.Reflection;
using NRedux.Exceptions;

namespace NRedux {
    public delegate TState Reducer<TState>(TState state, Object action);
    public static partial class Redux {
        public static Reducer<TState> CombineReducers<TState, TReducers>() where TState : new() {
            var stateType = typeof(TState);
            var reducerMethods = typeof(TReducers).GetMethods(BindingFlags.Public | BindingFlags.Static);
            var properties = stateType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);

            var reducerKeys = reducerMethods.Select(methodInfo => methodInfo.Name).OrderBy(x => x).ToArray();
            var propertyKeys = properties.Select(propertyInfo => propertyInfo.Name).OrderBy(x => x).ToArray();

            if (!reducerKeys.ArrayEquals(propertyKeys)) {
                throw new PropertyMismatchException();
            }

            var reducerReturnTypes = reducerMethods
                .ToDictionary(methodInfo => methodInfo.Name, methodInfo => methodInfo.ReturnType);
            var propertyTypes = properties
                .ToDictionary(propertyInfo => propertyInfo.Name, propertyInfo => propertyInfo.PropertyType);

            foreach (var item in reducerReturnTypes) {
                if (!reducerReturnTypes[item.Key].Equals(propertyTypes[item.Key])) {
                    throw new PropertyTypeMismatchException();
                }
            }

            return (state, action) => {
                var hasChanged = false;
                var nextState = Activator.CreateInstance(stateType);
                foreach (var reducer in reducerMethods) {
                    var previousStateForReducer = stateType.GetProperty(reducer.Name).GetValue(state);
                    var nextStateForReducer = reducer.Invoke(null, new Object[] { previousStateForReducer, action });
                    stateType.GetProperty(reducer.Name).SetValue(nextState, nextStateForReducer);
                    hasChanged = hasChanged || !nextStateForReducer.Equals(previousStateForReducer);
                }

                return hasChanged ? (TState) nextState : state;
            };
        }
    }
}
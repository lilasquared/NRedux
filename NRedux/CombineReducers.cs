using System;
using System.Linq;
using System.Reflection;
using NRedux.Exceptions;

namespace NRedux {
    public static partial class Redux {
        /// <summary>
        /// Turns all public static reducer functions from TReducers into a single reducer function.  
        /// It will call every reducer and gather their results into a single object whose 
        /// keys correspond to the keys of the reducer functions.
        /// </summary>
        /// <typeparam name="TState">The type of the state tree.</typeparam>
        /// <typeparam name="TReducers">The type of the reducer class.</typeparam>
        /// <returns>
        ///     A reducer function that invokes every public static reducer method defined on
        ///     TReducers and maps the results to the corresponding Properties of TState
        /// </returns>
        [Obsolete("While cool that this works, this functionality is not needed. See <TODO> for examples on creating complex reducers.  Do not use this")]
        public static Reducer<TState> CombineReducers<TState, TReducers>() {
            var stateType = typeof(TState);
            var reducerMethods = typeof(TReducers).GetMethods(BindingFlags.Public | BindingFlags.Static);
            var stateProperties = stateType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);

            var reducerKeys = reducerMethods.Select(methodInfo => methodInfo.Name).OrderBy(x => x).ToArray();
            var stateKeys = stateProperties.Select(propertyInfo => propertyInfo.Name).OrderBy(x => x).ToArray();

            if (!reducerKeys.ArrayEquals(stateKeys)) {
                throw new KeyMismatchException(stateKeys, reducerKeys);
            }

            var reducerReturnTypes = reducerMethods
                .OrderBy(methodInfo => methodInfo.Name)
                .ToDictionary(methodInfo => methodInfo.Name, methodInfo => methodInfo.ReturnType);
            var statePropertyTypes = stateProperties
                .OrderBy(propertyInfo => propertyInfo.Name)
                .ToDictionary(propertyInfo => propertyInfo.Name, propertyInfo => propertyInfo.PropertyType);
            
            if (!reducerReturnTypes.Values.ToArray().ArrayEquals(statePropertyTypes.Values.ToArray())) {
                throw new KeyTypeMismatchException(statePropertyTypes, reducerReturnTypes);
            }

            return (state, action) => {
                var hasChanged = false;
                var nextState = Activator.CreateInstance(stateType);
                foreach (var reducer in reducerMethods) {
                    var previousStateForReducer = stateType.GetProperty(reducer.Name).GetValue(state);
                    var nextStateForReducer = reducer.Invoke(null, new [] { previousStateForReducer, action });
                    stateType.GetProperty(reducer.Name).SetValue(nextState, nextStateForReducer);
                    hasChanged = hasChanged || !nextStateForReducer.Equals(previousStateForReducer);
                }

                return hasChanged ? (TState) nextState : state;
            };
        }
    }
}
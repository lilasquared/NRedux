# NRedux

NRedux is a predictable state container for .NET apps.  Inspired by https://github.com/reactjs/redux.

[![Build status](https://ci.appveyor.com/api/projects/status/04vk5lxghc02dcxn?svg=true)](https://ci.appveyor.com/project/lilasquared/nredux)
[![NuGet version](https://badge.fury.io/nu/NRedux.svg)](https://badge.fury.io/nu/NRedux)

## Table of Contents
 - [Installation](#installation)
 - [Quick-Start](#quick-start)
 
## Installation

The latest version of NRedux is available on [Nuget](https://www.nuget.org/packages/NRedux)
```
  Install-Package NRedux
```

## Quick-Start

### Actions
*Actions* are payloads of information that send data from your application to your *store*.  An action can be pretty much anything - except value types.
```csharp
public class IncrementAction { }

public class DecrementAction { }

public class AddTodoAction {
    public String Message { get; set; }
}
```

### Reducers
A *reducer* is a [pure function](https://en.wikipedia.org/wiki/Pure_function) with `(TState previousState, Object action) => TState nextState` signature.
It describes how an action transforms the application state into the next state.

If there is no action to perform for a particular reducer, you should return the previous state.

The shape of the state is up to you.  It can be primitive, and array, or an object.
The only important part is that you should not mutate the state object, but return a new object if the state changes.

```csharp
namespace NRedux.CounterApp {
    public partial class Reducer {
        public static Int32 Count(Int32 previousState, Object action) {
            if (action is IncrementAction incrementAction) {
                return previousState + 1;
            }
            
            if (action is DecrementAction decrementAction) {
                return previousState - 1;
            }
            
            return previousState;
        }
    }
}
```

There are a few things of note for reducers which will be touched upon later.  The reducer above is a public static method.  Since reducers are pure functions - they should not rely on *anything* that is not directly passed into the function.  Because of this it is safe to make them static and they are super easy to test.

```csharp
using Xunit;

namespace NRedux.CounterApp.Tests {
    public class ReducerTests {
        [Fact]
        public void Increments_State_When_Action_Is_IncrementAction() {
            var newState = Reducer.Count(0, new IncrementAction());

            Assert.Equal(1, newState);
        }

        [Fact]
        public void Decrements_State_When_Action_Is_DecrementAction() {
            var newState = Reducer.Count(0, new DecrementAction());

            Assert.Equal(-1, newState);
        }
    }
}
```

### Store

The Store is the class that brings *actions* and *reducers* together, along with third party *middleware*.  The store has the following responsibilities.

* Holds the application state of type TState
* Allows state to be updated via Dispatch(Object action)
* Registers listeners via Subscribe(Action listener)

The store can be enhanced by third party middleware that doesn't ship with NRedux, like logging, async tasks, or undo.

It is important to not that you should only ever have a single store in an NRedux application.  Since the store implements IStore<TState> it would be beneficial to implement some kind of dependency injection for your app.  In the examples it is just defined as a static property of the application class.

The store is created using a static factory method `Redux.CreateStore` (will change to NRedux.CreateStore for v1.0)

```csharp
namespace NRedux.TodoApp {
    class App {
        private static IStore<AppState> _store;
        static void Main(string[] args) {
            var rootReducer = Redux.CombineReducers<AppState, Reducer>();
            _store = Redux.CreateStore(rootReducer, new AppState());
            // other logic
        }
    }
}
```

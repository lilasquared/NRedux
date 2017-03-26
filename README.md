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
            if (action as IncrementAction incrementAction) {
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

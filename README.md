# NRedux

NRedux is a predictable state container for .NET apps.  Inspired by https://github.com/reactjs/redux. and https://github.com/GuillaumeSalles/redux.NET.  It is a port of the public javascript api that feels javascripty.  See Redux.NET for a more dot-net like approach.

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
First, let's define some actions

*Actions* are payloads of information that send data from your application to your *store*. They are the *only* source of information for the store. You send them to the store using `store.Dispatch()`.

```csharp
public class AddTodoAction {
    public String Message { get; set; }
}
```

Actions are C# POCO classes.  Actions should not be primitive types, and NRedux prohibits actions from being primitive types.

The structure of an action class is entirely up to you.

We'll add one more action type to describe a user ticking off a todo as completed.  We refer to a particular todo by `Id`.

```csharp
public class ToggleTodoAction {
    public Int32 TodoId { get; set; }
}
```

It's a good idea to pass as little data in each action as possible.  For example, it's better to pass `id` that the whole todo object.

Finally we'll add one more action type for changing the currently visible todos.

```csharp
public class SetVisibilityFilterAction {
    public VisibilityFilter Filter { get; set; }
}
```
#### Action Creators
**Action Creators** are exactly that - functions that create actions.  In C# this will most likey be the class constructor, however if there are more complex actions being constructed then static factory methods would work as well.  It is easy to conflate the terms "action" and "action creator", so do your best to use the proper term.

In NRedux.TodoApp the action creators are simply the constructor of the action class, or the default constructor.

```csharp
namespace NRedux.TodoApp.Actions {
    public class AddTodoAction {
        public String Message { get; set; }
        public AddTodoAction(String message) {
            Message = message;
        }
    }
}
```
This makes them very portable and very easy to test.

To dispatch an action simply pass the result of the action creator to the `store.Dispatch()` function:
```csharp
store.Dispatch(new AddTodoAction("Do Laundry"));
store.Dispatch(new ToggleTodoAction { TodoId = 1 });
```

### Reducers
A *reducer* is a  with signature.
It describes how an action transforms the application state into the next state.

#### Designing the State Shape
In NRedux, all the application state is stored as a single object.  It is a good idea to think of its shape before writing any code.  What is the minimal representation of your app's state as an object?

For the NRedux.TodoApp, I store three different things:
* The currently selected visibility filter
* The actual list of todos
* A boolean flag that indicates the user typed an unrecognized command.

```csharp
namespace NRedux.TodoApp {
    public class AppState {
        public Boolean InvalidAction { get; set; }
        public VisibilityFilter VisibilityFilter { get; set; }
        public Todo[] Todos { get; set; }

        public AppState() {
            VisibilityFilter = VisibilityFilter.All;
            Todos = new Todo[0];
        }
    }
}
```

I am using the constructor of my `AppState` class to create the initial state of my application. You will see later that this can be passed in to the `CreateStore` method as the initial state, *or* I can leave it up to the reducers themselves to decide what the initial state is going to be.

#### Handling Actions
Now that we have decided what our state object looks like, we're ready to write a reducer for it.  The reducer is a [pure function](https://en.wikipedia.org/wiki/Pure_function) that takes in the previous state and an action, and returns the next state.
 ```csharp
 (TState previousState, Object action) => TState nextState
```

It is called a reducer because it is the type of function you would pass into `Array.prototype.reduce(reducer, ?initialValue)` from javascript.  It is very important that a reducer stays pure. Things you should **never** do inside a reducer:
* Mutate its arguments
* Perform side effects like API calls and routing transitions
* Call non-pure functions eg `DateTime.Now` or `Math.Random()`

We'll explore how to perform side effects in the [advanced walkthrough](). For now just remember that the reduce must be pure.  **Given the same arguments, it should calculate the next state and return it. No surprises. No side effects. No API calls. No mutations.  Just a calculation.**

With this out of the way, let's start writing our reducer by gradually teaching it to understand the actions we defined earlier.

We'll start by specifying the initial state. Redux will call our reducer with an undefined state for the first time. This is our chance to return the initial state of our app:

```csharp
namespace NRedux.TodoApp.Reducers {
    public class Reducer {
        private static  AppState initialState = new AppState() {
            VisibilityFilter = Reducers.VisibilityFilter.All,
            Todos = new Models.Todo[0]
        };

        public static AppState Reduce(AppState state, Object action) {
            if (state == null) {
                return initialState;
            }
            // For now, don't handle any actions
            // and just return the state given to us.
            return state;
        }
    }
```

Now let's handle the 

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

#### Combine-Reducers

### Store

The Store is the class that brings *actions* and *reducers* together, along with third party *middleware*.  The store has the following responsibilities.

* Holds the application state of type `TState`
* Allows acces to the state via `TState GetState()`
* Allows state to be updated via `Object Dispatch(Object action)`
* Registers listeners via `Action Subscribe(Action listener)`
* Handles unregistering of listeners via the Action returned by `Subscribe(Action listener)`

It is important to note that you should only ever have a single store in an NRedux application.  When you want to split your data handling logic you'll use *reducer composition* instead of many stores.  Since the store implements IStore<TState> it would be beneficial to implement some kind of dependency injection for your app.  In the examples it is just defined as a static property of the application class.

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

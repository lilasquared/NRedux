using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static NRedux.Test.Helpers;

namespace NRedux.Test {
    public class CreateStoreTests {
        [Fact]
        public void Passes_Initial_Action_And_Initial_State() {
            var store = Redux.CreateStore(TodosReducer, new[] {
                new Todo {
                    Id = 1,
                    Message = "Hello"
                }
            });

            Assert.Equal(1, store.State.Length);
            Assert.Equal(1, store.State.First().Id);
            Assert.Equal("Hello", store.State.First().Message);
        }

        [Fact]
        public void Dispatches_Once_On_Store_Creation() {
            var store = Redux.CreateStore((state, action) => state + 1, 0);

            Assert.Equal(1, store.State);
        }

        [Fact]
        public void Applies_The_Reducer_To_Previous_State() {
            var store = Redux.CreateStore(TodosReducer);
            Assert.Equal(new Todo[] { }, store.State);

            store.Dispatch(new UnknownAction());
            Assert.Equal(new Todo[] { }, store.State);

            store.Dispatch(new AddTodoAction("Hello"));
            Assert.Equal(new[] {
                new Todo {
                    Id = 1,
                    Message = "Hello"
                }
            }, store.State);

            store.Dispatch(new AddTodoAction("World"));
            Assert.Equal(new[] {
                new Todo {
                    Id = 1,
                    Message = "Hello"
                },
                new Todo {
                    Id = 2,
                    Message = "World"
                }
            }, store.State);
        }

        [Fact]
        public void Applies_The_Reducer_To_The_Initial_State() {
            var store = Redux.CreateStore(TodosReducer, new[] {
                new Todo {
                    Id = 1,
                    Message = "Hello"
                }
            });
            Assert.Equal(new[] {
                new Todo {
                    Id = 1,
                    Message = "Hello"
                }
            }, store.State);

            store.Dispatch(new UnknownAction());
            Assert.Equal(new[] {
                new Todo {
                    Id = 1,
                    Message = "Hello"
                }
            }, store.State);

            store.Dispatch(new AddTodoAction("World"));
            Assert.Equal(new[] {
                new Todo {
                    Id = 1,
                    Message = "Hello"
                },
                new Todo {
                    Id = 2,
                    Message = "World"
                }
            }, store.State);
        }

        [Fact]
        public void Preservers_The_State_When_Replacing_A_Reducer() {
            var store = Redux.CreateStore(TodosReducer);
            store.Dispatch(new AddTodoAction("Hello"));
            store.Dispatch(new AddTodoAction("World"));
            Assert.Equal(new[] {
                new Todo {
                    Id = 1,
                    Message = "Hello"
                },
                new Todo {
                    Id = 2,
                    Message = "World"
                }
            }, store.State);

            store.ReplaceReducer(TodosReducerReverse);
            Assert.Equal(new[] {
                new Todo {
                    Id = 1,
                    Message = "Hello"
                },
                new Todo {
                    Id = 2,
                    Message = "World"
                }
            }, store.State);

            store.Dispatch(new AddTodoAction("Perhaps"));
            Assert.Equal(new[] {
                new Todo {
                    Id = 3,
                    Message = "Perhaps"
                },
                new Todo {
                    Id = 1,
                    Message = "Hello"
                },
                new Todo {
                    Id = 2,
                    Message = "World"
                }
            }, store.State);

            store.ReplaceReducer(TodosReducer);
            Assert.Equal(new[] {
                new Todo {
                    Id = 3,
                    Message = "Perhaps"
                },
                new Todo {
                    Id = 1,
                    Message = "Hello"
                },
                new Todo {
                    Id = 2,
                    Message = "World"
                }
            }, store.State);

            store.Dispatch(new AddTodoAction("Surely"));
            Assert.Equal(new[] {
                new Todo {
                    Id = 3,
                    Message = "Perhaps"
                },
                new Todo {
                    Id = 1,
                    Message = "Hello"
                },
                new Todo {
                    Id = 2,
                    Message = "World"
                },
                new Todo {
                    Id = 4,
                    Message = "Surely"
                }
            }, store.State);
        }

        [Fact]
        public void Supports_Multiple_Subscriptions() {
            var store = Redux.CreateStore(TodosReducer);

            var timesListenerACalled = 0;
            var timesListenerBCalled = 0;

            StateChangeHandler<Todo[]> listenerA = state => timesListenerACalled++;
            StateChangeHandler<Todo[]> listenerB = state => timesListenerBCalled++;

            store.StateChanged += listenerA;
            store.Dispatch(new UnknownAction());
            Assert.Equal(1, timesListenerACalled);
            Assert.Equal(0, timesListenerBCalled);

            store.Dispatch(new UnknownAction());
            Assert.Equal(2, timesListenerACalled);
            Assert.Equal(0, timesListenerBCalled);

            store.StateChanged += listenerB;
            Assert.Equal(2, timesListenerACalled);
            Assert.Equal(0, timesListenerBCalled);

            store.Dispatch(new UnknownAction());
            Assert.Equal(3, timesListenerACalled);
            Assert.Equal(1, timesListenerBCalled);

            store.StateChanged -= listenerA;
            Assert.Equal(3, timesListenerACalled);
            Assert.Equal(1, timesListenerBCalled);

            store.Dispatch(new UnknownAction());
            Assert.Equal(3, timesListenerACalled);
            Assert.Equal(2, timesListenerBCalled);

            store.StateChanged -= listenerB;
            Assert.Equal(3, timesListenerACalled);
            Assert.Equal(2, timesListenerBCalled);

            store.Dispatch(new UnknownAction());
            Assert.Equal(3, timesListenerACalled);
            Assert.Equal(2, timesListenerBCalled);

            store.StateChanged += listenerA;
            Assert.Equal(3, timesListenerACalled);
            Assert.Equal(2, timesListenerBCalled);

            store.Dispatch(new UnknownAction());
            Assert.Equal(4, timesListenerACalled);
            Assert.Equal(2, timesListenerBCalled);
        }

        [Fact]
        public void Only_Removes_Listener_Once_When_Unsubscribe_Is_Called() {
            var store = Redux.CreateStore(TodosReducer);
            var timesListenerACalled = 0;
            var timesListenerBCalled = 0;

            StateChangeHandler<Todo[]> listenerA = state => timesListenerACalled++;
            StateChangeHandler<Todo[]> listenerB = state => timesListenerBCalled++;

            store.StateChanged += listenerA;
            store.StateChanged += listenerB;

            store.StateChanged -= listenerA;
            store.StateChanged -= listenerA;

            store.Dispatch(new UnknownAction());
            Assert.Equal(0, timesListenerACalled);
            Assert.Equal(1, timesListenerBCalled);
        }

        [Fact]
        public void Supports_Removing_A_Subscription_Within_A_Subscription() {
            var store = Redux.CreateStore(TodosReducer);
            var timesListenerACalled = 0;
            var timesListenerBCalled = 0;
            var timesListenerCCalled = 0;

            StateChangeHandler<Todo[]> listenerA = state => timesListenerACalled++;
            StateChangeHandler<Todo[]> listenerB = state => timesListenerBCalled++;
            StateChangeHandler<Todo[]> listenerC = state => timesListenerCCalled++;

            store.StateChanged += listenerA;
            store.StateChanged += listenerB;
            store.StateChanged += state => store.StateChanged -= listenerB;
            store.StateChanged += listenerC;

            store.Dispatch(new UnknownAction());
            store.Dispatch(new UnknownAction());

            Assert.Equal(2, timesListenerACalled);
            Assert.Equal(1, timesListenerBCalled);
            Assert.Equal(2, timesListenerCCalled);
        }

        [Fact]
        public void Delays_Unsubscribe_Unitl_The_End_Of_Current_Dispatch() {
            var store = Redux.CreateStore(TodosReducer);

            IEnumerable<StateChangeHandler<Todo[]>> eventHandles = null;

            var timesListenerACalled = 0;
            var timesListenerBCalled = 0;
            var timesListenerCCalled = 0;

            Action doUnsubscribeAll = () => {
                foreach (var eventHandle in eventHandles) {
                    store.StateChanged -= eventHandle;
                }
            };

            StateChangeHandler<Todo[]> listenerA = state => timesListenerACalled++;
            StateChangeHandler<Todo[]> listenerB = state => {
                timesListenerBCalled++;
                doUnsubscribeAll();
            };
            StateChangeHandler<Todo[]> listenerC = state => timesListenerCCalled++;

            eventHandles = new List<StateChangeHandler<Todo[]>> {
                listenerA, listenerB, listenerC
            };

            store.StateChanged += listenerA;
            store.StateChanged += listenerB;
            store.StateChanged += listenerC;

            store.Dispatch(new UnknownAction());
            Assert.Equal(1, timesListenerACalled);
            Assert.Equal(1, timesListenerBCalled);
            Assert.Equal(1, timesListenerCCalled);

            store.Dispatch(new UnknownAction());
            Assert.Equal(1, timesListenerACalled);
            Assert.Equal(1, timesListenerBCalled);
            Assert.Equal(1, timesListenerCCalled);
        }

        [Fact]
        public void Uses_The_Last_Snapshot_Of_Subscribers_During_Nested_Dispatch() {
            var store = Redux.CreateStore(TodosReducer);
            var timesListener1Called = 0;
            var timesListener2Called = 0;
            var timesListener3Called = 0;
            var timesListener4Called = 0;

            StateChangeHandler<Todo[]> listener1 = state => timesListener1Called++;
            StateChangeHandler<Todo[]> listener2 = state => timesListener2Called++;
            StateChangeHandler<Todo[]> listener3 = state => timesListener3Called++;
            StateChangeHandler<Todo[]> listener4 = state => timesListener4Called++;

            StateChangeHandler<Todo[]> unsubMe = null;
            unsubMe = state => {
                listener1(state);
                Assert.Equal(1, timesListener1Called);
                Assert.Equal(0, timesListener2Called);
                Assert.Equal(0, timesListener3Called);
                Assert.Equal(0, timesListener4Called);

                store.StateChanged -= unsubMe;
                store.StateChanged += listener4;
                store.Dispatch(new UnknownAction());

                Assert.Equal(1, timesListener1Called);
                Assert.Equal(1, timesListener2Called);
                Assert.Equal(1, timesListener3Called);
                Assert.Equal(1, timesListener4Called);
            };

            store.StateChanged += unsubMe;
            store.StateChanged += listener2;
            store.StateChanged += listener3;

            store.Dispatch(new UnknownAction());
            Assert.Equal(1, timesListener1Called);
            Assert.Equal(2, timesListener2Called);
            Assert.Equal(2, timesListener3Called);
            Assert.Equal(1, timesListener4Called);

            store.StateChanged -= listener4;
            store.Dispatch(new UnknownAction());
            Assert.Equal(1, timesListener1Called);
            Assert.Equal(3, timesListener2Called);
            Assert.Equal(3, timesListener3Called);
            Assert.Equal(1, timesListener4Called);
        }

        [Fact]
        public void Provides_Up_To_Date_State_When_A_Subscriber_Is_Notified() {
            var store = Redux.CreateStore(TodosReducer);
            store.StateChanged += state => {
                Assert.Equal(new[] {
                    new Todo {
                        Id = 1,
                        Message = "Hello"
                    }
                }, state);
            };
            store.Dispatch(new AddTodoAction("Hello"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData(new Boolean())]
        [InlineData(new SByte())]
        [InlineData(new Byte())]
        [InlineData(new Int16())]
        [InlineData(new UInt16())]
        [InlineData(new Int32())]
        [InlineData(new UInt32())]
        [InlineData(new Int64())]
        [InlineData(new UInt64())]
        [InlineData(new Single())]
        [InlineData(new Double())]
        [InlineData('0')]
        [InlineData("0")]
        public void Does_Not_Allow_Value_TypeActions(Object action) {
            var store = Redux.CreateStore(TodosReducer);

            Assert.Throws<Exception>(() => store.Dispatch(action));
        }

        [Fact]
        public void Does_Not_Allow_Dispatch_From_Within_Reducer() {
            var store = Redux.CreateStore(BoundActionInMiddleReducer);

            Assert.Throws<Exception>(() => {
                store.Dispatch(new ActionWithBoundFn(() => {
                    store.Dispatch(new UnknownAction());
                }));
            });
        }

        [Fact]
        public void Does_Not_Allow_GetState_From_Within_Reducer() {
            var store = Redux.CreateStore(BoundActionInMiddleReducer);

            Assert.Throws<Exception>(() => {
                store.Dispatch(new ActionWithBoundFn(() => {
                    var state = store.State;
                }));
            });
        }

        [Fact]
        public void Does_Not_Allow_Subscribe_From_Within_Reducer() {
            var store = Redux.CreateStore(BoundActionInMiddleReducer);

            Assert.Throws<Exception>(() => {
                store.Dispatch(new ActionWithBoundFn(() => {
                    store.StateChanged += state => { };
                }));
            });
        }

        [Fact]
        public void Does_Not_Allow_Unsubscribe_From_Within_Reducer() {
            var store = Redux.CreateStore(BoundActionInMiddleReducer);
            StateChangeHandler<Int32> listener1 = state => { };

            store.StateChanged += listener1;
            Assert.Throws<Exception>(() => {
                store.Dispatch(new ActionWithBoundFn(() => store.StateChanged -= listener1));
            });
        }

        [Fact]
        public void Recovers_From_Error_Within_Reducer() {
            var store = Redux.CreateStore(ThrowErrorReducer);

            Assert.Throws<Exception>(() => store.Dispatch(new ErrorAction()));
            store.Dispatch(new UnknownAction());
        }

        [Fact]
        public async void Is_Thread_Safe() {
            var store = Redux.CreateStore((state, action) => state + 1, 0);

            await Task.WhenAll(
                Enumerable
                    .Range(0, 1000)
                    .Select(x => Task.Factory.StartNew(() => store.Dispatch(new UnknownAction()))));

            Assert.Equal(1001, store.State);
        }
    }
}
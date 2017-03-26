using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NRedux.Test {
    public class CreateStoreTests {
        [Fact]
        public void Passes_Initial_Action_And_Initial_State() {
            var store = Redux.CreateStore(Helpers.TodosReducer, new[] {
                new Helpers.Todo {
                    Id = 1,
                    Message = "Hello"
                }
            });

            Assert.Equal(1, store.GetState().Length);
            Assert.Equal(1, store.GetState().First().Id);
            Assert.Equal("Hello", store.GetState().First().Message);
        }

        [Fact]
        public void Applies_The_Reducer_To_Previous_State() {
            var store = Redux.CreateStore(Helpers.TodosReducer, null);
            Assert.Equal(new Helpers.Todo[] { }, store.GetState());

            store.Dispatch(new Helpers.UnknownAction());
            Assert.Equal(new Helpers.Todo[] { }, store.GetState());

            store.Dispatch(new Helpers.AddTodoAction("Hello"));
            Assert.Equal(new[] {
                new Helpers.Todo {
                    Id = 1,
                    Message = "Hello"
                }
            }, store.GetState());

            store.Dispatch(new Helpers.AddTodoAction("World"));
            Assert.Equal(new[] {
                new Helpers.Todo {
                    Id = 1,
                    Message = "Hello"
                },
                new Helpers.Todo {
                    Id = 2,
                    Message = "World"
                }
            }, store.GetState());
        }

        [Fact]
        public void Applies_The_Reducer_To_The_Initial_State() {
            var store = Redux.CreateStore(Helpers.TodosReducer, new[] {
                new Helpers.Todo {
                    Id = 1,
                    Message = "Hello"
                }
            });
            Assert.Equal(new[] {
                new Helpers.Todo {
                    Id = 1,
                    Message = "Hello"
                }
            }, store.GetState());

            store.Dispatch(new Helpers.UnknownAction());
            Assert.Equal(new[] {
                new Helpers.Todo {
                    Id = 1,
                    Message = "Hello"
                }
            }, store.GetState());

            store.Dispatch(new Helpers.AddTodoAction("World"));
            Assert.Equal(new[] {
                new Helpers.Todo {
                    Id = 1,
                    Message = "Hello"
                },
                new Helpers.Todo {
                    Id = 2,
                    Message = "World"
                }
            }, store.GetState());
        }

        [Fact]
        public void Preservers_The_State_When_Replacing_A_Reducer() {
            var store = Redux.CreateStore(Helpers.TodosReducer, null);
            store.Dispatch(new Helpers.AddTodoAction("Hello"));
            store.Dispatch(new Helpers.AddTodoAction("World"));
            Assert.Equal(new[] {
                new Helpers.Todo {
                    Id = 1,
                    Message = "Hello"
                },
                new Helpers.Todo {
                    Id = 2,
                    Message = "World"
                }
            }, store.GetState());

            store.ReplaceReducer(Helpers.TodosReducerReverse);
            Assert.Equal(new[] {
                new Helpers.Todo {
                    Id = 1,
                    Message = "Hello"
                },
                new Helpers.Todo {
                    Id = 2,
                    Message = "World"
                }
            }, store.GetState());

            store.Dispatch(new Helpers.AddTodoAction("Perhaps"));
            Assert.Equal(new[] {
                new Helpers.Todo {
                    Id = 3,
                    Message = "Perhaps"
                },
                new Helpers.Todo {
                    Id = 1,
                    Message = "Hello"
                },
                new Helpers.Todo {
                    Id = 2,
                    Message = "World"
                }
            }, store.GetState());

            store.ReplaceReducer(Helpers.TodosReducer);
            Assert.Equal(new[] {
                new Helpers.Todo {
                    Id = 3,
                    Message = "Perhaps"
                },
                new Helpers.Todo {
                    Id = 1,
                    Message = "Hello"
                },
                new Helpers.Todo {
                    Id = 2,
                    Message = "World"
                }
            }, store.GetState());

            store.Dispatch(new Helpers.AddTodoAction("Surely"));
            Assert.Equal(new[] {
                new Helpers.Todo {
                    Id = 3,
                    Message = "Perhaps"
                },
                new Helpers.Todo {
                    Id = 1,
                    Message = "Hello"
                },
                new Helpers.Todo {
                    Id = 2,
                    Message = "World"
                },
                new Helpers.Todo {
                    Id = 4,
                    Message = "Surely"
                }
            }, store.GetState());
        }

        [Fact]
        public void Supports_Multiple_Subscriptions() {
            var store = Redux.CreateStore(Helpers.TodosReducer, null);

            var listenerA = new Mock<Action>();
            var listenerB = new Mock<Action>();

            var unsubscribeA = store.Subscribe(listenerA.Object);
            store.Dispatch(new Helpers.UnknownAction());
            listenerA.Verify(x => x(), Times.Exactly(1));
            listenerB.Verify(x => x(), Times.Exactly(0));

            store.Dispatch(new Helpers.UnknownAction());
            listenerA.Verify(x => x(), Times.Exactly(2));
            listenerB.Verify(x => x(), Times.Exactly(0));

            var unsubscribeB = store.Subscribe(listenerB.Object);
            listenerA.Verify(x => x(), Times.Exactly(2));
            listenerB.Verify(x => x(), Times.Exactly(0));

            store.Dispatch(new Helpers.UnknownAction());
            listenerA.Verify(x => x(), Times.Exactly(3));
            listenerB.Verify(x => x(), Times.Exactly(1));

            unsubscribeA();
            listenerA.Verify(x => x(), Times.Exactly(3));
            listenerB.Verify(x => x(), Times.Exactly(1));

            store.Dispatch(new Helpers.UnknownAction());
            listenerA.Verify(x => x(), Times.Exactly(3));
            listenerB.Verify(x => x(), Times.Exactly(2));

            unsubscribeB();
            listenerA.Verify(x => x(), Times.Exactly(3));
            listenerB.Verify(x => x(), Times.Exactly(2));

            store.Dispatch(new Helpers.UnknownAction());
            listenerA.Verify(x => x(), Times.Exactly(3));
            listenerB.Verify(x => x(), Times.Exactly(2));

            store.Subscribe(listenerA.Object);
            listenerA.Verify(x => x(), Times.Exactly(3));
            listenerB.Verify(x => x(), Times.Exactly(2));

            store.Dispatch(new Helpers.UnknownAction());
            listenerA.Verify(x => x(), Times.Exactly(4));
            listenerB.Verify(x => x(), Times.Exactly(2));
        }

        [Fact]
        public void Only_Removes_Listener_Once_When_Unsubscribe_Is_Called() {
            var store = Redux.CreateStore(Helpers.TodosReducer, null);
            var listenerA = new Mock<Action>();
            var listenerB = new Mock<Action>();

            var unsubscribeA = store.Subscribe(listenerA.Object);
            store.Subscribe(listenerB.Object);

            unsubscribeA();
            unsubscribeA();

            store.Dispatch(new Helpers.UnknownAction());
            listenerA.Verify(x => x(), Times.Exactly(0));
            listenerB.Verify(x => x(), Times.Exactly(1));
        }

        [Fact]
        public void Supports_Removing_A_Subscription_Within_A_Subscription() {
            var store = Redux.CreateStore(Helpers.TodosReducer, null);
            var listenerA = new Mock<Action>();
            var listenerB = new Mock<Action>();
            var listenerC = new Mock<Action>();

            store.Subscribe(listenerA.Object);
            var unSubB = store.Subscribe(listenerB.Object);
            store.Subscribe(() => {
                unSubB();
            });
            store.Subscribe(listenerC.Object);

            store.Dispatch(new Helpers.UnknownAction());
            store.Dispatch(new Helpers.UnknownAction());

            listenerA.Verify(x => x(), Times.Exactly(2));
            listenerB.Verify(x => x(), Times.Exactly(1));
            listenerC.Verify(x => x(), Times.Exactly(2));
        }

        [Fact]
        public void Delays_Unsubscribe_Unitl_The_End_Of_Current_Dispatch() {
            var store = Redux.CreateStore(Helpers.TodosReducer, null);

            var unsubscribeHandles = new List<Action>();
            Action doUnsubscribeAll = () => {
                foreach (var unsubscribeHandle in unsubscribeHandles) {
                    unsubscribeHandle();
                }
            };

            var listener1 = new Mock<Action>();
            var listener2 = new Mock<Action>();
            var listener3 = new Mock<Action>();

            unsubscribeHandles.Add(store.Subscribe(() => listener1.Object()));
            unsubscribeHandles.Add(store.Subscribe(() => {
                listener2.Object();
                doUnsubscribeAll();
            }));
            unsubscribeHandles.Add(store.Subscribe(() => listener3.Object()));

            store.Dispatch(new Helpers.UnknownAction());
            listener1.Verify(x => x(), Times.Exactly(1));
            listener2.Verify(x => x(), Times.Exactly(1));
            listener3.Verify(x => x(), Times.Exactly(1));

            store.Dispatch(new Helpers.UnknownAction());
            listener1.Verify(x => x(), Times.Exactly(1));
            listener2.Verify(x => x(), Times.Exactly(1));
            listener3.Verify(x => x(), Times.Exactly(1));
        }

        [Fact]
        public void Uses_The_Last_Snapshot_Of_Subscribers_During_Nested_Dispatch() {
            var store = Redux.CreateStore(Helpers.TodosReducer, null);

            var listener1 = new Mock<Action>();
            var listener2 = new Mock<Action>();
            var listener3 = new Mock<Action>();
            var listener4 = new Mock<Action>();

            Action unsubscribe4 = null;
            Action unsubscribe1 = null;
            unsubscribe1 = store.Subscribe(() => {
                listener1.Object();

                listener1.Verify(x => x(), Times.Exactly(1));
                listener2.Verify(x => x(), Times.Exactly(0));
                listener3.Verify(x => x(), Times.Exactly(0));
                listener4.Verify(x => x(), Times.Exactly(0));

                unsubscribe1?.Invoke();
                unsubscribe4 = store.Subscribe(listener4.Object);
                store.Dispatch(new Helpers.UnknownAction());

                listener1.Verify(x => x(), Times.Exactly(1));
                listener2.Verify(x => x(), Times.Exactly(1));
                listener3.Verify(x => x(), Times.Exactly(1));
                listener4.Verify(x => x(), Times.Exactly(1));
            });
            store.Subscribe(listener2.Object);
            store.Subscribe(listener3.Object);

            store.Dispatch(new Helpers.UnknownAction());
            listener1.Verify(x => x(), Times.Exactly(1));
            listener2.Verify(x => x(), Times.Exactly(2));
            listener3.Verify(x => x(), Times.Exactly(2));
            listener4.Verify(x => x(), Times.Exactly(1));

            unsubscribe4?.Invoke();
            store.Dispatch(new Helpers.UnknownAction());
            listener1.Verify(x => x(), Times.Exactly(1));
            listener2.Verify(x => x(), Times.Exactly(3));
            listener3.Verify(x => x(), Times.Exactly(3));
            listener4.Verify(x => x(), Times.Exactly(1));
        }

        [Fact]
        public void Provides_Up_To_Date_State_When_A_Subscriber_Is_Notified() {
            var store = Redux.CreateStore(Helpers.TodosReducer, null);
            store.Subscribe(() => {
                Assert.Equal(new[] {
                    new Helpers.Todo {
                        Id = 1,
                        Message = "Hello"
                    }
                }, store.GetState());
            });
            store.Dispatch(new Helpers.AddTodoAction("Hello"));
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
            var store = Redux.CreateStore(Helpers.TodosReducer, null);

            Assert.Throws<Exception>(() => store.Dispatch(action));
        }

        [Fact]
        public void Does_Not_Allow_Dispatch_From_Within_Reducer() {
            var store = Redux.CreateStore(Helpers.DispatchInMiddleReducer, 0);

            Assert.Throws<Exception>(() => {
                store.Dispatch(new Helpers.ActionWithBoundFn(() => {
                    store.Dispatch(new Helpers.UnknownAction());
                }));
            });
        }

        [Fact]
        public void Recovers_From_Error_Within_Reducer() {
            var store = Redux.CreateStore(Helpers.ThrowErrorReducer, 0);

            Assert.Throws<Exception>(() => store.Dispatch(new Helpers.ErrorAction()));
            store.Dispatch(new Helpers.UnknownAction());
        }
    }
}
using System;
using System.Threading.Tasks;
using Xunit;

namespace NRedux.Test {
    public class ApplyMiddlewareTests {

        [Fact]
        public void Wraps_Dispatch_With_Middleware_Once() {
            var timesCalled = 0;
            Action stub = () => timesCalled++;

            var store = Redux.ApplyMiddleware(TestSpy<Helpers.Todo[]>(stub), Helpers.Thunk)(Redux.CreateStore)(Helpers.TodosReducer, null);

            store.Dispatch(new Helpers.AddTodoAction("Use Redux"));
            store.Dispatch(new Helpers.AddTodoAction("Flux FTW!"));

            var state = store.State;

            Assert.Equal(1, timesCalled);
            Assert.Equal(2, state.Length);
            Assert.Equal(1, state[0].Id);
            Assert.Equal("Use Redux", state[0].Message);
            Assert.Equal(2, state[1].Id);
            Assert.Equal("Flux FTW!", state[1].Message);
        }

        [Fact]
        public async void Passes_Recursive_Dispatches_Through_The_Middleware_Chain() {
            var timesCalled = 0;
            Action stub = () => timesCalled++;

            var store = Redux.ApplyMiddleware(TestRecursive<Helpers.Todo[]>(stub), Helpers.Thunk)(Redux.CreateStore)(Helpers.TodosReducer, null);
            var promise = store.Dispatch(Helpers.AddTodoAsync("Use Redux")) as Task;
            await promise.ContinueWith(task => Assert.Equal(2, timesCalled));
        }

        [Fact]
        public async void Works_With_Thunk_Middleware() {
            var store = Redux.ApplyMiddleware(Helpers.Thunk)(Redux.CreateStore)(Helpers.TodosReducer, null);

            var expectedState = new[] {
                new Helpers.Todo {
                    Id = 1,
                    Message = "Hello"
                }
            };

            store.Dispatch(Helpers.AddTodoIfEmpty("Hello"));
            Assert.Equal(expectedState, store.State);

            store.Dispatch(Helpers.AddTodoIfEmpty("Hello"));
            Assert.Equal(expectedState, store.State);

            expectedState = new[] {
                new Helpers.Todo {
                    Id = 1,
                    Message = "Hello"
                },
                new Helpers.Todo {
                    Id = 2,
                    Message = "World"
                }
            };

            store.Dispatch(new Helpers.AddTodoAction("World"));
            Assert.Equal(expectedState, store.State);

            expectedState = new[] {
                new Helpers.Todo {
                    Id = 1,
                    Message = "Hello"
                },
                new Helpers.Todo {
                    Id = 2,
                    Message = "World"
                },
                new Helpers.Todo {
                    Id = 3,
                    Message = "Maybe"
                }
            };

            var promise = store.Dispatch(Helpers.AddTodoAsync("Maybe")) as Task;
            await promise.ContinueWith(task => {
                Assert.Equal(expectedState, store.State);
            });
        }

        public Middleware<TState> TestSpy<TState>(Action spy) {
            return methods => {
                spy();
                return next => action => next(action);
            };
        }

        public Middleware<TState> TestRecursive<TState>(Action spy) {
            return store => next => action => {
                spy();
                return next(action);
            };
        }
    }
}
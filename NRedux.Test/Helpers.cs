using System;
using System.Collections.Generic;
using System.Linq;

namespace NRedux.Test {
    internal class Helpers {
        public class AddTodoAction {
            public String Message { get; }

            public AddTodoAction(String message) {
                Message = message;
            }
        }

        public class UnknownAction { }

        public class ActionWithBoundFn {
            public Action BoundAction { get; }
            public ActionWithBoundFn(Action action) {
                BoundAction = action;
            }
        }

        public class ErrorAction { }

        public class IncrementAction { }

        public class PushAction {
            public Int32 Value { get; set; }
        }

        public class Todo {
            public Int32 Id { get; set; }
            public String Message { get; set; }

            public override Boolean Equals(Object obj) {
                var compare = (Todo)obj;

                if (compare == null) return false;

                return compare.Id.Equals(Id) && compare.Message.Equals(Message);
            }

            public override Int32 GetHashCode() {
                unchecked {
                    return (Id * 397) ^ (Message?.GetHashCode() ?? 0);
                }
            }
        }

        private static readonly Func<Todo[], Int32> GetId = state => {
            return state.Length > 0
                ? state.Max(item => item.Id) + 1
                : 1;
        };

        public static Reducer<Todo[]> TodosReducer = (state, action) => {
            var currentState = state ?? new Todo[] { };

            if (action is AddTodoAction) {
                var castAction = (AddTodoAction)action;
                var nextState = currentState.ToList();
                nextState.Add(new Todo {
                    Id = GetId(currentState),
                    Message = castAction.Message
                });
                return nextState.ToArray();
            }
            return currentState;
        };

        public static Reducer<Todo[]> TodosReducerReverse = (state, action) => {
            var currentState = state ?? new Todo[] { };

            if (action is AddTodoAction) {
                var castAction = (AddTodoAction)action;
                var nextState = currentState.ToList();
                nextState.Insert(0, new Todo {
                    Id = GetId(currentState),
                    Message = castAction.Message
                });
                return nextState.ToArray();
            }
            return currentState;
        };

        public static Reducer<Int32> DispatchInMiddleReducer = (state, action) => {
            if (action is ActionWithBoundFn) {
                var castAction = (ActionWithBoundFn)action;
                castAction.BoundAction();
            }
            return state;
        };

        public static Reducer<Int32> ThrowErrorReducer = (state, action) => {
            if (action is ErrorAction) {
                throw new Exception("Inside Reducer");
            }
            return state;
        };

        public static Reducer<Int32> Count = (state, action) => {
            if (action is IncrementAction) {
                return state + 1;
            }
            return state;
        };

        public static Reducer<List<Int32>> Push = (state, action) => {
            if (action is PushAction) {
                var castAction = action as PushAction;
                var nextState = state.ToList();
                nextState.Add(castAction.Value);
                return nextState;
            }
            return state;
        };
    }
}
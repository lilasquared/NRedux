using NRedux.Exceptions;
using System;
using System.Linq;
using Xunit;

namespace NRedux.Test {
    public class CombineReducersTests {

        private class State {
            public Int32 Counter { get; set; }
            public String[] Stack { get; set; }
        }

        private class IncrementAction { }
        private class StackAction {
            public String Value { get; set; }
        }

        private class NonMatchingStateProperties {
            public Int32 Counter { get; set; }
            public String[] Queue { get; set; }
        }

        private class NonMatchingStatePropertyTypes {
            public Int32 Counter { get; set; }
            public Int32[] Stack { get; set; }
        }

        private class Reducer {
            public static Int32 Counter(Int32 state, Object action) {
                if (action is IncrementAction) {
                    return state + 1;
                }
                return state;
            }

            public static String[] Stack(String[] state, Object action) {
                state = state ?? new String[0];

                if (action is StackAction) {
                    var castAction = action as StackAction;
                    var nextState = state.ToList();
                    nextState.Add(castAction.Value);
                    return nextState.ToArray();
                }
                return state;
            }
        }

        [Fact]
        public void Returns_A_Composite_Reducer_That_Maps_The_State_Keys_To_Given_Reducers() {
            var reducer = Redux.CombineReducers<State, Reducer>();

            var state1 = reducer(new State(), new IncrementAction());
            Assert.Equal(1, state1.Counter);
            Assert.Equal(new String[0], state1.Stack);
            var state2 = reducer(state1, new StackAction { Value = "a" });
            Assert.Equal(1, state2.Counter);
            Assert.Equal(new String[] { "a" }, state2.Stack);
        }

        [Fact]
        public void Throws_An_Error_If_Public_State_Properties_Do_Not_Match_Public_Static_Reducer_Methods() {
            Assert.Throws<KeyMismatchException>(() => Redux.CombineReducers<NonMatchingStateProperties, Reducer>());
        }

        [Fact]
        public void ThrowsErrorIfPublicStatePropertyTypesDoNotPatchPublicStaticReducerMethodReturnTypes() {
            Assert.Throws<KeyTypeMismatchException>(() => Redux.CombineReducers<NonMatchingStatePropertyTypes, Reducer>());
        }
    }
}
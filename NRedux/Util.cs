using System;
using System.Collections.Generic;
using System.Linq;

namespace NRedux {
    internal static class Util {
        internal static Boolean IsPrimitiveOrNull(Object action) {
            return action == null
                   || action is Boolean
                   || action is SByte
                   || action is Byte
                   || action is Int16
                   || action is UInt16
                   || action is Int32
                   || action is UInt32
                   || action is Int64
                   || action is UInt64
                   || action is Single
                   || action is Double
                   || action is Decimal
                   || action is DateTime
                   || action is Char
                   || action is String;
        }

        internal static Func<Dispatcher, Dispatcher> Compose(params Func<Dispatcher, Dispatcher>[] funcs) {
            if (funcs.Length == 0) {
                return arg => arg;
            }

            if (funcs.Length == 1) {
                return funcs[0];
            }

            return funcs.Aggregate((a, b) => arg => a(b(arg)));
        }

        internal static Boolean ArrayEquals<T>(this T[] a1, T[] a2) {
            if (ReferenceEquals(a1, a2)) {
                return true;
            }

            if (a1.Length != a2.Length) {
                return false;
            }

            var comparer = EqualityComparer<T>.Default;
            for (var i = 0; i < a1.Length; i++) {
                if (!comparer.Equals(a1[i], a2[i])) {
                    return false;
                }
            }
            return true;
        }
        public static void ForEach<T>(this IEnumerable<T> array, Action<T> action) {
            foreach (var item in array) {
                action(item);
            }
        }
    }
}
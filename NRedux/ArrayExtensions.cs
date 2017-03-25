using System;
using System.Collections.Generic;

namespace NRedux {
    public static class ArrayExtensions {
        public static Boolean ArrayEquals<T>(this T[] a1, T[] a2) {
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
    }
}
using System;

namespace NRedux.Exceptions {
    public class PropertyTypeMismatchException : Exception {
        public PropertyTypeMismatchException() : base("Public state property types and public static reducer method return types must match 1-to-1") {

        }
    }
}
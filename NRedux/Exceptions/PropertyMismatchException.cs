using System;

namespace NRedux.Exceptions {
    public class PropertyMismatchException : Exception {
        public PropertyMismatchException() : base("Public state properties and public static reducer methods must match 1-to-1") {
        }
    }
}
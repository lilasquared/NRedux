using System;
using System.Collections.Generic;
using System.Text;

namespace NRedux.Exceptions {
    public class KeyMismatchException : Exception {
        public KeyMismatchException(IEnumerable<String> stateKeys, IEnumerable<String> reducerKeys)
            : base(GetKeyMismatchExceptionMessage(stateKeys, reducerKeys)) { }

        private static String GetKeyMismatchExceptionMessage(IEnumerable<String> stateKeys, IEnumerable<String> reducerKeys) {
            return
                "Public state properties and public static reducer methods must match 1-to-1" + Environment.NewLine +
              $"    State Keys   : <{String.Join(", ", stateKeys)}>" + Environment.NewLine +
              $"    Reducer Keys : <{String.Join(", ", reducerKeys)}>";
        }
    }
}
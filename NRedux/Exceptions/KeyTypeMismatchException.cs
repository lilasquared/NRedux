using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRedux.Exceptions {
    public class KeyTypeMismatchException : Exception {
        public KeyTypeMismatchException(IDictionary<String, Type> statePropertyTypes, IDictionary<String, Type> reducerReturnTypes) 
            : base(GetMessage(statePropertyTypes, reducerReturnTypes)) { }

        private static String GetMessage(IDictionary<String, Type> statePropertyTypes, IDictionary<String, Type> reducerReturnTypes) {
            var builder = new StringBuilder();
            builder.AppendLine("Public state property types and public static reducer method return types must match 1-to-1");
            builder.AppendLine("  State: { ");
            statePropertyTypes.ForEach(kvp => builder.AppendLine($"    {kvp.Key} : <{kvp.Value.Name}>"));
            builder.AppendLine("  }");
            builder.AppendLine("  Reducers: { ");
            reducerReturnTypes.ForEach(kvp => builder.AppendLine($"    {kvp.Key} : <{kvp.Value.Name}>"));
            builder.AppendLine("  }");
            return builder.ToString();
        }
    }
}
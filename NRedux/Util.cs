using System;

namespace NRedux {
    internal class Util {
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
    }
}
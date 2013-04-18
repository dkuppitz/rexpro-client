namespace Rexster
{
    using System;
    using System.Collections.Generic;

    using Rexster.Messages;

    internal static class TypeExtensions
    {
        public static bool IsDynamicScriptResponse(this Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (ScriptResponse<>))
            {
                type = type.GetGenericArguments()[0];
                return type.IsDynamic();
            }
            return false;
        }

        private static bool IsDynamic(this Type type)
        {
            if (type == typeof (object))
            {
                return true;
            }
            if (type.HasElementType)
            {
                return type.GetElementType().IsDynamic();
            }
            if (type.IsGenericType && typeof (IEnumerable<object>).IsAssignableFrom(type))
            {
                return type.GetGenericArguments()[0].IsDynamic();
            }
            return false;
        }
    }
}
namespace Rexster
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;

    using MsgPack;

    internal static class UnpackerExtensions
    {
        public static dynamic UnpackDynamicObject(this Unpacker unpacker)
        {
            if (unpacker.IsMapHeader)
            {
                var result = new ExpandoObject();
                var map = (IDictionary<string, object>) result;

                for (long i = 0, j = unpacker.ItemsCount; i < j; i++)
                {
                    if (unpacker.Read())
                    {
                        var key = unpacker.Unpack<string>();
                        if (unpacker.Read())
                        {
                            map.Add(key, unpacker.UnpackDynamicObject());
                        }
                    }
                }

                return result;
            }

            if (unpacker.IsArrayHeader)
            {
                var items = new ArrayList();

                for (long i = 0, j = unpacker.ItemsCount; i < j; i++)
                {
                    if (unpacker.Read())
                    {
                        items.Add(unpacker.UnpackDynamicObject());
                    }
                }

                return items.ToArray();
            }

            return unpacker.Data.GetValueOrDefault().ToObject();
        }
    }
}
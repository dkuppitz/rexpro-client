namespace Rexster
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Text.RegularExpressions;

    using MsgPack;

    internal static class UnpackerExtensions
    {
        static readonly Regex GraphItemPropertyRegex = new Regex("^_(id|type|properties|inV|outV|label)$", RegexOptions.Compiled);

        public static dynamic UnpackDynamicObject(this Unpacker unpacker)
        {
            if (unpacker.IsMapHeader)
            {
                var result = new ExpandoObject();
                var map = (IDictionary<string, object>) result;
                var isGraphItem = true;

                for (long i = 0, j = unpacker.ItemsCount; i < j; i++)
                {
                    if (unpacker.Read())
                    {
                        var key = unpacker.Unpack<string>();
                        if (unpacker.Read())
                        {
                            map.Add(key, unpacker.UnpackDynamicObject());
                        }
                        isGraphItem &= GraphItemPropertyRegex.IsMatch(key);
                    }
                }

                if (isGraphItem)
                {
                    switch (map["_type"] as string)
                    {
                        case "vertex":
                            return Vertex.FromMap(map);

                        case "edge":
                            return Edge.FromMap(map);
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
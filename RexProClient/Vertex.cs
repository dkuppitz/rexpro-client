namespace Rexster
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class Vertex<T> : GraphItem<T>
    {
    }

    [DataContract]
    public class Vertex : Vertex<dynamic>
    {
        internal static Vertex FromMap(IDictionary<string, object> map)
        {
            var result = new Vertex { Id = map["_id"] as string };

            object value;
            if (map.TryGetValue("_properties", out value))
            {
                result.Data = value;
            }

            return result;
        }
    }
}
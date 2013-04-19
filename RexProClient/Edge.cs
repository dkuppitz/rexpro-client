namespace Rexster
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class Edge<T> : GraphItem<T>
    {
        [DataMember(Name = "_inV")]
        public string InVertex { get; set; }

        [DataMember(Name = "_outV")]
        public string OutVertex { get; set; }

        [DataMember(Name = "_label")]
        public string Label { get; set; }
    }

    [DataContract]
    public class Edge : Edge<dynamic>
    {
        internal static Edge FromMap(IDictionary<string, object> map)
        {
            var result = new Edge
            {
                Id = map["_id"] as string,
                InVertex = map["_inV"] as string,
                OutVertex = map["_outV"] as string,
                Label = map["_label"] as string
            };

            object value;
            if (map.TryGetValue("_properties", out value))
            {
                result.Data = value;
            }

            return result;
        }
    }
}
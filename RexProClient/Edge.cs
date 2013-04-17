namespace Rexster
{
    using System.Runtime.Serialization;

    [DataContract]
    public class Edge
    {
        [DataMember(Name = "_id")]
        public string Id { get; set; }

        [DataMember(Name = "_inV")]
        public string InVertex { get; set; }

        [DataMember(Name = "_outV")]
        public string OutVertex { get; set; }

        [DataMember(Name = "_label")]
        public string Label { get; set; }
    }
}
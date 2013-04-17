namespace Rexster
{
    using System.Runtime.Serialization;

    [DataContract]
    public class Vertex
    {
        [DataMember(Name = "_id")]
        public string Id { get; set; }
    }

    [DataContract]
    public class Vertex<T> : Vertex
    {
        [DataMember(Name = "_properties")]
        public T Data { get; set; }
    }
}
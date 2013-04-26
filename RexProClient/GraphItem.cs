namespace Rexster
{
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class GraphItem
    {
        [DataMember(Name = "_id")]
        public string Id;
    }

    [DataContract]
    public abstract class GraphItem<T> : GraphItem
    {
        [DataMember(Name = "_properties")]
        public T Data;
    }
}
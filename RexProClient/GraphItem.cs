namespace Rexster
{
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class GraphItem<T>
    {
        [DataMember(Name = "_id")]
        public string Id;

        [DataMember(Name = "_properties")]
        public T Data;
    }
}
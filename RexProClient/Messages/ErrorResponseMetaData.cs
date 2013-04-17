namespace Rexster.Messages
{
    using System.Runtime.Serialization;

    [DataContract]
    public class ErrorResponseMetaData
    {
        [DataMember(Name = "flag")]
        public int Flag { get; set; }
    }
}
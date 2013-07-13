namespace Rexster.Messages
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    [DataContract]
    public class ErrorResponseMetaData
    {
        [JsonProperty("flag")]
        public int Flag { get; set; }
    }
}
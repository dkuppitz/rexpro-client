namespace Rexster
{
    using Newtonsoft.Json;

    public class Edge<T> : GraphItem<T>
    {
        [JsonProperty("_inV")]
        public string InVertex { get; set; }

        [JsonProperty("_outV")]
        public string OutVertex { get; set; }

        [JsonProperty("_label")]
        public string Label { get; set; }
    }

    public class Edge : Edge<dynamic>
    {
    }
}
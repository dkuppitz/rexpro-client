namespace Rexster
{
    using Newtonsoft.Json;

    public abstract class GraphItem
    {
        [JsonProperty("_id")] public string Id;
    }

    public abstract class GraphItem<T> : GraphItem
    {
        [JsonProperty("_properties")] public T Data;
    }
}
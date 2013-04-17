namespace Rexster.Tests
{
    using System.Runtime.Serialization;

    [DataContract]
    public class TestVertex
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}
namespace Rexster.Messages
{
    public class GraphSettings
    {
        public static readonly GraphSettings Default = new GraphSettings();

        public GraphSettings(string graphName = "graph", string graphObjName = "g")
        {
            this.GraphName = graphName;
            this.GraphObjectName = graphObjName;
        }

        public string GraphName { get; set; }
        public string GraphObjectName { get; set; }
    }
}
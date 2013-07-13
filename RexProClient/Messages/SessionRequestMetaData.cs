namespace Rexster.Messages
{
    public class SessionRequestMetaData : IRequestMetaData
    {
        private string graphName;
        private string graphObjName;
        private bool killSession;

        public SessionRequestMetaData() : this(null)
        {
        }

        public SessionRequestMetaData(GraphSettings settings, bool killSession = false)
        {
            settings = settings ?? GraphSettings.Default;

            this.graphName = settings.GraphName;
            this.graphObjName = settings.GraphObjectName;
            this.killSession = killSession;
        }

        public string GraphName
        {
            get { return this.graphName; }
            set { this.graphName = value; }
        }

        public string GraphObjectName
        {
            get { return this.graphObjName; }
            set { this.graphObjName = value; }
        }

        public bool KillSession
        {
            get { return this.killSession; }
            set { this.killSession = value; }
        }

        public object ToSerializableObject()
        {
            return new
            {
                this.graphName,
                this.graphObjName,
                this.killSession
            };
        }
    }
}
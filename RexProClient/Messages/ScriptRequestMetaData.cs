namespace Rexster.Messages
{
    public class ScriptRequestMetaData : IRequestMetaData
    {
        private bool inSession;
        private bool isolate;
        private bool transaction;
        private string graphName;
        private string graphObjName;

        public ScriptRequestMetaData()
        {
            this.isolate = true;
            this.transaction = true;
        }

        public bool InSession
        {
            get { return this.inSession; }
            set { this.inSession = value; }
        }

        public bool Isolate
        {
            get { return this.isolate; }
            set { this.isolate = value; }
        }

        public bool Transaction
        {
            get { return this.transaction; }
            set { this.transaction = value; }
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

        public object ToSerializableObject()
        {
            if (this.inSession)
            {
                return new
                {
                    this.inSession,
                    this.isolate,
                    this.transaction
                };
            }

            return new
            {
                this.inSession,
                this.isolate,
                this.transaction,
                this.graphName,
                this.graphObjName
            };
        }
    }
}
namespace Rexster.Messages
{
    using MsgPack;

    public class ScriptRequestMetaData : IPackable
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
            this.graphName = "graph";
            this.graphObjName = "g";
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

        public void PackToMessage(Packer packer, PackingOptions options)
        {
            packer.PackMapHeader(this.inSession ? 4 : 6)
                  .PackString("channel")
                  .Pack(Channel.MsgPack)
                  .PackString("inSession")
                  .Pack(this.inSession)
                  .PackString("isolate")
                  .Pack(this.isolate)
                  .PackString("transaction")
                  .Pack(this.transaction);

            if (!this.inSession)
            {
                packer.PackString("graphName")
                      .PackString(this.graphName)
                      .PackString("graphObjName")
                      .PackString(this.graphObjName);
            }
        }
    }
}
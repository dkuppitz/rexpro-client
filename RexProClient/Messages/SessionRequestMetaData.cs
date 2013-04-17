namespace Rexster.Messages
{
    using MsgPack;

    public class SessionRequestMetaData : IPackable
    {
        private string graphName;
        private string graphObjName;
        private bool killSession;

// ReSharper disable RedundantArgumentDefaultValue
        public SessionRequestMetaData() : this(null, false)
// ReSharper restore RedundantArgumentDefaultValue
        {

        }

        public SessionRequestMetaData(GraphSettings settings = null, bool killSession = false)
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

        public void PackToMessage(Packer packer, PackingOptions options)
        {
            packer.PackMapHeader(3);
            packer.PackString("graphName");
            packer.Pack(this.graphName);
            packer.PackString("graphObjName");
            packer.Pack(this.graphObjName);
            packer.PackString("killSession");
            packer.Pack(this.killSession);
        }
    }
}
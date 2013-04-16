namespace Rexster.Messages
{
    using System;
    using MsgPack;

    public class ScriptRequestMetaData : IPackable
    {
        private readonly int channel;
        private bool inSession;
        private bool isolate;
        private bool transaction;
        private string graphName;
        private string graphObjName;

        public ScriptRequestMetaData()
        {
            this.channel = Messages.Channel.MsgPack;
            this.isolate = true;
            this.transaction = true;
            this.graphName = "graph";
            this.graphObjName = "g";
        }

        public int Channel
        {
            get { return this.channel; }
            set
            {
                // why not simply a read-only property?
                // -> MsgPack (de)serialization needs the setter
                //
                // * channel in request messages must always be 2 (MsgPack)
                // * channel in response messages will always be 2 (MsgPack)
                if (Messages.Channel.MsgPack != value)
                {
                    throw new NotSupportedException();
                }
            }
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
            packer.PackMapHeader(this.inSession ? 4 : 6);
            packer.PackString("channel");
            packer.Pack(this.channel);
            packer.PackString("inSession");
            packer.Pack(this.inSession);
            packer.PackString("isokate");
            packer.Pack(this.isolate);
            packer.PackString("transaction");
            packer.Pack(this.transaction);

            if (!this.inSession)
            {
                packer.PackString("graphName");
                packer.Pack(this.graphName);
                packer.PackString("graphObjName");
                packer.Pack(this.graphObjName);
            }
        }
    }
}
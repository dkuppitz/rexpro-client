namespace Rexster.Messages
{
    using System;
    using MsgPack;

    public class MetaData : IPackable, IUnpackable
    {
        private int flag;
        private int channel;
        private bool inSession;
        private bool isolate;
        private bool transaction;
        private string graphName;
        private string graphObjName;

        public MetaData()
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
            set { this.channel = value; }
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
            packer.PackMapHeader(6);
            packer.PackString("channel");
            packer.Pack(this.channel);
            packer.PackString("inSession");
            packer.Pack(this.inSession);
            packer.PackString("isokate");
            packer.Pack(this.isolate);
            packer.PackString("transaction");
            packer.Pack(this.transaction);
            packer.PackString("graphName");
            packer.Pack(this.graphName);
            packer.PackString("graphObjName");
            packer.Pack(this.graphObjName);
        }

        public void UnpackFromMessage(Unpacker unpacker)
        {
            if (unpacker.IsMapHeader)
            {
                string key;
                for (long i = 0, j = unpacker.ItemsCount; i < j && unpacker.ReadString(out key); i++)
                {
                    switch (key)
                    {
                        case "flag":
                            unpacker.ReadInt32(out this.flag);
                            break;

                        default:
                            throw new Exception(string.Concat("Unexpected key: ", key));
                    }
                }
            }
        }
    }
}
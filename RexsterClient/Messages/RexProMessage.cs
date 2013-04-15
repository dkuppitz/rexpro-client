namespace Rexster.Messages
{
    using MsgPack.Serialization;

    public abstract class RexProMessage
    {
        [MessagePackMember(0)]
        public byte[] Session { get; set; }

        [MessagePackMember(1)]
        public byte[] Request { get; set; }
    }

    public abstract class RexProMessage<TMetaData> : RexProMessage
    {
        [MessagePackMember(2)]
        public TMetaData Meta { get; set; }
    }
}
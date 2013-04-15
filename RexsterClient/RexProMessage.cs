namespace Rexster
{
    using MsgPack.Serialization;

    public abstract class RexProMessage
    {
        [MessagePackMember(0)]
        public byte[] Session { get; set; }

        [MessagePackMember(1)]
        public byte[] Request { get; set; }

        [MessagePackMember(2)]
        public MetaData Meta { get; set; }
    }
}
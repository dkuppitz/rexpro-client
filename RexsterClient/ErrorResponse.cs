namespace Rexster
{
    using MsgPack.Serialization;

    public class ErrorResponse : RexProMessage
    {
        [MessagePackMember(3)]
        public string ErrorMessage { get; set; }
    }
}
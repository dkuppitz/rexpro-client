namespace Rexster.Messages
{
    using MsgPack.Serialization;

    public class ErrorResponse : RexProMessage<ErrorResponseMetaData>
    {
        [MessagePackMember(3)]
        public string ErrorMessage { get; set; }
    }
}
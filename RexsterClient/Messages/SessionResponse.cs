namespace Rexster.Messages
{
    using System.Collections.Generic;
    using MsgPack.Serialization;

    public class SessionResponse : RexProMessage<Dictionary<string, object>> 
    {
        [MessagePackMember(3)]
        public string[] Languages { get; set; }
    }
}
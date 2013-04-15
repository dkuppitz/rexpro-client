namespace Rexster
{
    using System.Collections.Generic;

    using MsgPack.Serialization;

    public class ScriptResponse<T> : ScriptResponse
    {
        [MessagePackMember(3)]
        public T Result { get; set; }
    }

    public abstract class ScriptResponse : RexProMessage
    {
        [MessagePackMember(4)]
        public Dictionary<string, object> Bindings { get; set; }
    }
}
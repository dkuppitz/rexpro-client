namespace Rexster
{
    using System.Collections.Generic;

    using MsgPack.Serialization;

    public class ScriptResponse<T> : RexProMessage
    {
        [MessagePackMember(3)]
        public T Result { get; set; }

        [MessagePackMember(4)]
        public Dictionary<string, object> Bindings { get; set; }
    }
}
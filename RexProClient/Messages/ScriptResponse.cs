namespace Rexster.Messages
{
    using System.Collections.Generic;

    using MsgPack;
    using MsgPack.Serialization;

    public class DynamicScriptResponse : ScriptResponse<object>, IUnpackable
    {
        public void UnpackFromMessage(Unpacker unpacker)
        {
            if (unpacker.Read()) this.Session = unpacker.Unpack<byte[]>();
            if (unpacker.Read()) this.Request = unpacker.Unpack<byte[]>();
            if (unpacker.Read()) this.Meta = unpacker.Unpack<Dictionary<string, object>>();
            if (unpacker.Read())
            {
                this.Result = unpacker.UnpackDynamicObject();
            }
            if (unpacker.Read()) this.Bindings = unpacker.Unpack<Dictionary<string, object>>();
        }
    }

    public class ScriptResponse<T> : ScriptResponse
    {
        [MessagePackMember(3)]
        public T Result { get; set; }

        public static implicit operator T(ScriptResponse<T> response)
        {
            return response.Result;
        }
    }

    public abstract class ScriptResponse : RexProMessage<Dictionary<string, object>> 
    {
        [MessagePackMember(4)]
        public Dictionary<string, object> Bindings { get; set; }
    }
}
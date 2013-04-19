namespace Rexster.Messages
{
    using System.Collections.Generic;
    using MsgPack;

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
}
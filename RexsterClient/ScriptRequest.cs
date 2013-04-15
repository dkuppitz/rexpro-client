namespace Rexster
{
    using System;
    using System.Collections.Generic;

    using MsgPack.Serialization;

    public class ScriptRequest : RexProMessage
    {
        private static readonly byte[] EmptyByteArray = Guid.Empty.ToByteArray();

        private Dictionary<string, object> bindings;

        public ScriptRequest()
        {
            this.Session = EmptyByteArray;
            this.Request = Guid.NewGuid().ToByteArray();
            this.Meta = new MetaData();
            this.Language = "groovy";
        }

        public ScriptRequest(string script) : this(script, null)
        {
        }

        public ScriptRequest(string script, Dictionary<string, object> bindings) : this()
        {
            this.Script = script;
            this.bindings = bindings;
        }

        [MessagePackMember(3)]
        public string Language { get; set; }

        [MessagePackMember(4)]
        public string Script { get; set; }

        [MessagePackMember(5)]
        public Dictionary<string, object> Bindings
        {
            get { return this.bindings; }
            set { this.bindings = value; }
        }

        public void AddBinding(string name, object value)
        {
            if (this.bindings == null)
            {
                this.bindings = new Dictionary<string, object>
                {
                    { name, value }
                };
            }
            else
            {
                this.bindings.Add(name, value);
            }
        }
    }
}
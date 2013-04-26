namespace Rexster.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using MsgPack.Serialization;

    public class ScriptRequest : RexProMessage<ScriptRequestMetaData>
    {
        private Dictionary<string, object> bindings;

        public ScriptRequest()
        {
            this.Request = Guid.NewGuid().ToByteArray();
            this.Meta = new ScriptRequestMetaData();
            this.Language = "groovy";
        }

        public ScriptRequest(string script) : this(script, null)
        {
        }

        public ScriptRequest(string script, Dictionary<string, object> bindings) : this()
        {
            this.Script = script;
            if (bindings != null)
                bindings.ToList().ForEach(binding => this.AddBinding(binding.Key, binding.Value));
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

        public ScriptRequest AddBinding(string name, object value)
        {
            if (this.bindings == null)
            {
                this.bindings = new Dictionary<string, object>
                {
                    { name, GetBindingValue(value) }
                };
            }
            else
            {
                this.bindings.Add(name, GetBindingValue(value));
            }

            return this;
        }

        private static object GetBindingValue(object value)
        {
            return (value is GraphItem) ? ((GraphItem) value).Id : value;
        }
    }
}
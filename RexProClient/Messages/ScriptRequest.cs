namespace Rexster.Messages
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;

    public class ScriptRequest : RexProMessage<ScriptRequestMetaData>
    {
        private object bindings;

        public ScriptRequest()
        {
            this.Request = Guid.NewGuid();
            this.Meta = new ScriptRequestMetaData();
            this.Language = "groovy";
        }

        public ScriptRequest(string script) : this(script, null)
        {
        }

        public ScriptRequest(string script, object bindings) : this()
        {
            this.Script = script;
            this.AddBindings(bindings);
        }

        public string Language { get; set; }
        public string Script { get; set; }

        public object Bindings
        {
            get { return this.bindings; }
            set { this.bindings = value; }
        }

        private void AddBindings(object bindings)
        {
            if (bindings == null)
                return;

            var dict = bindings as IDictionary<string, object>;
            if (dict != null)
            {
                foreach (var entry in dict)
                {
                    this.AddBinding(entry.Key, entry.Value);
                }
                return;
            }

            if (!bindings.GetType().IsPrimitive)
            {
                var jObject = JObject.FromObject(bindings);
                foreach (var property in jObject.Properties())
                {
                    this.AddBinding(property.Name, property.Value);
                }
            }
        }

        public ScriptRequest AddBinding(string name, object value)
        {
            var bindingValue = GetBindingValue(value);

            if (this.bindings == null)
            {
                this.bindings = new Dictionary<string, object>
                {
                    { name, bindingValue }
                };
            }
            else
            {
                var dict = this.bindings as IDictionary<string, object>;
                if (dict != null)
                {
                    dict.Add(name, bindingValue);
                }
                else
                {
                    var jObject = this.bindings as JObject;
                    if (jObject != null)
                    {
                        jObject.Add(name, JToken.FromObject(bindingValue));
                    }
                    else
                    {
                        throw new RexProClientException(string.Format("Cannot add binding to type '{0}'",
                                                                      this.bindings.GetType().Name));
                    }
                }
            }

            return this;
        }

        private static object GetBindingValue(object value)
        {
            var item = value as GraphItem;
            if (item != null)
            {
                return item.Id;
            }

            var jToken = value as JToken;
            if (jToken != null && jToken.HasValues && jToken["_type"] != null && jToken["_id"] != null)
            {
                switch (jToken["_type"].ToObject<string>())
                {
                    case "vertex":
                    case "edge":
                        return jToken["_id"].ToObject<string>();
                }
            }

            return value;
        }

        public override object[] ToSerializableArray()
        {
            var result = base.ToSerializableArray();
            var size = result.Length;
            Array.Resize(ref result, size + 3);
            result[size++] = this.Language;
            result[size++] = this.Script;
            result[size] = this.bindings ?? new object();
            return result;
        }
    }
}
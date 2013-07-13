namespace Rexster.Messages
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;

    public class ScriptResponse<T> : ScriptResponse
    {
        public T Result { get; set; }

        public static implicit operator T(ScriptResponse<T> response)
        {
            return response.Result;
        }

        public override void LoadJson(string json)
        {
            var arr = JArray.Parse(json);

            this.Session = arr[0].ToObject<Guid>();
            this.Request = arr[1].ToObject<Guid>();
            this.Meta = arr[2].ToObject<IDictionary<string, object>>();
            this.Result = arr[3].ToObject<T>();
            this.Bindings = arr[4].ToObject<IDictionary<string, object>>();
        }
    }

    public abstract class ScriptResponse : RexProMessage<IDictionary<string, object>>
    {
        public IDictionary<string, object> Bindings { get; set; }
    }
}
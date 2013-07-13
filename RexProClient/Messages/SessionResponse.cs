namespace Rexster.Messages
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;

    public class SessionResponse : RexProMessage<IDictionary<string, object>> 
    {
        public string[] Languages { get; set; }

        public override void LoadJson(string json)
        {
            var arr = JArray.Parse(json);
            this.Session = arr[0].ToObject<Guid>();
            this.Request = arr[1].ToObject<Guid>();
            this.Meta = null;
            this.Languages = arr[3].ToObject<string[]>();
        }
    }
}
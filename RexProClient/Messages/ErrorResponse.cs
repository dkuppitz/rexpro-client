namespace Rexster.Messages
{
    using System;
    using Newtonsoft.Json.Linq;

    public class ErrorResponse : RexProMessage<ErrorResponseMetaData>
    {
        public string ErrorMessage { get; set; }

        public override void LoadJson(string json)
        {
            var arr = JArray.Parse(json);
            this.Session = arr[0].ToObject<Guid>();
            this.Request = arr[1].ToObject<Guid>();
            this.Meta = null;
            this.ErrorMessage = arr[3].ToObject<string>();
        }
    }
}
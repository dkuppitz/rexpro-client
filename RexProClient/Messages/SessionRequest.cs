namespace Rexster.Messages
{
    using System;

    public class SessionRequest :  RexProMessage<SessionRequestMetaData>
    {
        public SessionRequest() : this(null)
        {
        }

        public SessionRequest(GraphSettings settings, RexProSession session = null, bool killSession = false)
        {
            this.Session = session;
            this.Meta = new SessionRequestMetaData(settings, killSession);
        }

        public string Username { get; set; }
        public string Password { get; set; }

        public override object[] ToSerializableArray()
        {
            var result = base.ToSerializableArray();
            var size = result.Length;
            Array.Resize(ref result, size + 2);
            result[size++] = Username;
            result[size] = Password;
            return result;
        }
    }
}

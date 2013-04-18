namespace Rexster.Messages
{
    using System;
    using MsgPack.Serialization;

    public class SessionRequest :  RexProMessage<SessionRequestMetaData>
    {
        private readonly int channel;

        public SessionRequest() : this(null)
        {
        }

        public SessionRequest(GraphSettings settings, RexProSession session = null, bool killSession = false)
        {
            this.channel = Messages.Channel.MsgPack;
            this.Session = session;
            this.Meta = new SessionRequestMetaData(settings, killSession);
        }

        [MessagePackMember(3)]
        public int Channel
        {
            get { return this.channel; }
            set
            {
                if (Messages.Channel.MsgPack != value)
                {
                    throw new NotSupportedException();
                }
            }
        }

        [MessagePackMember(4)]
        public string Username { get; set; }

        [MessagePackMember(5)]
        public string Password { get; set; }
    }
}

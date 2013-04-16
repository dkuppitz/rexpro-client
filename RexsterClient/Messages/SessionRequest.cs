namespace Rexster.Messages
{
    using System;
    using MsgPack.Serialization;

    public class SessionRequest :  RexProMessage<SessionRequestMetaData>
    {
        private readonly int channel;

        public SessionRequest()
        {
            this.channel = Messages.Channel.MsgPack;
            this.Meta = new SessionRequestMetaData();
        }

        public SessionRequest(byte[] session, bool kill = false) : this()
        {
            this.Session = session;
            this.Meta.KillSession = kill;
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

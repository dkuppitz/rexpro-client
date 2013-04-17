namespace Rexster
{
    using System;

    public class RexProSession : IDisposable
    {
        private readonly RexProClient client;
        private readonly byte[] session;

        internal RexProSession(RexProClient client, byte[] session)
        {
            this.client = client;
            this.session = session;
        }

        public void Dispose()
        {
            this.client.KillSession(this);
        }

        public static implicit operator byte[](RexProSession session)
        {
            return session.session;
        }
    }
}
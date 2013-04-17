namespace Rexster
{
    using System;

    public class RexProSession : IDisposable
    {
        private readonly RexProClient client;
        private readonly byte[] session;

        internal event EventHandler Kill;

        internal RexProSession(RexProClient client, byte[] session)
        {
            this.client = client;
            this.session = session;
        }

        public void Dispose()
        {
            this.client.KillSession(this);

            if (this.Kill != null)
                this.Kill(this, EventArgs.Empty);
        }

        public static implicit operator byte[](RexProSession session)
        {
            return session != null ? session.session : null;
        }
    }
}
namespace Rexster
{
    using System;

    public sealed class RexProSession : IDisposable
    {
        private readonly RexProClient client;
        private readonly Guid session;

        internal RexProSession(RexProClient client, Guid session)
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

        internal event EventHandler Kill;

        public static implicit operator Guid(RexProSession session)
        {
            return session != null ? session.session : Guid.Empty;
        }
    }
}
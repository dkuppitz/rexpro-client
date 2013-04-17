namespace Rexster
{
    using System;

    using Rexster.Messages;

    public class RexsterClientException : Exception
    {
        public RexsterClientException(ErrorResponse msg)
            : base(msg.ErrorMessage)
        {
            this.Session = msg.Session;
            this.Request = msg.Request;
        }

        public byte[] Session { get; private set; }
        public byte[] Request { get; private set; }
    }
}
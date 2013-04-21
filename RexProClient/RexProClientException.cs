namespace Rexster
{
    using System;
    using System.Runtime.Serialization;

    using Rexster.Messages;

    [Serializable]
    public class RexProClientException : Exception
    {
        public RexProClientException()
        {
        }

        public RexProClientException(string message) : base(message)
        {
        }

        public RexProClientException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected RexProClientException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.Session = (byte[]) info.GetValue("Session", typeof (byte[]));
            this.Request = (byte[]) info.GetValue("Request", typeof (byte[]));
        }

        public RexProClientException(ErrorResponse response)
            : this(response.ErrorMessage)
        {
            this.Session = response.Session;
            this.Request = response.Request;
        }

        public byte[] Session { get; private set; }
        public byte[] Request { get; private set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Session", this.Session, typeof (byte[]));
            info.AddValue("Request", this.Request, typeof (byte[]));
        }
    }
}
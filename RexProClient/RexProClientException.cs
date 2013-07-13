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
            this.Session = (Guid) info.GetValue("Session", typeof (Guid));
            this.Request = (Guid) info.GetValue("Request", typeof (Guid));
        }

        public RexProClientException(ErrorResponse response)
            : this(response.ErrorMessage)
        {
            this.Session = response.Session;
            this.Request = response.Request;
        }

        public Guid Session { get; private set; }
        public Guid Request { get; private set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Session", this.Session, typeof (byte[]));
            info.AddValue("Request", this.Request, typeof (byte[]));
        }
    }
}
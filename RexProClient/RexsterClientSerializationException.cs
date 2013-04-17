namespace Rexster
{
    using System;
    using System.Runtime.Serialization;

    public class RexsterClientSerializationException : Exception
    {
        public RexsterClientSerializationException()
        {
        }

        public RexsterClientSerializationException(string message) : base(message)
        {
        }

        public RexsterClientSerializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected RexsterClientSerializationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
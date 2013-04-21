namespace Rexster
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class RexProClientSerializationException : Exception
    {
        public RexProClientSerializationException()
        {
        }

        public RexProClientSerializationException(string message) : base(message)
        {
        }

        public RexProClientSerializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected RexProClientSerializationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
namespace Rexster.Messages
{
    using System;

    public abstract class RexProMessage
    {
        public Guid Session { get; set; }
        public Guid Request { get; set; }

        public virtual object[] ToSerializableArray()
        {
            return new object[]
            {
                this.Session,
                this.Request
            };
        }

        public virtual void LoadJson(string json)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class RexProMessage<TMetaData> : RexProMessage
    {
        public TMetaData Meta { get; set; }

        public override object[] ToSerializableArray()
        {
            var result = base.ToSerializableArray();
            var size = result.Length;
            Array.Resize(ref result, size + 1);
            if (this.Meta is IRequestMetaData)
            {
                result[size] = ((IRequestMetaData) this.Meta).ToSerializableObject();
            }
            else
            {
                result[size] = this.Meta;
            }
            return result;
        }
    }
}
using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace PsdzClient.Psdz
{
    [Serializable]
    public class PsdzRuntimeException : Exception
    {
        public PsdzRuntimeException()
        {
        }

        public PsdzRuntimeException(int messageId, string message) : base(message)
        {
            this.MessageId = messageId;
        }

        public PsdzRuntimeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PsdzRuntimeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.MessageId = info.GetInt32("MessageId");
        }

        public int MessageId { get; private set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("MessageId", this.MessageId);
            base.GetObjectData(info, context);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} (MessageId: {1})", base.ToString(), this.MessageId);
        }
    }
}

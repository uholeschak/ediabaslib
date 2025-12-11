using System;
using System.Globalization;
using System.Runtime.Serialization;

#pragma warning disable CS0672, SYSLIB0051
namespace BMW.Rheingold.Psdz.Model.Exceptions
{
    [Serializable]
    public class PsdzRuntimeException : Exception
    {
        public int MessageId { get; private set; }

        public PsdzRuntimeException()
        {
        }

        public PsdzRuntimeException(int messageId, string message) : base(message)
        {
            MessageId = messageId;
        }

        public PsdzRuntimeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PsdzRuntimeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            MessageId = info.GetInt32("MessageId");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            info.AddValue("MessageId", MessageId);
            base.GetObjectData(info, context);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} (MessageId: {1})", base.ToString(), MessageId);
        }
    }
}
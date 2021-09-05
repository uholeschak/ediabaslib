using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace PsdzClient
{
    [Serializable]
    public class PsdzRuntimeException : Exception
    {
        // Token: 0x060004CB RID: 1227 RVA: 0x00003703 File Offset: 0x00001903
        public PsdzRuntimeException()
        {
        }

        // Token: 0x060004CC RID: 1228 RVA: 0x0000370B File Offset: 0x0000190B
        public PsdzRuntimeException(int messageId, string message) : base(message)
        {
            this.MessageId = messageId;
        }

        // Token: 0x060004CD RID: 1229 RVA: 0x0000371B File Offset: 0x0000191B
        public PsdzRuntimeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        // Token: 0x060004CE RID: 1230 RVA: 0x00003725 File Offset: 0x00001925
        protected PsdzRuntimeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.MessageId = info.GetInt32("MessageId");
        }

        // Token: 0x17000256 RID: 598
        // (get) Token: 0x060004CF RID: 1231 RVA: 0x00003740 File Offset: 0x00001940
        // (set) Token: 0x060004D0 RID: 1232 RVA: 0x00003748 File Offset: 0x00001948
        public int MessageId { get; private set; }

        // Token: 0x060004D1 RID: 1233 RVA: 0x00003751 File Offset: 0x00001951
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("MessageId", this.MessageId);
            base.GetObjectData(info, context);
        }

        // Token: 0x060004D2 RID: 1234 RVA: 0x0000377A File Offset: 0x0000197A
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} (MessageId: {1})", base.ToString(), this.MessageId);
        }
    }
}

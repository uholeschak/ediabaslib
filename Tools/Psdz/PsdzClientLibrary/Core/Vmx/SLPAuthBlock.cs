using BMW.Rheingold.CoreFramework;
using System;

namespace BMW.Rheingold.xVM
{
    public class SLPAuthBlock
    {
        public string authstruct;

        public ushort bsd;

        public ushort length;

        public string opaque;

        public ushort opaquelen;

        public SLPString spistr;

        public uint timestamp;

        public SLPAuthBlock()
        {
            spistr = new SLPString();
            authstruct = string.Empty;
            opaque = string.Empty;
        }

        public ushort GetLength()
        {
            return length;
        }

        public bool ParsePayload(byte[] payload, ushort offsetstart, out ushort offsetend)
        {
            ushort num = offsetstart;
            try
            {
                bsd = SLP.AsUINT16(payload, num);
                num += 2;
                length = SLP.AsUINT16(payload, num);
                num += 2;
                timestamp = SLP.AsUINT32(payload, num);
                num += 4;
                spistr.ParsePayload(payload, num, out num);
                ushort num2 = (ushort)(length - 10 - spistr.Length);
                SLP.AsSTRING(payload, num, num2);
                num += num2;
            }
            catch (Exception exception)
            {
                Log.WarningException("SLPUrlEntry.ParsePayload()", exception);
                offsetend = 0;
                return false;
            }
            offsetend = num;
            return true;
        }
    }
}

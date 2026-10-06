using BMW.Rheingold.CoreFramework;
using System;
using System.Text;
using BMW.Rheingold.xVM;

namespace BMW.Rheingold.xVM
{
    public class SLPUrlEntry
    {
        public SLPAuthBlock[] autharray;

        public byte authcount;

        public ushort lifetime;

        public SLPString opaque;

        public byte reserved;

        public SLPString url;

        public SLPUrlEntry()
        {
            url = new SLPString();
            authcount = 0;
            autharray = new SLPAuthBlock[0];
        }

        public ushort GetLength()
        {
            try
            {
                ushort num = 4;
                num += url.Length;
                for (int i = 0; i < authcount; i++)
                {
                    num += autharray[i].GetLength();
                }
                return num;
            }
            catch (Exception exception)
            {
                Log.WarningException("SLPUrlEntry.GetLength()", exception);
                return 0;
            }
        }

        public bool ParsePayload(byte[] payload, ushort offsetstart, out ushort offsetend)
        {
            ushort num = offsetstart;
            try
            {
                reserved = payload[num++];
                lifetime = SLP.AsUINT16(payload, num);
                num += 2;
                url.ParsePayload(payload, 1, out num);
                authcount = payload[num++];
                if (authcount > 0)
                {
                    autharray = new SLPAuthBlock[authcount];
                }
                for (int i = 0; i < authcount; i++)
                {
                    autharray[i] = new SLPAuthBlock();
                    autharray[i].ParsePayload(payload, num, out num);
                }
                if (payload.Length > num)
                {
                    opaque.ParsePayload(payload, num, out num);
                }
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

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("reserved: {0}\n", reserved);
            stringBuilder.AppendFormat("lifetime: {0}\n", lifetime);
            if (url != null)
            {
                stringBuilder.AppendFormat("urllen: {0}\n", url.Length);
            }
            else
            {
                stringBuilder.AppendFormat("urllen: null\n");
            }
            stringBuilder.AppendFormat("url: {0}\n", url.Str);
            stringBuilder.AppendFormat("authcount: {0}\n", authcount);
            return stringBuilder.ToString();
        }
    }
}

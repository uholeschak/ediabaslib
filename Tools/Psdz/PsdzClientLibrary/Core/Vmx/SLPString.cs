using BMW.Rheingold.CoreFramework;
using System;
using PsdzClient.Core;

namespace BMW.Rheingold.xVM
{
    public class SLPString
    {
        private ushort length;

        private string str;

        public ushort Length
        {
            get
            {
                return length;
            }
            set
            {
                length = value;
            }
        }

        public string Str
        {
            get
            {
                return str;
            }
            set
            {
                str = value;
            }
        }

        public SLPString()
        {
            length = 0;
            str = string.Empty;
        }

        public static implicit operator SLPString(string aStr)
        {
            return new SLPString
            {
                str = aStr,
                length = (ushort)aStr.Length
            };
        }

        public static implicit operator string(SLPString sStr)
        {
            return sStr.str;
        }

        public static implicit operator ushort(SLPString sStr)
        {
            return sStr.length;
        }

        public bool ParsePayload(byte[] payload, ushort offsetstart, out ushort offsetend)
        {
            try
            {
                length = SLP.AsUINT16(payload, offsetstart);
                if (length > 0)
                {
                    str = SLP.AsSTRING(payload, (ushort)(offsetstart + 2), length);
                }
                offsetend = (ushort)(offsetstart + length + 2);
            }
            catch (Exception exception)
            {
                Log.WarningException("SLPString.ParsePayload()", exception);
                offsetend = 0;
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            return str;
        }
    }
}

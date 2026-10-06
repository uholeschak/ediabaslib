using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.xVM;
using System;
using System.Text;

namespace BMW.Rheingold.xVM
{
    public class SLPAttrRply : SLPHeader
    {
        public const byte SLPTypeId = 7;

        public SLPString attrlist;

        public byte[] autharray = new byte[0];

        public byte authcount;

        public ushort errorcode;

        public SLPAttrRply()
        {
            attrlist = new SLPString();
        }

        public bool ParsePayload()
        {
            ushort num = 0;
            byte[] array = payload;
            try
            {
                errorcode = SLP.AsUINT16(array, num);
                num += 2;
                attrlist.ParsePayload(array, num, out num);
                authcount = array[num++];
            }
            catch (Exception exception)
            {
                Log.WarningException("SLPAttrRply.ParsePayload()", exception);
                return false;
            }
            return true;
        }

        public bool SetupSendBuffer()
        {
            uint num = 0u;
            length = (ushort)(attrlist.Length + 14 + 4 + langtaglen + 1);
            sendbuffer = new byte[length];
            sendbuffer[0] = version;
            sendbuffer[1] = 7;
            switch (version)
            {
                case 1:
                    SLP.ToUINT16(sendbuffer, (ushort)length, 2);
                    num = 6u;
                    break;
                case 2:
                    {
                        SLP.ToUINT24(sendbuffer, length, 2);
                        sendbuffer[5] = flags;
                        SLP.ToUINT24(sendbuffer, extoffset, 7);
                        SLP.ToUINT16(sendbuffer, xid, 10);
                        string text;
                        if (string.IsNullOrEmpty(langtag))
                        {
                            langtaglen = 0;
                        }
                        else
                        {
                            langtaglen = (ushort)langtag.Length;
                            num = 14u;
                            text = langtag;
                            for (int i = 0; i < text.Length; i++)
                            {
                                byte b = (byte)text[i];
                                sendbuffer[num++] = b;
                            }
                        }
                        SLP.ToUINT16(sendbuffer, langtaglen, 12);
                        SLP.ToUINT16(sendbuffer, errorcode, (ushort)num);
                        num += 2;
                        SLP.ToUINT16(sendbuffer, attrlist.Length, (ushort)num);
                        num += 2;
                        text = attrlist.Str;
                        for (int i = 0; i < text.Length; i++)
                        {
                            byte b2 = (byte)text[i];
                            sendbuffer[num++] = b2;
                        }
                        authcount = 0;
                        sendbuffer[num++] = authcount;
                        break;
                    }
                default:
                    return false;
            }
            return true;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("SLPAttrRply\n");
            stringBuilder.AppendFormat("errorcode: {0}\n", errorcode);
            stringBuilder.AppendFormat("attrlistlen: {0}\n", attrlist.Length);
            if (attrlist.Length > 0)
            {
                stringBuilder.AppendFormat("attrlist: {0}\n", attrlist.Str);
            }
            stringBuilder.AppendFormat("authcount: {0}\n", authcount);
            return base.ToString() + stringBuilder;
        }
    }

}

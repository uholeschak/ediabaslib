using BMW.Rheingold.CoreFramework;
using System;
using System.Text;

namespace BMW.Rheingold.xVM
{
    public class SLPSrvTypeRqst : SLPHeader
    {
        public const byte SLPTypeId = 9;

        public SLPString namingauth;

        public SLPString prlist;

        public SLPString scopelist;

        public SLPSrvTypeRqst()
        {
            prlist = string.Empty;
            namingauth = string.Empty;
            scopelist = string.Empty;
        }

        public bool ParsePayload()
        {
            ushort offsetend = 0;
            byte[] array = payload;
            try
            {
                prlist.ParsePayload(array, offsetend, out offsetend);
                namingauth.ParsePayload(array, offsetend, out offsetend);
                scopelist.ParsePayload(array, offsetend, out offsetend);
                return true;
            }
            catch (Exception exception)
            {
                Log.WarningException("SLPSrvTypeRqst.ParsePayload()", exception);
            }
            return false;
        }

        public bool SetupSendBuffer()
        {
            uint num = 0u;
            length = (ushort)(prlist.Length + scopelist.Length + namingauth.Length + 7 + 14 + langtaglen + 1);
            sendbuffer = new byte[length];
            sendbuffer[0] = version;
            sendbuffer[1] = 9;
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
                        SLP.ToUINT16(sendbuffer, prlist.Length, (ushort)num);
                        num += 2;
                        text = prlist.Str;
                        for (int i = 0; i < text.Length; i++)
                        {
                            byte b2 = (byte)text[i];
                            sendbuffer[num++] = b2;
                        }
                        SLP.ToUINT16(sendbuffer, namingauth.Length, (ushort)num);
                        num += 2;
                        text = namingauth.Str;
                        for (int i = 0; i < text.Length; i++)
                        {
                            byte b3 = (byte)text[i];
                            sendbuffer[num++] = b3;
                        }
                        SLP.ToUINT16(sendbuffer, scopelist.Length, (ushort)num);
                        num += 2;
                        text = scopelist.Str;
                        for (int i = 0; i < text.Length; i++)
                        {
                            byte b4 = (byte)text[i];
                            sendbuffer[num++] = b4;
                        }
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
            stringBuilder.AppendFormat("SLPSrvTypeRqst\n");
            stringBuilder.AppendFormat("prlistlen: {0}\n", prlist);
            if ((ushort)prlist > 0)
            {
                stringBuilder.AppendFormat("prlist: {0}\n", prlist);
            }
            stringBuilder.AppendFormat("namingauthlen: {0}\n", namingauth);
            if ((ushort)namingauth > 0)
            {
                stringBuilder.AppendFormat("namingauth: {0}\n", namingauth);
            }
            stringBuilder.AppendFormat("scopelistlen: {0}\n", scopelist);
            if ((ushort)scopelist > 0)
            {
                stringBuilder.AppendFormat("scopelist: {0}\n", scopelist);
            }
            return base.ToString() + stringBuilder;
        }
    }
}

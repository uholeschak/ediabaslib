using BMW.Rheingold.CoreFramework;
using System;
using System.Data.SqlTypes;
using System.Text;

namespace BMW.Rheingold.xVM
{
    public class SLPAttrRqst : SLPHeader
    {
        public const byte SLPTypeId = 6;

        public SLPString prlist;

        public SLPString scopelist;

        public SLPString spistr;

        public SLPString taglist;

        public SLPString url;

        public SLPAttrRqst()
        {
            prlist = new SLPString();
            url = new SLPString();
            scopelist = new SLPString();
            taglist = new SLPString();
            spistr = new SLPString();
        }

        public bool ParsePayload()
        {
            ushort offsetend = 0;
            byte[] array = payload;
            try
            {
                prlist.ParsePayload(array, offsetend, out offsetend);
                url.ParsePayload(array, offsetend, out offsetend);
                scopelist.ParsePayload(array, offsetend, out offsetend);
            }
            catch (Exception exception)
            {
                Log.WarningException("SLPAttrRqst.ParsePayload()", exception);
                return false;
            }
            return true;
        }

        public bool SetupSendBuffer()
        {
            uint num = 0u;
            length = (ushort)(prlist.Length + url.Length + scopelist.Length + taglist.Length + spistr.Length + 11 + 14 + langtaglen + 1);
            sendbuffer = new byte[length];
            sendbuffer[0] = version;
            sendbuffer[1] = 6;
            switch (version)
            {
                case 1:
                    {
                        SLP.ToUINT16(sendbuffer, (ushort)length, 2);
                        num = 6u;
                        string text = scopelist.Str;
                        for (int i = 0; i < text.Length; i++)
                        {
                            _ = text[i];
                        }
                        break;
                    }
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
                        SLP.ToUINT16(sendbuffer, url.Length, (ushort)num);
                        num += 2;
                        text = url.Str;
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
                        SLP.ToUINT16(sendbuffer, taglist.Length, (ushort)num);
                        num += 2;
                        text = taglist.Str;
                        for (int i = 0; i < text.Length; i++)
                        {
                            byte b5 = (byte)text[i];
                            sendbuffer[num++] = b5;
                        }
                        SLP.ToUINT16(sendbuffer, spistr.Length, (ushort)num);
                        num += 2;
                        text = spistr.Str;
                        for (int i = 0; i < text.Length; i++)
                        {
                            byte b6 = (byte)text[i];
                            sendbuffer[num++] = b6;
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
            stringBuilder.AppendFormat("SLPAttrRqst\n");
            stringBuilder.AppendFormat("prlistlen: {0}\n", prlist.Length);
            if (prlist.Length > 0)
            {
                stringBuilder.AppendFormat("prlist: {0}\n", prlist.Str);
            }
            stringBuilder.AppendFormat("urllen: {0}\n", url.Length);
            if (url.Length > 0)
            {
                stringBuilder.AppendFormat("url: {0}\n", url.Str);
            }
            stringBuilder.AppendFormat("scopelistlen: {0}\n", scopelist.Length);
            if (scopelist.Length > 0)
            {
                stringBuilder.AppendFormat("scopelist: {0}\n", scopelist.Str);
            }
            stringBuilder.AppendFormat("taglistlen: {0}\n", taglist.Length);
            if (taglist.Length > 0)
            {
                stringBuilder.AppendFormat("taglist: {0}\n", taglist.Str);
            }
            stringBuilder.AppendFormat("spistrlen: {0}\n", spistr.Length);
            if (spistr.Length > 0)
            {
                stringBuilder.AppendFormat("spistr: {0}\n", spistr.Str);
            }
            return base.ToString() + stringBuilder;
        }
    }
}

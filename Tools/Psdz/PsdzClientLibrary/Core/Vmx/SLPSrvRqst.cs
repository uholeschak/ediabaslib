using BMW.Rheingold.CoreFramework;
using System;
using System.Text;

namespace BMW.Rheingold.xVM
{
    public class SLPSrvRqst : SLPHeader
    {
        public const byte SLPTypeId = 1;

        private SLPString predicate;

        private SLPString prlist;

        private SLPString scopelist;

        private SLPString spistr;

        private SLPString srvtype;

        public SLPString Predicate
        {
            get
            {
                return predicate;
            }
            set
            {
                predicate = value;
            }
        }

        public SLPString PrList
        {
            get
            {
                return prlist;
            }
            set
            {
                prlist = value;
            }
        }

        public SLPString ScopeList
        {
            get
            {
                return scopelist;
            }
            set
            {
                scopelist = value;
            }
        }

        public SLPString SrvType
        {
            get
            {
                return srvtype;
            }
            set
            {
                srvtype = value;
            }
        }

        public SLPString SpiStr
        {
            get
            {
                return spistr;
            }
            set
            {
                spistr = value;
            }
        }

        public SLPSrvRqst()
        {
            prlist = new SLPString();
            srvtype = new SLPString();
            scopelist = new SLPString();
            predicate = new SLPString();
            spistr = new SLPString();
        }

        public bool ParsePayload()
        {
            ushort offsetend = 0;
            byte[] array = payload;
            try
            {
                prlist.ParsePayload(array, offsetend, out offsetend);
                srvtype.ParsePayload(array, offsetend, out offsetend);
                scopelist.ParsePayload(array, offsetend, out offsetend);
                predicate.ParsePayload(array, offsetend, out offsetend);
                spistr.ParsePayload(array, offsetend, out offsetend);
            }
            catch (Exception exception)
            {
                Log.WarningException("SrvRqst.ParsePayload()", exception);
                return false;
            }
            return true;
        }

        public bool SetupSendBuffer()
        {
            uint num = 0u;
            length = (ushort)(prlist.Length + srvtype.Length + scopelist.Length + predicate.Length + spistr.Length + 11 + 14 + langtaglen + 1);
            sendbuffer = new byte[length];
            sendbuffer[0] = version;
            sendbuffer[1] = 1;
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
                        SLP.ToUINT16(sendbuffer, srvtype.Length, (ushort)num);
                        num += 2;
                        text = srvtype.Str;
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
                        SLP.ToUINT16(sendbuffer, predicate.Length, (ushort)num);
                        num += 2;
                        text = predicate.Str;
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
            stringBuilder.AppendFormat("SrvRqst\n");
            stringBuilder.AppendFormat("prlistlen: {0}\n", prlist);
            if ((ushort)prlist > 0)
            {
                stringBuilder.AppendFormat("prlist: {0}\n", prlist);
            }
            stringBuilder.AppendFormat("srvtypelen: {0}\n", srvtype);
            if ((ushort)srvtype > 0)
            {
                stringBuilder.AppendFormat("srvtype: {0}\n", srvtype);
            }
            stringBuilder.AppendFormat("scopelistlen: {0}\n", scopelist);
            if ((ushort)scopelist > 0)
            {
                stringBuilder.AppendFormat("scopelist: {0}\n", scopelist);
            }
            stringBuilder.AppendFormat("predicatelen: {0}\n", predicate);
            if ((ushort)predicate > 0)
            {
                stringBuilder.AppendFormat("predicate: {0}\n", predicate);
            }
            stringBuilder.AppendFormat("spistrlen: {0}\n", spistr);
            if ((ushort)spistr > 0)
            {
                stringBuilder.AppendFormat("spistr: {0}\n", spistr);
            }
            return base.ToString() + stringBuilder;
        }
    }
}

using BMW.Rheingold.CoreFramework;
using System;
using System.Text;

namespace BMW.Rheingold.xVM
{
    public class SLPSAAdvert : SLPHeader
    {
        public const byte SLPTypeId = 11;

        public SLPString attrlist;

        public SLPAuthBlock[] autharray;

        public ushort authcount;

        public SLPString scopelist;

        public SLPString url;

        public SLPSAAdvert()
        {
            url = new SLPString();
            scopelist = new SLPString();
            attrlist = new SLPString();
            authcount = 0;
            autharray = new SLPAuthBlock[0];
        }

        public bool ParsePayload()
        {
            ushort offsetend = 0;
            byte[] array = payload;
            try
            {
                url.ParsePayload(array, offsetend, out offsetend);
                scopelist.ParsePayload(array, offsetend, out offsetend);
                attrlist.ParsePayload(array, offsetend, out offsetend);
                authcount = array[offsetend++];
                autharray = new SLPAuthBlock[authcount];
                for (int i = 0; i < authcount; i++)
                {
                    autharray[i] = new SLPAuthBlock();
                    autharray[i].ParsePayload(array, offsetend, out offsetend);
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("SLPSAAdvert.ParsePayload()", exception);
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("SLPSAAdvert\n");
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
            stringBuilder.AppendFormat("attrlistlen: {0}\n", attrlist.Length);
            if (attrlist.Length > 0)
            {
                stringBuilder.AppendFormat("attrlist: {0}\n", attrlist.Str);
            }
            for (int i = 0; i < authcount; i++)
            {
                stringBuilder.AppendFormat("SLPAuthBlock[{0}]:\n", i);
                stringBuilder.AppendFormat(autharray[i].ToString());
            }
            return base.ToString() + stringBuilder;
        }
    }
}

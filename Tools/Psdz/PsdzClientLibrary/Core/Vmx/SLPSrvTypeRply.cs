using BMW.Rheingold.CoreFramework;
using System;
using System.Text;
using BMW.Rheingold.xVM;

namespace BMW.Rheingold.xVM
{
    public class SLPSrvTypeRply : SLPHeader
    {
        public const byte SLPTypeId = 10;

        private ushort errorcode;

        private SLPString srvtypelist;

        public ushort ErrorCode
        {
            get
            {
                return errorcode;
            }
            set
            {
                errorcode = value;
            }
        }

        public SLPString SrvTypeList
        {
            get
            {
                return srvtypelist;
            }
            set
            {
                srvtypelist = value;
            }
        }

        public SLPSrvTypeRply()
        {
            srvtypelist = new SLPString();
        }

        public bool ParsePayload()
        {
            byte[] ucp = payload;
            try
            {
                errorcode = SLP.AsUINT16(ucp, 0);
                srvtypelist.ParsePayload(ucp, 2, out var _);
            }
            catch (Exception exception)
            {
                Log.WarningException("SLPSrvTypeRply.ParsePayload()", exception);
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("SLPSrvTypeRply\n");
            stringBuilder.AppendFormat("errorcode: {0}\n", errorcode);
            stringBuilder.AppendFormat("srvtypelistlen: {0}\n", srvtypelist);
            if ((ushort)srvtypelist > 0)
            {
                stringBuilder.AppendFormat("srvtypelist: {0}\n", srvtypelist);
            }
            return base.ToString() + stringBuilder;
        }
    }

}

using BMW.Rheingold.CoreFramework;
using System;
using BMW.Rheingold.xVM;

namespace BMW.Rheingold.xVM
{
    public class SLPSrvRply : SLPHeader
    {
        public const byte SLPTypeId = 2;

        public ushort errorcode;

        public SLPUrlEntry[] urlarray;

        public ushort urlcount;

        public bool ParsePayload()
        {
            byte[] ucp = payload;
            try
            {
                errorcode = SLP.AsUINT16(ucp, 0);
                urlcount = SLP.AsUINT16(ucp, 2);
            }
            catch (Exception exception)
            {
                Log.WarningException("SLPSrvRply.ParsePayload()", exception);
                return false;
            }
            return true;
        }
    }
}

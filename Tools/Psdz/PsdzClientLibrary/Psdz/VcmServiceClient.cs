using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model;

namespace BMW.Rheingold.Psdz.Client
{
    // [UH] Only for compatibility with older version
    internal sealed class VcmServiceClient : PsdzClientBase<IVcmService>, IVcmService
    {
        internal VcmServiceClient(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {
        }

        public IPsdzIstufenTriple GetIStufenTripleActual(IPsdzConnection connection)
        {
            return CallFunction((IVcmService m) => m.GetIStufenTripleActual(connection));
        }

        public IPsdzIstufenTriple GetIStufenTripleBackup(IPsdzConnection connection)
        {
            return CallFunction((IVcmService m) => m.GetIStufenTripleBackup(connection));
        }

        public IPsdzStandardFa GetStandardFaActual(IPsdzConnection connection)
        {
            return CallFunction((IVcmService m) => m.GetStandardFaActual(connection));
        }

        public IPsdzStandardFa GetStandardFaBackup(IPsdzConnection connection)
        {
            return CallFunction((IVcmService m) => m.GetStandardFaBackup(connection));
        }

        public IPsdzStandardFp GetStandardFp(IPsdzConnection connection)
        {
            return CallFunction((IVcmService m) => m.GetStandardFp(connection));
        }

        public IPsdzStandardSvt GetStandardSvtActual(IPsdzConnection connection)
        {
            return CallFunction((IVcmService m) => m.GetStandardSvtActual(connection));
        }

        public IPsdzVin GetVinFromBackup(IPsdzConnection connection)
        {
            return CallFunction((IVcmService m) => m.GetVinFromBackup(connection));
        }

        public IPsdzVin GetVinFromMaster(IPsdzConnection connection)
        {
            return CallFunction((IVcmService m) => m.GetVinFromMaster(connection));
        }

        public IPsdzReadVpcFromVcmCto RequestVpcFromVcm(IPsdzConnection connection)
        {
            return CallFunction((IVcmService m) => m.RequestVpcFromVcm(connection));
        }

        public PsdzResultStateEto WriteFa(IPsdzConnection connection, IPsdzStandardFa standardFa)
        {
            return CallFunction((IVcmService m) => m.WriteFa(connection, standardFa));
        }

        public PsdzResultStateEto WriteFaToBackup(IPsdzConnection connection, IPsdzStandardFa standardFa)
        {
            return CallFunction((IVcmService m) => m.WriteFaToBackup(connection, standardFa));
        }

        public PsdzResultStateEto WriteFp(IPsdzConnection connection, IPsdzStandardFp standardFp)
        {
            return CallFunction((IVcmService m) => m.WriteFp(connection, standardFp));
        }

        public PsdzResultStateEto WriteIStufen(IPsdzConnection connection, string iStufeShipment, string iStufeLast, string iStufeCurrent)
        {
            return CallFunction((IVcmService m) => m.WriteIStufen(connection, iStufeShipment, iStufeLast, iStufeCurrent));
        }

        public PsdzResultStateEto WriteIStufenToBackup(IPsdzConnection connection, string iStufeShipment, string iStufeLast, string iStufeCurrent)
        {
            return CallFunction((IVcmService m) => m.WriteIStufenToBackup(connection, iStufeShipment, iStufeLast, iStufeCurrent));
        }

        public PsdzResultStateEto WriteSvt(IPsdzConnection connection, IPsdzStandardSvt standardSvt)
        {
            return CallFunction((IVcmService m) => m.WriteSvt(connection, standardSvt));
        }
    }
}

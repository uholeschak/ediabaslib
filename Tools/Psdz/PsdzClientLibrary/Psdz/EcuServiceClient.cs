using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz.Client
{
    internal sealed class EcuServiceClient : PsdzClientBase<IEcuService>, IEcuService
    {
        internal EcuServiceClient(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {
        }

        public IPsdzStandardSvt RequestSvt(IPsdzConnection connection)
        {
            return CallFunction((IEcuService m) => m.RequestSvt(connection));
        }

        public IPsdzStandardSvt RequestSvt(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus)
        {
            return CallFunction((IEcuService m) => m.RequestSvt(connection, installedEcus));
        }

        public IPsdzSvt RequestSvtWithSmacs(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus)
        {
            return CallFunction((IEcuService m) => m.RequestSvtWithSmacs(connection, installedEcus));
        }

        public IPsdzSvt RequestSVTwithSmAcAndMirror(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus)
        {
            return CallFunction((IEcuService m) => m.RequestSVTwithSmAcAndMirror(connection, installedEcus));
        }

        public IEnumerable<IPsdzEcuContextInfo> RequestEcuContextInfos(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus)
        {
            return CallFunction((IEcuService m) => m.RequestEcuContextInfos(connection, installedEcus));
        }

        public IPsdzResponse UpdatePiaPortierungsmaster(IPsdzConnection connection, IPsdzSvt svt)
        {
            return CallFunction((IEcuService m) => m.UpdatePiaPortierungsmaster(connection, svt));
        }
    }
}

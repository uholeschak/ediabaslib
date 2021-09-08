using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    class EcuServiceClient : PsdzClientBase<IEcuService>, IEcuService
    {
        internal EcuServiceClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
        {
        }

        public IPsdzStandardSvt RequestSvt(IPsdzConnection connection)
        {
            return base.CallFunction<IPsdzStandardSvt>((IEcuService m) => m.RequestSvt(connection));
        }

        public IPsdzStandardSvt RequestSvt(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus)
        {
            return base.CallFunction<IPsdzStandardSvt>((IEcuService m) => m.RequestSvt(connection, installedEcus));
        }

        public IEnumerable<IPsdzEcuContextInfo> RequestEcuContextInfos(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus)
        {
            return base.CallFunction<IEnumerable<IPsdzEcuContextInfo>>((IEcuService m) => m.RequestEcuContextInfos(connection, installedEcus));
        }

        public IPsdzResponse UpdatePiaPortierungsmaster(IPsdzConnection connection, IPsdzSvt svt)
        {
            return base.CallFunction<IPsdzResponse>((IEcuService m) => m.UpdatePiaPortierungsmaster(connection, svt));
        }
    }
}

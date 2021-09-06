using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    class EcuServiceClient : PsdzClientBase<IEcuService>, IEcuService
    {
        // Token: 0x06000074 RID: 116 RVA: 0x00002517 File Offset: 0x00000717
        internal EcuServiceClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
        {
        }

        // Token: 0x06000075 RID: 117 RVA: 0x00003D78 File Offset: 0x00001F78
        public IPsdzStandardSvt RequestSvt(IPsdzConnection connection)
        {
            return base.CallFunction<IPsdzStandardSvt>((IEcuService m) => m.RequestSvt(connection));
        }

        // Token: 0x06000076 RID: 118 RVA: 0x00003DA4 File Offset: 0x00001FA4
        public IPsdzStandardSvt RequestSvt(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus)
        {
            return base.CallFunction<IPsdzStandardSvt>((IEcuService m) => m.RequestSvt(connection, installedEcus));
        }

        // Token: 0x06000077 RID: 119 RVA: 0x00003DD8 File Offset: 0x00001FD8
        public IEnumerable<IPsdzEcuContextInfo> RequestEcuContextInfos(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus)
        {
            return base.CallFunction<IEnumerable<IPsdzEcuContextInfo>>((IEcuService m) => m.RequestEcuContextInfos(connection, installedEcus));
        }

        // Token: 0x06000078 RID: 120 RVA: 0x00003E0C File Offset: 0x0000200C
        public IPsdzResponse UpdatePiaPortierungsmaster(IPsdzConnection connection, IPsdzSvt svt)
        {
            return base.CallFunction<IPsdzResponse>((IEcuService m) => m.UpdatePiaPortierungsmaster(connection, svt));
        }
    }
}

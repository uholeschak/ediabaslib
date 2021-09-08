using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    class SecurityManagementServiceClient : PsdzDuplexClientBase<ISecurityManagementService, IPsdzProgressListener>, ISecurityManagementService
    {
        internal SecurityManagementServiceClient(IPsdzProgressListener progressListener, Binding binding, EndpointAddress remoteAddress) : base(progressListener, binding, remoteAddress)
        {
        }

        public IPsdzReadEcuUidResultCto readEcuUid(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> ecus, IPsdzSvt svt)
        {
            return base.CallFunction<IPsdzReadEcuUidResultCto>((ISecurityManagementService service) => service.readEcuUid(connection, ecus, svt));
        }
    }
}

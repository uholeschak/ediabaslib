using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.SecurityManagement;

namespace BMW.Rheingold.Psdz.Client
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

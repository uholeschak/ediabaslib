using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.SecurityManagement;
using BMW.Rheingold.Psdz.Model.Sfa;
using PsdzClientLibrary;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace BMW.Rheingold.Psdz.Client
{
    [PreserveSource(Removed = true)]
    internal sealed class SecurityManagementServiceClient : PsdzDuplexClientBase<ISecurityManagementService, IPsdzProgressListener>, ISecurityManagementService
    {
        internal SecurityManagementServiceClient(IPsdzProgressListener progressListener, Binding binding, EndpointAddress remoteAddress)
            : base(progressListener, binding, remoteAddress)
        {
        }

        public IEnumerable<IPsdzEcuIdentifier> GenerateECUlistWithIPsecBitmasksDiffering(IPsdzConnection connection, byte[] targetBm, IDictionary<IPsdzEcuIdentifier, byte[]> ecuBms)
        {
            return CallFunction((ISecurityManagementService service) => service.GenerateECUlistWithIPsecBitmasksDiffering(connection, targetBm, ecuBms));
        }

        public IPsdzTargetBitmask GenerateIPSecTargetBitmask(IPsdzConnection connection, IPsdzSvt svt)
        {
            return CallFunction((ISecurityManagementService service) => service.GenerateIPSecTargetBitmask(connection, svt));
        }

        public IEnumerable<IPsdzEcuIdentifier> GetIPsecEnabledECUs(IPsdzSvt svt)
        {
            return CallFunction((ISecurityManagementService service) => service.GetIPsecEnabledECUs(svt));
        }

        public IPsdzReadEcuUidResultCto readEcuUid(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> ecus, IPsdzSvt svt)
        {
            return CallFunction((ISecurityManagementService service) => service.readEcuUid(connection, ecus, svt));
        }

        public IPsdzIPsecEcuBitmaskResultCto ReadIPsecBitmasks(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> ecus, IPsdzSvt svt)
        {
            return CallFunction((ISecurityManagementService service) => service.ReadIPsecBitmasks(connection, ecus, svt));
        }

        public IEnumerable<IPsdzEcuFailureResponseCto> WriteIPsecBitmasks(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> ecus, byte[] targetBm, IPsdzSvt svt)
        {
            return CallFunction((ISecurityManagementService service) => service.WriteIPsecBitmasks(connection, ecus, targetBm, svt));
        }
    }
}

using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Exceptions;
using PsdzClient;
using System.Collections.Generic;
using System.ServiceModel;

namespace BMW.Rheingold.Psdz
{
    [PreserveSource(AttributesModified = true)]
    [ServiceContract(SessionMode = SessionMode.Required)]
    [ServiceKnownType(typeof(PsdzSgbmId))]
    [ServiceKnownType(typeof(PsdzEcuIdentifier))]
    [ServiceKnownType(typeof(PsdzFa))]
    [ServiceKnownType(typeof(PsdzIstufe))]
    [ServiceKnownType(typeof(PsdzConnection))]
    public interface IMacrosService
    {
        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzSgbmId> CheckSoftwareEntries(IEnumerable<IPsdzSgbmId> sgbmIds);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzEcuIdentifier> GetInstalledEcuList(IPsdzFa fa, IPsdzIstufe iStufe);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzEcuIdentifier> GetInstalledEcuListWithConnection(IPsdzConnection connection, IPsdzFa fa, IPsdzIstufe iStufe);
    }
}

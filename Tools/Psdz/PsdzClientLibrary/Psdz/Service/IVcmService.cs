using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Exceptions;
using PsdzClient;
using System.ServiceModel;

namespace BMW.Rheingold.Psdz
{
    public enum PsdzResultStateEto
    {
        FINISHED,
        FINISHED_WITH_ERROR,
        FINISHED_WITH_WARNINGS,
        UNKNOWN
    }

    [PreserveSource(AttributesModified = true)]
    [ServiceContract(SessionMode = SessionMode.Required)]
    [ServiceKnownType(typeof(PsdzConnection))]
    [ServiceKnownType(typeof(PsdzFa))]
    [ServiceKnownType(typeof(PsdzStandardFa))]
    [ServiceKnownType(typeof(PsdzFp))]
    [ServiceKnownType(typeof(PsdzStandardFp))]
    [ServiceKnownType(typeof(PsdzSvt))]
    [ServiceKnownType(typeof(PsdzStandardSvt))]
    [ServiceKnownType(typeof(PsdzVin))]
    [ServiceKnownType(typeof(PsdzIstufenTriple))]
    [ServiceKnownType(typeof(PsdzReadVpcFromVcmCto))]
    public interface IVcmService
    {
        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzIstufenTriple GetIStufenTripleActual(IPsdzConnection connection);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzIstufenTriple GetIStufenTripleBackup(IPsdzConnection connection);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzStandardFa GetStandardFaActual(IPsdzConnection connection);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzStandardFa GetStandardFaBackup(IPsdzConnection connection);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzStandardFp GetStandardFp(IPsdzConnection connection);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzStandardSvt GetStandardSvtActual(IPsdzConnection connection);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzVin GetVinFromBackup(IPsdzConnection connection);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzVin GetVinFromMaster(IPsdzConnection connection);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        PsdzResultStateEto WriteFa(IPsdzConnection connection, IPsdzStandardFa standardFa);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        PsdzResultStateEto WriteFaToBackup(IPsdzConnection connection, IPsdzStandardFa standardFa);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        PsdzResultStateEto WriteFp(IPsdzConnection connection, IPsdzStandardFp standardFp);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        PsdzResultStateEto WriteIStufen(IPsdzConnection connection, string iStufeShipment, string iStufeLast, string iStufeCurrent);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        PsdzResultStateEto WriteIStufenToBackup(IPsdzConnection connection, string iStufeShipment, string iStufeLast, string iStufeCurrent);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        PsdzResultStateEto WriteSvt(IPsdzConnection connection, IPsdzStandardSvt standardSvt);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzReadVpcFromVcmCto RequestVpcFromVcm(IPsdzConnection connection);
    }
}

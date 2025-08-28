using System.ServiceModel;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Exceptions;

namespace BMW.Rheingold.Psdz
{
    public enum PsdzResultStateEto
    {
        FINISHED,
        FINISHED_WITH_ERROR,
        FINISHED_WITH_WARNINGS,
        UNKNOWN
    }

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
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzIstufenTriple GetIStufenTripleActual(IPsdzConnection connection);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzIstufenTriple GetIStufenTripleBackup(IPsdzConnection connection);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzStandardFa GetStandardFaActual(IPsdzConnection connection);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzStandardFa GetStandardFaBackup(IPsdzConnection connection);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzStandardFp GetStandardFp(IPsdzConnection connection);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzStandardSvt GetStandardSvtActual(IPsdzConnection connection);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzVin GetVinFromBackup(IPsdzConnection connection);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzVin GetVinFromMaster(IPsdzConnection connection);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        PsdzResultStateEto WriteFa(IPsdzConnection connection, IPsdzStandardFa standardFa);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        PsdzResultStateEto WriteFaToBackup(IPsdzConnection connection, IPsdzStandardFa standardFa);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        PsdzResultStateEto WriteFp(IPsdzConnection connection, IPsdzStandardFp standardFp);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        PsdzResultStateEto WriteIStufen(IPsdzConnection connection, string iStufeShipment, string iStufeLast, string iStufeCurrent);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        PsdzResultStateEto WriteIStufenToBackup(IPsdzConnection connection, string iStufeShipment, string iStufeLast, string iStufeCurrent);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        PsdzResultStateEto WriteSvt(IPsdzConnection connection, IPsdzStandardSvt standardSvt);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzReadVpcFromVcmCto RequestVpcFromVcm(IPsdzConnection connection);
    }
}

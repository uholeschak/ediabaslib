using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Certificate
{
    public enum PsdzSecurityCalculationOverallStatus
    {
        Conflict,
        Detailed,
        Empty,
        Error,
        Malformed,
        Ok,
        UnknownVersion,
        VinMalformed,
        WrongFormat,
        InvalidFatRequest,
        RequestNotOnCertStore,
        InvalidEcuTypeCerts,
        InvalidSignature,
        OTHER_ERROR
    }

    public enum PsdzCertCalculationDetailedStatus
    {
        CertDenied,
        CnameDenied,
        CnameMissing,
        EcuNameDenied,
        EcuNameMissing,
        EcuUidDenied,
        EcuUidFormat,
        EcuUidMismatch,
        EcuUidMissing,
        Error,
        Malformed,
        Ok,
        CaError,
        EcuDenied,
        KeyIdUnknown,
        KeyIdDenied
    }

    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzSecurityCalculatedObjectCto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzCertMemoryObject MemoryObject { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzSecurityCalculationOverallStatus OverallStatus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IDictionary<string, PsdzCertCalculationDetailedStatus> RoleStatus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IDictionary<string, PsdzCertCalculationDetailedStatus> KeyIdStatus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string ServicePack { get; set; }
    }
}

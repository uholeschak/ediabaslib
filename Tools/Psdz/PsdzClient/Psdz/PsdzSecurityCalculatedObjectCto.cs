using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
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

    [DataContract]
    public class PsdzSecurityCalculatedObjectCto
    {
        [DataMember]
        public PsdzCertMemoryObject MemoryObject { get; set; }

        [DataMember]
        public PsdzSecurityCalculationOverallStatus OverallStatus { get; set; }

        [DataMember]
        public IDictionary<string, PsdzCertCalculationDetailedStatus> RoleStatus { get; set; }

        [DataMember]
        public IDictionary<string, PsdzCertCalculationDetailedStatus> KeyIdStatus { get; set; }

        [DataMember]
        public string ServicePack { get; set; }
    }
}

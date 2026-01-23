using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzSecureCodingConfigCto : IPsdzSecureCodingConfigCto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzBackendNcdCalculationEtoEnum BackendNcdCalculationEtoEnum { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzBackendSignatureEtoEnum BackendSignatureEtoEnum { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int ConnectionTimeout { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IList<string> Crls { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string NcdRootDirectory { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzNcdRecalculationEtoEnum NcdRecalculationEtoEnum { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int Retries { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int ScbPollingTimeout { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IList<string> ScbUrls { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IList<string> SwlSecBackendUrls { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzAuthenticationTypeEto PsdzAuthenticationTypeEto { get; set; }
    }
}

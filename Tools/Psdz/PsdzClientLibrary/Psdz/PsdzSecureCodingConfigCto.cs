using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    [DataContract]
    public class PsdzSecureCodingConfigCto : IPsdzSecureCodingConfigCto
    {
        [DataMember]
        public PsdzBackendNcdCalculationEtoEnum BackendNcdCalculationEtoEnum { get; set; }

        [DataMember]
        public PsdzBackendSignatureEtoEnum BackendSignatureEtoEnum { get; set; }

        [DataMember]
        public int ConnectionTimeout { get; set; }

        [DataMember]
        public IList<string> Crls { get; set; }

        [DataMember]
        public string NcdRootDirectory { get; set; }

        [DataMember]
        public PsdzNcdRecalculationEtoEnum NcdRecalculationEtoEnum { get; set; }

        [DataMember]
        public int Retries { get; set; }

        [DataMember]
        public int ScbPollingTimeout { get; set; }

        [DataMember]
        public IList<string> ScbUrls { get; set; }

        [DataMember]
        public IList<string> SwlSecBackendUrls { get; set; }

        [DataMember]
        public PsdzAuthenticationTypeEto PsdzAuthenticationTypeEto { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    [DataContract]
    internal class EcuObjPdxInfo : IEcuPdxInfo
    {
        [DataMember]
        public int CertVersion { get; internal set; }

        [DataMember]
        public bool IsCert2018 { get; internal set; }

        [DataMember]
        public bool IsCert2021 { get; internal set; }

        [DataMember]
        public bool IsCertEnabled { get; internal set; }

        [DataMember]
        public bool IsSecOcEnabled { get; internal set; }

        [DataMember]
        public bool IsSfaEnabled { get; internal set; }

        [DataMember]
        public bool IsIPSecEnabled { get; internal set; }

        [DataMember]
        public bool IsLcsServicePackSupported { get; internal set; }

        [DataMember]
        public bool IsLcsSystemTimeSwitchSupported { get; internal set; }

        [DataMember]
        public bool IsMirrorProtocolSupported { get; internal set; }

        [DataMember]
        public bool IsEcuAuthEnabled { get; internal set; }

        [DataMember]
        public bool IsIPsecBitmaskSupported { get; internal set; }

        [DataMember]
        public int ProgrammingProtectionLevel { get; internal set; }

        [DataMember]
        public bool IsSmartActuatorMaster { get; internal set; }
    }
}

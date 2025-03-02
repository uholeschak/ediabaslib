using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Ecu
{
    public interface IPsdzEcuPdxInfo
    {
        int CertVersion { get; }

        bool IsCert2018 { get; }

        bool IsCert2021 { get; }

        bool IsCertEnabled { get; }

        bool IsSecOcEnabled { get; }

        bool IsSecOcMaster { get; }

        bool IsSfaEnabled { get; }

        bool IsIPSecEnabled { get; }

        bool IsLcsServicePackSupported { get; }

        bool IsLcsSystemTimeSwitchSupported { get; }

        bool IsMirrorProtocolSupported { get; }

        bool IsEcuAuthEnabled { get; }

        bool IsIPsecBitmaskSupported { get; }

        int ProgrammingProtectionLevel { get; }

        bool IsMACsecEnabled { get; }

        bool AclEnabled { get; }

        bool IsSmartActuatorMaster { get; }

        bool UpdateSmartActuatorConfigurationSupported { get; }

        bool LcsIntegrityProtectionOCSupported { get; }

        bool LcsIukCluster { get; }

        int ServicePack { get; }
    }
}

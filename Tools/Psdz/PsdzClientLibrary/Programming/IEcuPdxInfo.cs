using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IEcuPdxInfo
    {
        int CertVersion { get; }

        bool IsCert2018 { get; }

        bool IsCert2021 { get; }

        bool IsCert2025 { get; }

        bool IsCertEnabled { get; }

        bool IsSecOcEnabled { get; }

        bool IsSfaEnabled { get; }

        bool IsIPSecEnabled { get; }

        bool IsLcsServicePackSupported { get; }

        bool IsLcsSystemTimeSwitchSupported { get; }

        bool IsLcsIntegrityProtectionOCSupported { get; }

        bool IsLcsIukCluster { get; }

        bool IsMACsecEnabled { get; }

        bool IsMirrorProtocolSupported { get; }

        bool IsEcuAuthEnabled { get; }

        bool IsIPsecBitmaskSupported { get; }

        int ProgrammingProtectionLevel { get; }

        bool IsSmartActuatorMaster { get; }

        int ServicePack { get; }

        bool IsAclEnabled { get; }
    }
}
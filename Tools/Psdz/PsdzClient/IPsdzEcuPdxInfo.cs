using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public interface IPsdzEcuPdxInfo
    {
        int CertVersion { get; }

        bool IsCert2018 { get; }

        bool IsCert2021 { get; }

        bool IsCertEnabled { get; }

        bool IsSecOcEnabled { get; }

        bool IsSfaEnabled { get; }

        bool IsIPSecEnabled { get; }

        bool IsLcsServicePackSupported { get; }

        bool IsLcsSystemTimeSwitchSupported { get; }

        bool IsMirrorProtocolSupported { get; }
    }
}

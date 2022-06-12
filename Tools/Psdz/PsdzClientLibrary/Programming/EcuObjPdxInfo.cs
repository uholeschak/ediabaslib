using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public class EcuObjPdxInfo : IEcuPdxInfo
    {
        public int CertVersion { get; internal set; }

        public bool IsCert2018 { get; internal set; }

        public bool IsCert2021 { get; internal set; }

        public bool IsCertEnabled { get; internal set; }

        public bool IsSecOcEnabled { get; internal set; }

        public bool IsSfaEnabled { get; internal set; }

        public bool IsIPSecEnabled { get; internal set; }

        public bool IsLcsServicePackSupported { get; internal set; }

        public bool IsLcsSystemTimeSwitchSupported { get; internal set; }
    }
}

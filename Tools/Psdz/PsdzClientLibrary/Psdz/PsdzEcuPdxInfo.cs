using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Ecu
{
    [DataContract]
    public class PsdzEcuPdxInfo : IPsdzEcuPdxInfo
    {
        [DataMember]
        public int CertVersion { get; set; }

        [DataMember]
        public bool IsCert2018 { get; set; }

        [DataMember]
        public bool IsCert2021 { get; set; }

        [DataMember]
        public bool IsCert2025 { get; set; }

        [DataMember]
        public bool IsCertEnabled { get; set; }

        [DataMember]
        public bool IsSecOcEnabled { get; set; }

        [DataMember]
        public bool IsSecOcMaster { get; set; }

        [DataMember]
        public bool IsSfaEnabled { get; set; }

        [DataMember]
        public bool IsIPSecEnabled { get; set; }

        [DataMember]
        public bool IsLcsServicePackSupported { get; set; }

        [DataMember]
        public bool IsLcsSystemTimeSwitchSupported { get; set; }

        [DataMember]
        public bool IsMirrorProtocolSupported { get; set; }

        [DataMember]
        public bool IsEcuAuthEnabled { get; set; }

        [DataMember]
        public bool IsIPsecBitmaskSupported { get; set; }

        [DataMember]
        public int ProgrammingProtectionLevel { get; set; }

        [DataMember]
        public bool IsMACsecEnabled { get; set; }

        [DataMember]
        public bool AclEnabled { get; set; }

        [DataMember]
        public bool IsSmartActuatorMaster { get; set; }

        [DataMember]
        public bool UpdateSmartActuatorConfigurationSupported { get; set; }

        [DataMember]
        public bool LcsIntegrityProtectionOCSupported { get; set; }

        [DataMember]
        public bool LcsIukCluster { get; set; }

        [DataMember]
        public int ServicePack { get; set; }

        public PsdzEcuPdxInfo()
        {
        }

        public PsdzEcuPdxInfo(PsdzEcuPdxInfoCtorModel model, int programmingProtectionLevel = 0)
        {
            CertVersion = model.CertVersion;
            IsSecOcEnabled = model.IsSecOcEnabled;
            IsSecOcMaster = model.IsSecOcMaster;
            IsSfaEnabled = model.IsSfaEnabled;
            IsIPSecEnabled = model.IsIpSecEnabled;
            IsLcsServicePackSupported = model.IsLcsServicePackSupported;
            IsLcsSystemTimeSwitchSupported = model.IsLcsSystemTimeSwitchSupported;
            IsCert2018 = model.IsCert2018;
            IsCert2021 = model.IsCert2021;
            IsCert2025 = model.IsCert2025;
            IsCertEnabled = model.IsCertEnabled;
            IsMirrorProtocolSupported = model.IsMirrorProtocolSupported;
            IsEcuAuthEnabled = model.IsEcuAuthEnabled;
            IsIPsecBitmaskSupported = model.IsIPsecBitmaskSupported;
            ProgrammingProtectionLevel = programmingProtectionLevel;
            IsMACsecEnabled = model.IsMACsecEnabled;
            AclEnabled = model.AclEnabled;
            IsSmartActuatorMaster = model.IsSmartActuatorMaster;
            UpdateSmartActuatorConfigurationSupported = model.UpdateSmartActuatorConfigurationSupported;
            LcsIntegrityProtectionOCSupported = model.LcsIntegrityProtectionOCSupported;
            LcsIukCluster = model.LcsIukCluster;
            ServicePack = model.ServicePack;
        }

        public override bool Equals(object obj)
        {
            PsdzEcuPdxInfo psdzEcuPdxInfo = obj as PsdzEcuPdxInfo;
            if (CertVersion == psdzEcuPdxInfo.CertVersion && IsCert2018 == psdzEcuPdxInfo.IsCert2018 && IsCert2021 == psdzEcuPdxInfo.IsCert2021 && IsCertEnabled == psdzEcuPdxInfo.IsCertEnabled && IsSecOcEnabled == psdzEcuPdxInfo.IsSecOcEnabled && IsSfaEnabled == psdzEcuPdxInfo.IsSfaEnabled && IsIPSecEnabled == psdzEcuPdxInfo.IsIPSecEnabled && IsLcsServicePackSupported == psdzEcuPdxInfo.IsLcsServicePackSupported && IsLcsSystemTimeSwitchSupported == psdzEcuPdxInfo.IsLcsSystemTimeSwitchSupported && IsMirrorProtocolSupported == psdzEcuPdxInfo.IsMirrorProtocolSupported && IsEcuAuthEnabled == psdzEcuPdxInfo.IsEcuAuthEnabled && IsIPsecBitmaskSupported == psdzEcuPdxInfo.IsIPsecBitmaskSupported)
            {
                return ProgrammingProtectionLevel == psdzEcuPdxInfo.ProgrammingProtectionLevel;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}", CertVersion, IsCert2018, IsCert2021, IsCertEnabled, IsSecOcEnabled, IsSfaEnabled, IsMirrorProtocolSupported, IsIPsecBitmaskSupported, ProgrammingProtectionLevel);
        }
    }
}

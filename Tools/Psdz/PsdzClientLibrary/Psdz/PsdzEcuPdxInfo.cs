using PsdzClient;
using System.Globalization;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Ecu
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzEcuPdxInfo : IPsdzEcuPdxInfo
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int CertVersion { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsCert2018 { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsCert2021 { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsCert2025 { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsCertEnabled { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsSecOcEnabled { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsSecOcMaster { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsSfaEnabled { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsIPSecEnabled { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsLcsServicePackSupported { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsLcsSystemTimeSwitchSupported { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsMirrorProtocolSupported { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsEcuAuthEnabled { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsIPsecBitmaskSupported { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int ProgrammingProtectionLevel { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsMACsecEnabled { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool AclEnabled { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsSmartActuatorMaster { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool UpdateSmartActuatorConfigurationSupported { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool LcsIntegrityProtectionOCSupported { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool LcsIukCluster { get; set; }

        [PreserveSource(KeepAttribute = true)]
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
            if (this == obj)
            {
                return true;
            }

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            PsdzEcuPdxInfo psdzEcuPdxInfo = (PsdzEcuPdxInfo)obj;
            if (CertVersion == psdzEcuPdxInfo.CertVersion && IsCert2018 == psdzEcuPdxInfo.IsCert2018 && IsCert2021 == psdzEcuPdxInfo.IsCert2021 && IsCert2025 == psdzEcuPdxInfo.IsCert2025 && IsCertEnabled == psdzEcuPdxInfo.IsCertEnabled && IsSecOcEnabled == psdzEcuPdxInfo.IsSecOcEnabled && IsSecOcMaster == psdzEcuPdxInfo.IsSecOcMaster && IsSfaEnabled == psdzEcuPdxInfo.IsSfaEnabled && IsIPSecEnabled == psdzEcuPdxInfo.IsIPSecEnabled && IsLcsServicePackSupported == psdzEcuPdxInfo.IsLcsServicePackSupported && IsLcsSystemTimeSwitchSupported == psdzEcuPdxInfo.IsLcsSystemTimeSwitchSupported && IsMirrorProtocolSupported == psdzEcuPdxInfo.IsMirrorProtocolSupported && IsEcuAuthEnabled == psdzEcuPdxInfo.IsEcuAuthEnabled && IsIPsecBitmaskSupported == psdzEcuPdxInfo.IsIPsecBitmaskSupported && ProgrammingProtectionLevel == psdzEcuPdxInfo.ProgrammingProtectionLevel && IsMACsecEnabled == psdzEcuPdxInfo.IsMACsecEnabled && AclEnabled == psdzEcuPdxInfo.AclEnabled && IsSmartActuatorMaster == psdzEcuPdxInfo.IsSmartActuatorMaster && UpdateSmartActuatorConfigurationSupported == psdzEcuPdxInfo.UpdateSmartActuatorConfigurationSupported && LcsIntegrityProtectionOCSupported == psdzEcuPdxInfo.LcsIntegrityProtectionOCSupported && LcsIukCluster == psdzEcuPdxInfo.LcsIukCluster)
            {
                return ServicePack == psdzEcuPdxInfo.ServicePack;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (CertVersion, IsCert2018, IsCert2021, IsCert2025, IsCertEnabled, IsSecOcEnabled, IsSecOcMaster, IsSfaEnabled, IsIPSecEnabled, IsLcsServicePackSupported, IsLcsSystemTimeSwitchSupported, IsMirrorProtocolSupported, IsEcuAuthEnabled, IsIPsecBitmaskSupported, ProgrammingProtectionLevel, IsMACsecEnabled, AclEnabled, IsSmartActuatorMaster, UpdateSmartActuatorConfigurationSupported, LcsIntegrityProtectionOCSupported, LcsIukCluster, ServicePack).GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}:{9}:{10}:{11}:{12}:{13}:{14}:{15}:{16}:{17}:{18}:{19}:{20}:{21}", CertVersion, IsCert2018, IsCert2021, IsCert2025, IsCertEnabled, IsSecOcEnabled, IsSecOcMaster, IsSfaEnabled, IsIPSecEnabled, IsLcsServicePackSupported, IsLcsSystemTimeSwitchSupported, IsMirrorProtocolSupported, IsEcuAuthEnabled, IsIPsecBitmaskSupported, ProgrammingProtectionLevel, IsMACsecEnabled, AclEnabled, IsSmartActuatorMaster, UpdateSmartActuatorConfigurationSupported, LcsIntegrityProtectionOCSupported, LcsIukCluster, ServicePack);
        }
    }
}
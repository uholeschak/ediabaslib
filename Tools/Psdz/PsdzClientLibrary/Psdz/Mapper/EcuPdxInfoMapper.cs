using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz
{
    internal static class EcuPdxInfoMapper
    {
        public static IPsdzEcuPdxInfo Map(EcuPdxInfoModel ecuPdxInfoModel)
        {
            if (ecuPdxInfoModel == null)
            {
                return null;
            }

            return new PsdzEcuPdxInfo
            {
                CertVersion = ecuPdxInfoModel.CertVersion,
                IsCert2018 = ecuPdxInfoModel.Cert2018,
                IsCert2021 = ecuPdxInfoModel.Cert2021,
                IsCertEnabled = ecuPdxInfoModel.CertEnabled,
                IsEcuAuthEnabled = ecuPdxInfoModel.EcuAuthEnabled,
                IsIPSecEnabled = ecuPdxInfoModel.IpsecEnabled,
                IsIPsecBitmaskSupported = ecuPdxInfoModel.IpsecBitmaskSupported,
                IsLcsServicePackSupported = ecuPdxInfoModel.LcsServicePackSupported,
                IsLcsSystemTimeSwitchSupported = ecuPdxInfoModel.LcsSystemTimeSwitchSupported,
                IsMirrorProtocolSupported = ecuPdxInfoModel.MirrorProtocolSupported,
                IsSecOcEnabled = ecuPdxInfoModel.SecOcEnabled,
                IsSfaEnabled = ecuPdxInfoModel.SfaEnabled,
                ProgrammingProtectionLevel = ecuPdxInfoModel.ProgrammingProtectionLevel,
                IsSmartActuatorMaster = ecuPdxInfoModel.SmartActuatorMaster,
                UpdateSmartActuatorConfigurationSupported = ecuPdxInfoModel.UpdateSmartActuatorConfigurationSupported,
                IsCert2025 = ecuPdxInfoModel.Cert2025,
                LcsIntegrityProtectionOCSupported = ecuPdxInfoModel.LcsIntegrityProtectionOCSupported,
                LcsIukCluster = ecuPdxInfoModel.LcsIukCluster,
                IsMACsecEnabled = ecuPdxInfoModel.MacsecEnabled,
                ServicePack = ecuPdxInfoModel.ServicePack,
                AclEnabled = ecuPdxInfoModel.AclEnabled
            };
        }

        public static EcuPdxInfoModel Map(IPsdzEcuPdxInfo ecuPdxInfo)
        {
            if (ecuPdxInfo == null)
            {
                return null;
            }

            return new EcuPdxInfoModel
            {
                CertVersion = ecuPdxInfo.CertVersion,
                Cert2018 = ecuPdxInfo.IsCert2018,
                Cert2021 = ecuPdxInfo.IsCert2021,
                Cert2025 = ecuPdxInfo.IsCert2025,
                CertEnabled = ecuPdxInfo.IsCertEnabled,
                EcuAuthEnabled = ecuPdxInfo.IsEcuAuthEnabled,
                IpsecEnabled = ecuPdxInfo.IsIPSecEnabled,
                IpsecBitmaskSupported = ecuPdxInfo.IsIPsecBitmaskSupported,
                LcsServicePackSupported = ecuPdxInfo.IsLcsServicePackSupported,
                LcsSystemTimeSwitchSupported = ecuPdxInfo.IsLcsSystemTimeSwitchSupported,
                LcsIntegrityProtectionOCSupported = ecuPdxInfo.LcsIntegrityProtectionOCSupported,
                LcsIukCluster = ecuPdxInfo.LcsIukCluster,
                MacsecEnabled = ecuPdxInfo.IsMACsecEnabled,
                AclEnabled = ecuPdxInfo.AclEnabled,
                MirrorProtocolSupported = ecuPdxInfo.IsMirrorProtocolSupported,
                SecOcEnabled = ecuPdxInfo.IsSecOcEnabled,
                SecOcMaster = ecuPdxInfo.IsSecOcMaster,
                SfaEnabled = ecuPdxInfo.IsSfaEnabled,
                ProgrammingProtectionLevel = ecuPdxInfo.ProgrammingProtectionLevel,
                SmartActuatorMaster = ecuPdxInfo.IsSmartActuatorMaster,
                UpdateSmartActuatorConfigurationSupported = ecuPdxInfo.UpdateSmartActuatorConfigurationSupported,
                ServicePack = ecuPdxInfo.ServicePack
            };
        }
    }
}
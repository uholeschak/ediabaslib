namespace BMW.Rheingold.Psdz.Model.Ecu
{
    public class PsdzEcuPdxInfoCtorModel
    {
        public bool IsSecOcEnabled { get; set; }
        public bool IsSecOcMaster { get; set; }
        public bool IsSfaEnabled { get; set; }
        public int CertVersion { get; set; }
        public bool IsIpSecEnabled { get; set; }
        public bool IsLcsServicePackSupported { get; set; }
        public bool IsLcsSystemTimeSwitchSupported { get; set; }
        public bool IsCert2018 { get; set; }
        public bool IsCert2021 { get; set; }
        public bool IsCert2025 { get; set; }
        public bool IsCertEnabled { get; set; }
        public bool IsMirrorProtocolSupported { get; set; }
        public bool IsIPsecBitmaskSupported { get; set; }
        public bool IsEcuAuthEnabled { get; set; }
        public bool IsMACsecEnabled { get; set; }
        public bool AclEnabled { get; set; }
        public bool IsSmartActuatorMaster { get; set; }
        public bool UpdateSmartActuatorConfigurationSupported { get; set; }
        public bool LcsIntegrityProtectionOCSupported { get; set; }
        public bool LcsIukCluster { get; set; }
        public int ServicePack { get; set; }
    }
}
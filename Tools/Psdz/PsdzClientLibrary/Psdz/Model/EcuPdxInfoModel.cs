using Newtonsoft.Json;

namespace BMW.Rheingold.Psdz
{
    public class EcuPdxInfoModel
    {
        [JsonProperty("aclEnabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool AclEnabled { get; set; }

        [JsonProperty("cert2018", NullValueHandling = NullValueHandling.Ignore)]
        public bool Cert2018 { get; set; }

        [JsonProperty("cert2021", NullValueHandling = NullValueHandling.Ignore)]
        public bool Cert2021 { get; set; }

        [JsonProperty("cert2025", NullValueHandling = NullValueHandling.Ignore)]
        public bool Cert2025 { get; set; }

        [JsonProperty("certEnabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool CertEnabled { get; set; }

        [JsonProperty("certVersion", NullValueHandling = NullValueHandling.Ignore)]
        public int CertVersion { get; set; }

        [JsonProperty("ecuAuthEnabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool EcuAuthEnabled { get; set; }

        [JsonProperty("ipsecBitmaskSupported", NullValueHandling = NullValueHandling.Ignore)]
        public bool IpsecBitmaskSupported { get; set; }

        [JsonProperty("ipsecEnabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool IpsecEnabled { get; set; }

        [JsonProperty("lcsIntegrityProtectionOCSupported", NullValueHandling = NullValueHandling.Ignore)]
        public bool LcsIntegrityProtectionOCSupported { get; set; }

        [JsonProperty("lcsIukCluster", NullValueHandling = NullValueHandling.Ignore)]
        public bool LcsIukCluster { get; set; }

        [JsonProperty("lcsServicePackSupported", NullValueHandling = NullValueHandling.Ignore)]
        public bool LcsServicePackSupported { get; set; }

        [JsonProperty("lcsSystemTimeSwitchSupported", NullValueHandling = NullValueHandling.Ignore)]
        public bool LcsSystemTimeSwitchSupported { get; set; }

        [JsonProperty("macsecEnabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool MacsecEnabled { get; set; }

        [JsonProperty("mirrorProtocolSupported", NullValueHandling = NullValueHandling.Ignore)]
        public bool MirrorProtocolSupported { get; set; }

        [JsonProperty("programmingProtectionLevel", NullValueHandling = NullValueHandling.Ignore)]
        public int ProgrammingProtectionLevel { get; set; }

        [JsonProperty("secOcEnabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool SecOcEnabled { get; set; }

        [JsonProperty("secOcMaster", NullValueHandling = NullValueHandling.Ignore)]
        public bool SecOcMaster { get; set; }

        [JsonProperty("servicePack", NullValueHandling = NullValueHandling.Ignore)]
        public int ServicePack { get; set; }

        [JsonProperty("sfaEnabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool SfaEnabled { get; set; }

        [JsonProperty("smartActuatorMaster", NullValueHandling = NullValueHandling.Ignore)]
        public bool SmartActuatorMaster { get; set; }

        [JsonProperty("updateSmartActuatorConfigurationSupported", NullValueHandling = NullValueHandling.Ignore)]
        public bool UpdateSmartActuatorConfigurationSupported { get; set; }
    }
}
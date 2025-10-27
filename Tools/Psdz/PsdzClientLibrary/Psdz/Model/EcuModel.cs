using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    [JsonConverter(typeof(JsonInheritanceConverter), new object[] { "discriminatorType" })]
    [JsonInheritance("SmartActuatorEcuModel", typeof(SmartActuatorEcuModel))]
    [JsonInheritance("SmartActuatorMasterEcuModel", typeof(SmartActuatorMasterEcuModel))]
    public class EcuModel
    {
        [JsonProperty("baseVariant", NullValueHandling = NullValueHandling.Ignore)]
        public string BaseVariant { get; set; }

        [JsonProperty("bnTnName", NullValueHandling = NullValueHandling.Ignore)]
        public string BnTnName { get; set; }

        [JsonProperty("busConnections", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<BusNameModel> BusConnections { get; set; }

        [JsonProperty("diagnosticBus", NullValueHandling = NullValueHandling.Ignore)]
        public BusNameModel DiagnosticBus { get; set; }

        [JsonProperty("ecuDetailInfo", NullValueHandling = NullValueHandling.Ignore)]
        public EcuDetailInfoModel EcuDetailInfo { get; set; }

        [JsonProperty("ecuPdxInfo", NullValueHandling = NullValueHandling.Ignore)]
        public EcuPdxInfoModel EcuPdxInfo { get; set; }

        [JsonProperty("ecuStatusInfo", NullValueHandling = NullValueHandling.Ignore)]
        public EcuStatusInfoModel EcuStatusInfo { get; set; }

        [JsonProperty("ecuVariant", NullValueHandling = NullValueHandling.Ignore)]
        public string EcuVariant { get; set; }

        [JsonProperty("gatewayDiagAddr", NullValueHandling = NullValueHandling.Ignore)]
        public DiagAddressModel GatewayDiagAddr { get; set; }

        [JsonProperty("smartActuator", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsSmartActuator { get; set; }

        [JsonProperty("primaryKey", NullValueHandling = NullValueHandling.Ignore)]
        public EcuIdentifierModel PrimaryKey { get; set; }

        [JsonProperty("serialNumber", NullValueHandling = NullValueHandling.Ignore)]
        public string SerialNumber { get; set; }

        [JsonProperty("standardSvk", NullValueHandling = NullValueHandling.Ignore)]
        public StandardSvkModel StandardSvk { get; set; }
    }
}
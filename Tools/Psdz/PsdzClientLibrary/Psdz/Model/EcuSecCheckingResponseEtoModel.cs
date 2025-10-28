using BMW.Rheingold.Psdz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public class EcuSecCheckingResponseEtoModel
    {
        [JsonProperty("bindingsStatus", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public EcuSecCheckingStatusEtoModel? BindingsStatus { get; set; }

        [JsonProperty("certificateBindingDetailStatus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<BindingDetailStatusModel> CertificateBindingDetailStatus { get; set; }

        [JsonProperty("certificateStatus", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public EcuSecCheckingStatusEtoModel? CertificateStatus { get; set; }

        [JsonProperty("creationTimestamp", NullValueHandling = NullValueHandling.Ignore)]
        public string CreationTimestamp { get; set; }

        [JsonProperty("ecu", NullValueHandling = NullValueHandling.Ignore)]
        public EcuIdentifierModel Ecu { get; set; }

        [JsonProperty("keypackDetailStatus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<KeypackDetailStatusModel> KeypackDetailStatus { get; set; }

        [JsonProperty("keypackStatus", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public EcuSecCheckingStatusEtoModel? KeypackStatus { get; set; }

        [JsonProperty("onlineBindingDetailStatus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<BindingDetailStatusModel> OnlineBindingDetailStatus { get; set; }

        [JsonProperty("onlineBindingsStatus", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public EcuSecCheckingStatusEtoModel? OnlineBindingsStatus { get; set; }

        [JsonProperty("onlineCertificateStatus", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public EcuSecCheckingStatusEtoModel? OnlineCertificateStatus { get; set; }

        [JsonProperty("otherBindingDetailStatus", NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<OtherBindingDetailStatusModel> OtherBindingDetailStatus { get; set; }

        [JsonProperty("otherBindingsStatus", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public EcuSecCheckingStatusEtoModel? OtherBindingsStatus { get; set; }
    }
}
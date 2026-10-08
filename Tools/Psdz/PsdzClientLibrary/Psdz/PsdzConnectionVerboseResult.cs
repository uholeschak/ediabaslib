using BMW.Rheingold.Psdz.Client;
using PsdzClient;
using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu
{
    public class PsdzConnectionVerboseResult : IPsdzConnectionVerboseResult
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool CheckConnection { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Message { get; set; }
    }
}
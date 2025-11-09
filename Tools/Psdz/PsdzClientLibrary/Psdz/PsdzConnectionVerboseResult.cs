using BMW.Rheingold.Psdz.Client;
using PsdzClientLibrary;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
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
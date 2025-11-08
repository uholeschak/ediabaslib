using BMW.Rheingold.Psdz.Client;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public class PsdzConnectionVerboseResult : IPsdzConnectionVerboseResult
    {
        // [UH] Keep data members for backward compatibility
        [DataMember]
        public bool CheckConnection { get; set; }

        [DataMember]
        public string Message { get; set; }
    }
}
using BMW.Rheingold.Psdz.Model.Tal;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [DataContract]
    public class PsdzHddUpdateTA : PsdzTa
    {
        [DataMember]
        public long SecondsToCompletion { get; set; }
    }
}
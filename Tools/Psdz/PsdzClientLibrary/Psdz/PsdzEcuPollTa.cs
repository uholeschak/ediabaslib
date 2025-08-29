using BMW.Rheingold.Psdz.Model.Tal;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [DataContract]
    public class PsdzEcuPollTa : PsdzTa
    {
        [DataMember]
        public long EstimatedExecutionTime { get; set; }
    }
}
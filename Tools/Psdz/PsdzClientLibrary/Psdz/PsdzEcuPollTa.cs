using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzEcuPollTa : PsdzTa
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public long EstimatedExecutionTime { get; set; }
    }
}
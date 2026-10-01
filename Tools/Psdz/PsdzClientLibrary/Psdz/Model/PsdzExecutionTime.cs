using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzExecutionTime : IPsdzExecutionTime
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public long ActualEndTime { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public long ActualStartTime { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public long PlannedEndTime { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public long PlannedStartTime { get; set; }
    }
}
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [DataContract]
    public class PsdzExecutionTime : IPsdzExecutionTime
    {
        [DataMember]
        public long ActualEndTime { get; set; }

        [DataMember]
        public long ActualStartTime { get; set; }

        [DataMember]
        public long PlannedEndTime { get; set; }

        [DataMember]
        public long PlannedStartTime { get; set; }
    }
}
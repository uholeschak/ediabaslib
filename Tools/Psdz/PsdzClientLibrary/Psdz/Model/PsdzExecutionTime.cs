using PsdzClient;
using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal
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
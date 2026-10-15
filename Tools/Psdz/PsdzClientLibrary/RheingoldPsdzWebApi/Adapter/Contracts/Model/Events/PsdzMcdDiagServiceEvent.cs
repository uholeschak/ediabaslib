using PsdzClient;
using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Localization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Events
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzMcdDiagServiceEvent : PsdzEvent, IPsdzMcdDiagServiceEvent, IPsdzEvent, ILocalizableMessage
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int ErrorId { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string ErrorName { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string JobName { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string LinkName { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string ServiceName { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string ResponseType { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsTimingEvent { get; set; }
    }
}

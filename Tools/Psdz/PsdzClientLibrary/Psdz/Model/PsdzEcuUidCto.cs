using PsdzClient;
using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.SecurityManagement;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecurityManagement
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzEcuUidCto : IPsdzEcuUidCto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string EcuUid { get; set; }
    }
}

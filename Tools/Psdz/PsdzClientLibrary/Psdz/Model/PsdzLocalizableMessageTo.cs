using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Localization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa.LocalizableMessageTo;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    [DataContract]
    public class PsdzLocalizableMessageTo : ILocalizableMessageTo, ILocalizableMessage
    {
        [DataMember]
        public int MessageId { get; set; }

        [DataMember]
        public string Description { get; set; }
    }
}

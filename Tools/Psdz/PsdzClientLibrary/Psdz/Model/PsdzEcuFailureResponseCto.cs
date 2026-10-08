using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa.LocalizableMessageTo;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    [DataContract]
    [KnownType(typeof(PsdzEcuIdentifier))]
    [KnownType(typeof(PsdzLocalizableMessageTo))]
    public class PsdzEcuFailureResponseCto : IPsdzEcuFailureResponseCto
    {
        [DataMember]
        public IPsdzEcuIdentifier EcuIdentifierCto { get; set; }

        [DataMember]
        public ILocalizableMessageTo Cause { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Sfa;
using BMW.Rheingold.Psdz.Model.Sfa.LocalizableMessageTo;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

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
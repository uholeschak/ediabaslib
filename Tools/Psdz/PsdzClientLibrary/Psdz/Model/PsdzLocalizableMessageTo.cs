using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
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

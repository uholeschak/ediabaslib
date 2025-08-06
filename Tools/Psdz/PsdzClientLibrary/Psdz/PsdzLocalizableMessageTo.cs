using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Localization;
using BMW.Rheingold.Psdz.Model.Sfa.LocalizableMessageTo;

namespace BMW.Rheingold.Psdz.Model.Sfa
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

using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BMW.Rheingold.Psdz.Model.Swt;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Swt;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Swt
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzSwtEcu))]
    public class PsdzSwtAction : IPsdzSwtAction
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IEnumerable<IPsdzSwtEcu> SwtEcus { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [KnownType(typeof(PsdzSwDeployTa))]
    [KnownType(typeof(PsdzFscDeployTa))]
    [KnownType(typeof(PsdzSFADeleteTA))]
    [KnownType(typeof(PsdzSFAVerifyTA))]
    [KnownType(typeof(PsdzIdBackupLightTa))]
    [KnownType(typeof(PsdzIdRestoreTa))]
    [KnownType(typeof(PsdzBlFlashTa))]
    [KnownType(typeof(PsdzIdRestoreLightTa))]
    [KnownType(typeof(PsdzIbaDeployTa))]
    [KnownType(typeof(PsdzTa))]
    [KnownType(typeof(PsdzSFAWriteTA))]
    [DataContract]
    public class PsdzTaCategory : IPsdzTaCategory
    {
        [DataMember]
        public bool IsEmpty { get; set; }

        [DataMember]
        public IEnumerable<IPsdzTa> Tas { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    [KnownType(typeof(PsdzEcu))]
    [DataContract]
    public class PsdzStandardSvt : IPsdzStandardSvt
    {
        [DataMember]
        public string AsString { get; set; }

        [DataMember]
        public IEnumerable<IPsdzEcu> Ecus { get; set; }

        [DataMember]
        public byte[] HoSignature { get; set; }

        [DataMember]
        public DateTime HoSignatureDate { get; set; }

        [DataMember]
        public int Version { get; set; }

        public override bool Equals(object obj)
        {
            return PsdzStandardSvt.PsdzStandardSvtComparerInstance.Equals(this, obj as PsdzStandardSvt);
        }

        public override int GetHashCode()
        {
            return PsdzStandardSvt.PsdzStandardSvtComparerInstance.GetHashCode(this);
        }

        private static readonly IEqualityComparer<PsdzStandardSvt> PsdzStandardSvtComparerInstance = new PsdzStandardSvtComparer();
    }
}

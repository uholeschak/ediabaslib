using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    [PreserveSource(KeepAttribute = true)]
    [DataContract]
    [KnownType(typeof(PsdzEcu))]
    [KnownType(typeof(PsdzSmartActuatorMasterEcu))]
    [KnownType(typeof(PsdzSmartActuatorEcu))]
    public class PsdzStandardSvt : IPsdzStandardSvt
    {
        private static readonly IEqualityComparer<PsdzStandardSvt> PsdzStandardSvtComparerInstance = new PsdzStandardSvtComparer();

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string AsString { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IEnumerable<IPsdzEcu> Ecus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public byte[] HoSignature { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public DateTime HoSignatureDate { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int Version { get; set; }

        public override bool Equals(object obj)
        {
            return PsdzStandardSvtComparerInstance.Equals(this, obj as PsdzStandardSvt);
        }

        public override int GetHashCode()
        {
            return PsdzStandardSvtComparerInstance.GetHashCode(this);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BMW.Rheingold.Psdz.Model.Comparer;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz
{
    // [UH] keep attributes for compatibility with existing serialized data
    [DataContract]
    [KnownType(typeof(PsdzEcu))]
    [KnownType(typeof(PsdzSmartActuatorMasterEcu))]
    [KnownType(typeof(PsdzSmartActuatorEcu))]
    public class PsdzStandardSvt : IPsdzStandardSvt
    {
        private static readonly IEqualityComparer<PsdzStandardSvt> PsdzStandardSvtComparerInstance = new PsdzStandardSvtComparer();

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
            return PsdzStandardSvtComparerInstance.Equals(this, obj as PsdzStandardSvt);
        }

        public override int GetHashCode()
        {
            return PsdzStandardSvtComparerInstance.GetHashCode(this);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Ecu
{
    [DataContract]
    public class PsdzEcuStatusInfo : IPsdzEcuStatusInfo
    {
        [DataMember]
        public byte ByteValue { get; set; }

        [DataMember]
        public bool HasIndividualData { get; set; }

        public override bool Equals(object obj)
        {
            PsdzEcuStatusInfo psdzEcuStatusInfo = obj as PsdzEcuStatusInfo;
            return psdzEcuStatusInfo != null && this.ByteValue == psdzEcuStatusInfo.ByteValue;
        }

        public override int GetHashCode()
        {
            return this.ByteValue.GetHashCode();
        }
    }
}

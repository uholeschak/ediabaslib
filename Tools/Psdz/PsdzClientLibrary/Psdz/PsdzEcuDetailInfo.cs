using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Ecu
{
    [DataContract]
    public class PsdzEcuDetailInfo : IPsdzEcuDetailInfo
    {
        [DataMember]
        public byte ByteValue { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is PsdzEcuDetailInfo psdzEcuDetailInfo)
            {
                return ByteValue == psdzEcuDetailInfo.ByteValue;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ByteValue.GetHashCode();
        }
    }
}

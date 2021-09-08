using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [DataContract]
    public class PsdzEcuDetailInfo : IPsdzEcuDetailInfo
    {
        [DataMember]
        public byte ByteValue { get; set; }

        public override bool Equals(object obj)
        {
            PsdzEcuDetailInfo psdzEcuDetailInfo = obj as PsdzEcuDetailInfo;
            return psdzEcuDetailInfo != null && this.ByteValue == psdzEcuDetailInfo.ByteValue;
        }

        public override int GetHashCode()
        {
            return this.ByteValue.GetHashCode();
        }
    }
}

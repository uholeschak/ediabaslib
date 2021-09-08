using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [DataContract]
    public class PsdzDiagAddress : IPsdzDiagAddress
    {
        [DataMember]
        public int Offset { get; set; }

        public override bool Equals(object obj)
        {
            PsdzDiagAddress psdzDiagAddress = obj as PsdzDiagAddress;
            return psdzDiagAddress != null && this.Offset == psdzDiagAddress.Offset;
        }

        public override int GetHashCode()
        {
            return this.Offset.GetHashCode();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Ecu
{
    [DataContract]
    public class PsdzDiagAddress : IPsdzDiagAddress
    {
        [DataMember]
        public int Offset { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is PsdzDiagAddress psdzDiagAddress)
            {
                return Offset == psdzDiagAddress.Offset;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Offset.GetHashCode();
        }
    }
}

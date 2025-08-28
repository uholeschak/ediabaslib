using System.Runtime.Serialization;

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

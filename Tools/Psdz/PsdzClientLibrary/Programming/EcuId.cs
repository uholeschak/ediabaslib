using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    [DataContract]
    internal class EcuId : IEcuIdentifier
    {
        [DataMember]
        public string BaseVariant { get; private set; }

        [DataMember]
        public int DiagAddrAsInt { get; private set; }

        public EcuId(string baseVariant, int diagAddrAsInt)
        {
            if (baseVariant == null)
            {
                throw new ArgumentNullException("baseVariant");
            }

            BaseVariant = baseVariant;
            DiagAddrAsInt = diagAddrAsInt;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}_0x{1:X2}", BaseVariant, DiagAddrAsInt);
        }
    }
}
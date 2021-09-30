using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public class EcuId : IEcuIdentifier
    {
        public EcuId(string baseVariant, int diagAddrAsInt)
        {
            if (baseVariant == null)
            {
                throw new ArgumentNullException("baseVariant");
            }
            this.BaseVariant = baseVariant;
            this.DiagAddrAsInt = diagAddrAsInt;
        }

        public string BaseVariant { get; private set; }

        public int DiagAddrAsInt { get; private set; }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}_0x{1:X2}", this.BaseVariant, this.DiagAddrAsInt);
        }
    }
}

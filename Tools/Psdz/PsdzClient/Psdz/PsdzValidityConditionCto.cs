using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public class PsdzValidityConditionCto : IPsdzValidityConditionCto
    {
        public PsdzConditionTypeEtoEnum ConditionType { get; set; }

        public string ValidityValue { get; set; }
    }
}

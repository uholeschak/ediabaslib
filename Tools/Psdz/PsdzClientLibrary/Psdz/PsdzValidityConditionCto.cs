using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    public class PsdzValidityConditionCto : IPsdzValidityConditionCto
    {
        public PsdzConditionTypeEtoEnum ConditionType { get; set; }

        public string ValidityValue { get; set; }
    }
}

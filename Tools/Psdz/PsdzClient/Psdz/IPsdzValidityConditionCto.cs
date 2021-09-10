using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    public interface IPsdzValidityConditionCto
    {
        PsdzConditionTypeEtoEnum ConditionType { get; set; }

        string ValidityValue { get; set; }
    }
}

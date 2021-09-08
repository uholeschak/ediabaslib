using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public interface IPsdzValidityConditionCto
    {
        PsdzConditionTypeEtoEnum ConditionType { get; set; }

        string ValidityValue { get; set; }
    }
}

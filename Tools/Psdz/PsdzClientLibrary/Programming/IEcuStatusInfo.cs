using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    public interface IEcuStatusInfo
    {
        byte Value { get; }

        bool HasIndividualData { get; }
    }
}

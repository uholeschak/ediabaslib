using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public interface IEcuFailureResponse
    {
        IEcuIdentifier Ecu { get; }

        string Reason { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using PsdzClient.Programming;

namespace BMW.Rheingold.Programming.API
{
    internal class EcuFailureResponse : IEcuFailureResponse
    {
        public IEcuIdentifier Ecu { get; internal set; }

        public string Reason { get; internal set; }
    }
}

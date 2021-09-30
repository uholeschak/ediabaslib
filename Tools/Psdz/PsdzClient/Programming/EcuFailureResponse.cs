using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public class EcuFailureResponse : IEcuFailureResponse
    {
        public IEcuIdentifier Ecu { get; internal set; }

        public string Reason { get; internal set; }
    }
}

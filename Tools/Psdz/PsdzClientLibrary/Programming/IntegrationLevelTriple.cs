using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Programming;

namespace BMW.Rheingold.Programming.API
{
    internal class IntegrationLevelTriple : IIstufenTriple
    {
        public string Current { get; private set; }
        public string Last { get; private set; }
        public string Shipment { get; private set; }

        public IntegrationLevelTriple(string shipment, string last, string current)
        {
            Shipment = shipment;
            Last = last;
            Current = current;
        }
    }
}
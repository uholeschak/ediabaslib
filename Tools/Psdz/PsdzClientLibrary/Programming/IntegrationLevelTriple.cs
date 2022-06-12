using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public class IntegrationLevelTriple : IIstufenTriple
    {
        public IntegrationLevelTriple(string shipment, string last, string current)
        {
            this.Shipment = shipment;
            this.Last = last;
            this.Current = current;
        }

        public string Current { get; private set; }

        public string Last { get; private set; }

        public string Shipment { get; private set; }
    }
}

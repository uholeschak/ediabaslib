using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public class ObdTripleValue : IObdTripleValue
    {
        public string CalId { get; set; }
        public string ObdId { get; set; }
        public string SubCVN { get; set; }

        public ObdTripleValue(string calid, string obdid, string subcvn)
        {
            CalId = calid;
            ObdId = obdid;
            SubCVN = subcvn;
        }
    }
}
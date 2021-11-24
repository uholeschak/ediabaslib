using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public class ObdTripleValue : IObdTripleValue
    {
        public ObdTripleValue(string calid, string obdid, string subcvn)
        {
            this.CalId = calid;
            this.ObdId = obdid;
            this.SubCVN = subcvn;
        }

        public string CalId { get; set; }

        public string ObdId { get; set; }

        public string SubCVN { get; set; }
    }
}

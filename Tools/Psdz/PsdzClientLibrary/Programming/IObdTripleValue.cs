using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public interface IObdTripleValue
    {
        string CalId { get; }

        string ObdId { get; }

        string SubCVN { get; }
    }
}

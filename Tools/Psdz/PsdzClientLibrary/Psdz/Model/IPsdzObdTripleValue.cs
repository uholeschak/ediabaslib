using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Obd
{
    public interface IPsdzObdTripleValue
    {
        string CalId { get; }

        string ObdId { get; }

        string SubCVN { get; }
    }
}

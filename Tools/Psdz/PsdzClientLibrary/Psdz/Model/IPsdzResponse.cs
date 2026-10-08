using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model
{
    public interface IPsdzResponse
    {
        string Cause { get; set; }

        object Result { get; set; }

        bool IsSuccessful { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model
{
    public interface IPsdzFp : IPsdzStandardFp
    {
        string Baureihenverbund { get; }

        string Entwicklungsbaureihe { get; }
    }
}

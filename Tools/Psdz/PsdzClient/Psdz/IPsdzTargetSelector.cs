using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public interface IPsdzTargetSelector
    {
        string Baureihenverbund { get; }

        bool IsDirect { get; }

        string Project { get; }

        string VehicleInfo { get; }
    }
}

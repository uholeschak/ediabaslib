using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    public interface IPsdzFsaTa : IPsdzTa, IPsdzTalElement
    {
        long EstimatedExecutionTime { get; set; }

        long FeatureId { get; set; }
    }
}

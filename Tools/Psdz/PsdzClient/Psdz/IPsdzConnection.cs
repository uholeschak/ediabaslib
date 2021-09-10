using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model
{
    public interface IPsdzConnection
    {
        Guid Id { get; }

        IPsdzTargetSelector TargetSelector { get; }

        int Port { get; }
    }
}

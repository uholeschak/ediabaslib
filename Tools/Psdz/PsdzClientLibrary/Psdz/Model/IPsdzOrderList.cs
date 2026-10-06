using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Svb;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Svb
{
    public interface IPsdzOrderList
    {
        int NumberOfUnits { get; }

        IPsdzEcuVariantInstance[] BntnVariantInstances { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Psdz;

namespace PsdzClient.Programming
{
    class BusEnumMapper : ProgrammingEnumMapperBase<PsdzBus, Bus>
    {
        protected override IDictionary<PsdzBus, Bus> CreateMap()
        {
            return Enum.GetValues(typeof(PsdzBus)).Cast<PsdzBus>().ToDictionary((PsdzBus k) => k, (PsdzBus v) => (Bus)Enum.Parse(typeof(Bus), Enum.GetName(typeof(PsdzBus), v)));
        }
    }
}

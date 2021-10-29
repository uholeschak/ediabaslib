using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PsdzClient.Core.CharacteristicExpression;

namespace PsdzClient
{
    static class ClientContext
    {
        public static PdszDatabase Database { get; set; }

        public static EnumBrand SelectedBrand { get; set; } = EnumBrand.BMWPKW;
    }
}

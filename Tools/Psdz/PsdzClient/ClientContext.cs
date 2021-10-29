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
        static ClientContext()
        {
            Database = null;
            SelectedBrand = EnumBrand.BMWPKW;
            OutletCountry = string.Empty;
        }

        public static PdszDatabase Database { get; set; }

        public static EnumBrand SelectedBrand { get; set; }

        public static string OutletCountry { get; set; }
    }
}

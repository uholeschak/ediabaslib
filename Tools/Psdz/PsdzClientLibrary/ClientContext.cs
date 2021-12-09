using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Core;
using static PsdzClient.Core.CharacteristicExpression;

namespace PsdzClient
{
    public class ClientContext
    {
        private static ClientContext _clientContext;

        ClientContext()
        {
            Database = null;
            SelectedBrand = EnumBrand.BMWBMWiMINI;
            OutletCountry = string.Empty;
            Language = "En";
        }

        public static ClientContext GetClientContext(Vehicle vehicle = null)
        {
            if (_clientContext == null)
            {
                _clientContext = new ClientContext();
            }

            return _clientContext;
        }

        public PdszDatabase Database { get; set; }

        public EnumBrand SelectedBrand { get; set; }

        public string OutletCountry { get; set; }

        public string Language { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using PsdzClient.Core;

namespace PsdzClient
{
    public class ClientContext
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ClientContext));

        public ClientContext()
        {
            Database = null;
            SelectedBrand = CharacteristicExpression.EnumBrand.BMWBMWiMINI;
            OutletCountry = string.Empty;
            Language = "En";
        }

        public static ClientContext GetClientContext(Vehicle vehicle)
        {
            if (vehicle != null)
            {
                if (vehicle.ClientContext == null)
                {
                    log.ErrorFormat("GetClientContext ClientContext is null");
                }

                return vehicle.ClientContext;
            }

            log.ErrorFormat("GetClientContext Vehicle is null");
            return null;
        }

        public PdszDatabase Database { get; set; }

        public CharacteristicExpression.EnumBrand SelectedBrand { get; set; }

        public string OutletCountry { get; set; }

        public string Language { get; set; }
    }
}

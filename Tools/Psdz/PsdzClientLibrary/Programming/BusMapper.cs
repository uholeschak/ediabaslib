using BMW.Rheingold.CoreFramework.Programming.Data.Ecu;
using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient.Core;
using PsdzClient.Programming;
using System;
using System.Linq;

namespace PsdzClient.Programming
{
    public class BusMapper
    {
        public static IBusObject MapToBus(PsdzBus psdzBus)
        {
#if OLD_PSDZ_HOST
            int id = (int)psdzBus;
            string name = Enum.GetName(typeof(PsdzBus), psdzBus) ?? "-";
            return new BusObject(BusObject.ACan.Id + id, name);
#else
            if (psdzBus == null)
            {
                return new BusObject(0, "UNKNOWN");
            }
            ServiceLocator.Current.GetService<IFasta2Service>()?.AddServiceCode(ServiceCodes.MAP01_PsdzValuesAreNotMapped_nu_LF, "PSdZ value '" + psdzBus.Name + "' not mapped", LayoutGroup.X, allowMultipleEntries: false, bufferIfSessionNotStarted: false, null, null);
            return new BusObject(psdzBus.Id, psdzBus.Name);
#endif
        }

        public static PsdzBus MapToPsdzBus(IBusObject bus)
        {
#if OLD_PSDZ_HOST
            if (bus == null)
            {
                return PsdzBus.ACan;
            }

            if (!Enum.TryParse(bus.Name, true, out PsdzBus result))
            {
                return PsdzBus.ACan;
            }
            return result;
#else
            if (bus == null)
            {
                return new PsdzBus(0, "UNKNOWN", pDirectAccess: true);
            }
            return new PsdzBus(bus.Id, bus.Name, bus.DirectAddress);
#endif
        }
    }
}
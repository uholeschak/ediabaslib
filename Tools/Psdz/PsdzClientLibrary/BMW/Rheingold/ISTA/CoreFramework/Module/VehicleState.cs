using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using System.Collections.Generic;
using System.Globalization;
using PsdzClient;

namespace BMW.Rheingold.ISTA.CoreFramework.Module
{
    internal class VehicleState
    {
        private readonly Dictionary<string, IStateLocator> vehicleStateList;
        [PreserveSource(Added = true)]
        private readonly ClientContext clientContext;

        [PreserveSource(Hint = "ClientContext added", SignatureModified = true)]
        public VehicleState(ClientContext clientContext = null)
        {
            vehicleStateList = new Dictionary<string, IStateLocator>();
            //[+] this.clientContext = clientContext;
            this.clientContext = clientContext;
        }

        public IStateLocator GetState(IVehiclePartLocator part)
        {
            if (vehicleStateList.TryGetValue(part.Id, out var value))
            {
                return value;
            }
            return null;
        }

        public IStateLocator GetState(XEP_VEHICLEPART partRef)
        {
            if (vehicleStateList.TryGetValue(partRef.Id.ToString(CultureInfo.InvariantCulture), out var value))
            {
                return value;
            }
            return null;
        }

        public void SetState(XEP_VEHICLEPART vehiclePart, XEP_VEHICLESTATE vehicleState)
        {
            //[-] IStateLocator value = new StateLocator(vehicleState.Id.ToString(CultureInfo.InvariantCulture));
            //[+] IStateLocator value = new StateLocator(vehicleState.Id.ToString(CultureInfo.InvariantCulture), clientContext);
            IStateLocator value = new StateLocator(vehicleState.Id.ToString(CultureInfo.InvariantCulture), clientContext);
            vehicleStateList.Add(vehiclePart.Id.ToString(CultureInfo.InvariantCulture), value);
        }

        public void SetState(IVehiclePartLocator vehiclePart, XEP_VEHICLESTATE vehicleState)
        {
            //[-] IStateLocator value = new StateLocator(vehicleState.Id.ToString(CultureInfo.InvariantCulture));
            //[+] IStateLocator value = new StateLocator(vehicleState.Id.ToString(CultureInfo.InvariantCulture), clientContext);
            IStateLocator value = new StateLocator(vehicleState.Id.ToString(CultureInfo.InvariantCulture), clientContext);
            vehicleStateList.Add(vehiclePart.Id, value);
        }

        public void SetState(IVehiclePartLocator vehiclePart, IVehicleStateLocator vehicleState)
        {
            //[-] IStateLocator value = new StateLocator(vehicleState.Id.ToString(CultureInfo.InvariantCulture));
            //[+] IStateLocator value = new StateLocator(vehicleState.Id.ToString(CultureInfo.InvariantCulture), clientContext);
            IStateLocator value = new StateLocator(vehicleState.Id.ToString(CultureInfo.InvariantCulture), clientContext);
            vehicleStateList.Add(vehiclePart.Id, value);
        }

        private string Localize(XEP_VEHICLESTATE vehicleState)
        {
            switch (ConfigSettings.CurrentUICulture)
            {
                case "de-DE":
                    return vehicleState.Title_dede;
                case "en-GB":
                    return vehicleState.Title_engb;
                case "en-US":
                    return vehicleState.Title_enus;
                case "fr-FR":
                    return vehicleState.Title_fr;
                case "es-ES":
                    return vehicleState.Title_es;
                case "th-TH":
                    return vehicleState.Title_th;
                case "tr-TR":
                    return vehicleState.Title_tr;
                case "el-GR":
                    return vehicleState.Title_el;
                case "ja-JP":
                    return vehicleState.Title_ja;
                case "ru-RU":
                    return vehicleState.Title_ru;
                case "it-IT":
                    return vehicleState.Title_it;
                case "nl-NL":
                    return vehicleState.Title_nl;
                case "cs-CZ":
                    return vehicleState.Title_cscz;
                case "pl-PL":
                    return vehicleState.Title_plpl;
                case "pt-PT":
                    return vehicleState.Title_pt;
                case "sv-SE":
                    return vehicleState.Title_sv;
                case "zh-CN":
                    return vehicleState.Title_zhcn;
                case "zh-TW":
                    return vehicleState.Title_zhtw;
                case "ko-KR":
                    return vehicleState.Title_ko;
                default:
                    Log.Error("VehicleState.localize", "CurrentUICulture {0} not available - language set to ENGB", ConfigSettings.CurrentUICulture);
                    return vehicleState.Title_dede;
            }
        }

        private string Localize(XEP_VEHICLEPART vehiclePart)
        {
            switch (ConfigSettings.CurrentUICulture)
            {
                case "de-DE":
                    return vehiclePart.Title_dede;
                case "en-GB":
                    return vehiclePart.Title_engb;
                case "en-US":
                    return vehiclePart.Title_enus;
                case "fr-FR":
                    return vehiclePart.Title_fr;
                case "es-ES":
                    return vehiclePart.Title_es;
                case "th-TH":
                    return vehiclePart.Title_th;
                case "tr-TR":
                    return vehiclePart.Title_tr;
                case "el-GR":
                    return vehiclePart.Title_el;
                case "ja-JP":
                    return vehiclePart.Title_ja;
                case "ru-RU":
                    return vehiclePart.Title_ru;
                case "it-IT":
                    return vehiclePart.Title_it;
                case "nl-NL":
                    return vehiclePart.Title_nl;
                case "cs-CZ":
                    return vehiclePart.Title_cscz;
                case "pl-PL":
                    return vehiclePart.Title_plpl;
                case "pt-PT":
                    return vehiclePart.Title_pt;
                case "sv-SE":
                    return vehiclePart.Title_sv;
                case "zh-CN":
                    return vehiclePart.Title_zhcn;
                case "zh-TW":
                    return vehiclePart.Title_zhtw;
                case "ko-KR":
                    return vehiclePart.Title_ko;
                default:
                    Log.Error("VehiclePart.localize", "CurrentUICulture {0} not available - language set to ENGB", ConfigSettings.CurrentUICulture);
                    return vehiclePart.Title_dede;
            }
        }
    }
}

using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PsdzClient;
using System;
using System.Globalization;

namespace BMW.Rheingold.ISTA.CoreFramework
{
    public class VehiclePart : IVehiclePartLocator, ISPELocator
    {
        private readonly string[] dataValueNames = new string[22]
        {
        "ID", "TITLEID", "TITLE_DEDE", "TITLE_ENGB", "TITLE_ENUS", "TITLE_FR", "TITLE_TH", "TITLE_SV", "TITLE_IT", "TITLE_ES",
        "TITLE_ID", "TITLE_KO", "TITLE_EL", "TITLE_TR", "TITLE_ZHCN", "TITLE_RU", "TITLE_NL", "TITLE_PT", "TITLE_ZHTW", "TITLE_JA",
        "TITLE_CSCZ", "TITLE_PLPL"
        };

        private readonly Exception exception;

        private readonly XEP_VEHICLEPART part;

        public ISPELocator[] Children => new ISPELocator[0];

        public string DataClassName => "VehiclePart";

        public string[] DataValueNames => dataValueNames;

        public Exception Exception => exception;

        public bool HasException => exception != null;

        public string Id
        {
            get
            {
                if (part != null)
                {
                    return part.Id.ToString(CultureInfo.InvariantCulture);
                }
                return "0";
            }
        }

        public string[] IncomingLinkNames => new string[0];

        public string[] OutgoingLinkNames => new string[0];

        public ISPELocator[] Parents => new ISPELocator[0];

        public decimal SignedId
        {
            get
            {
                if (part == null)
                {
                    return -1m;
                }
                return part.Id;
            }
        }

        public string Title
        {
            get
            {
                if (part == null)
                {
                    return null;
                }
                return part.Title;
            }
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public VehiclePart(XEP_VEHICLEPART part)
        {
            this.part = part;
        }

        [PreserveSource(Hint = "clientContext added", SignatureModified = true)]
        public VehiclePart(string partValueId, ClientContext clientContext)
        {
            try
            {
                decimal id = Convert.ToDecimal(partValueId);
                //[-] part = DatabaseProviderFactory.Instance.GetXepVehiclePartById(id);
                //[+] part = clientContext.Database.GetXepVehiclePartById(id);
                part = clientContext.Database.GetXepVehiclePartById(id);
            }
            catch (Exception ex)
            {
                Log.WarningException("VehiclePart.VehiclePart()", ex);
                exception = ex;
            }
        }

        public string GetDataValue(string name)
        {
            if (part != null && !string.IsNullOrEmpty(name))
            {
                switch (name.ToUpperInvariant())
                {
                    case "ID":
                        return Id;
                    case "TITLEID":
                        break;
                    case "TITLE":
                        return part.Title;
                    case "TITLE_DEDE":
                        return part.Title_dede;
                    case "TITLE_ENGB":
                        return part.Title_engb;
                    case "TITLE_ENUS":
                        return part.Title_enus;
                    case "TITLE_FR":
                        return part.Title_fr;
                    case "TITLE_TH":
                        return part.Title_th;
                    case "TITLE_SV":
                        return part.Title_sv;
                    case "TITLE_IT":
                        return part.Title_it;
                    case "TITLE_ES":
                        return part.Title_es;
                    case "TITLE_ID":
                        return part.Title_id;
                    case "TITLE_KO":
                        return part.Title_ko;
                    case "TITLE_EL":
                        return part.Title_el;
                    case "TITLE_TR":
                        return part.Title_tr;
                    case "TITLE_ZHCN":
                        return part.Title_zhcn;
                    case "TITLE_RU":
                        return part.Title_ru;
                    case "TITLE_NL":
                        return part.Title_nl;
                    case "TITLE_PT":
                        return part.Title_pt;
                    case "TITLE_ZHTW":
                        return part.Title_zhtw;
                    case "TITLE_JA":
                        return part.Title_ja;
                    case "TITLE_CSCZ":
                        return part.Title_cscz;
                    case "TITLE_PLPL":
                        return part.Title_plpl;
                    default:
                        Log.Error("VehiclePart.GetDataValue()", "No such name as '{0}' available! Returnning null!", name);
                        return null;
                }
                if (part.TitleId.HasValue)
                {
                    return part.TitleId.ToString();
                }
            }
            return null;
        }

        public T GetDataValue<T>(string name)
        {
            return default(T);
        }

        public ISPELocator[] GetIncomingLinks()
        {
            throw new NotImplementedException();
        }

        public ISPELocator[] GetIncomingLinks(string incomingLinkName)
        {
            throw new NotImplementedException();
        }

        public ISPELocator[] GetOutgoingLinks()
        {
            throw new NotImplementedException();
        }

        public ISPELocator[] GetOutgoingLinks(string outgoingLinkName)
        {
            throw new NotImplementedException();
        }
    }
}

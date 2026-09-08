using PsdzClient.Core;
using System;
using System.Globalization;
using PsdzClient;
using PsdzClientLibrary;

#pragma warning disable CS0649
namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class EquipmentLocator : IEquipmentLocator, ISPELocator
    {
        private readonly XEP_EQUIPMENT equipment;

        private readonly ISPELocator[] children;

        private readonly ISPELocator[] parents;

        public ISPELocator[] Children => children;

        public string Id => equipment.ID.ToString(CultureInfo.InvariantCulture);

        public ISPELocator[] Parents => parents;

        public string DataClassName => "Equipment";

        public string[] OutgoingLinkNames => new string[0];

        public string[] IncomingLinkNames => new string[0];

        public string[] DataValueNames => new string[26]
        {
        "ID", "TITLEID", "TITLE_DEDE", "TITLE_ENGB", "TITLE_ENUS", "TITLE_FR", "TITLE_TH", "TITLE_SV", "TITLE_IT", "TITLE_ES",
        "TITLE_ID", "TITLE_KO", "TITLE_EL", "TITLE_TR", "TITLE_ZHCN", "TITLE_RU", "TITLE_NL", "TITLE_PT", "TITLE_ZHTW", "TITLE_JA",
        "TITLE_CSCZ", "TITLE_PLPL", "NAME", "SICHERHEITSRELEVANT", "VALIDTO", "VALIDFROM"
        };

        public decimal SignedId
        {
            get
            {
                if (equipment == null)
                {
                    return -1m;
                }
                return equipment.ID;
            }
        }

        public Exception Exception => null;

        public bool HasException => false;

        public string Title
        {
            get
            {
                if (equipment == null)
                {
                    return null;
                }
                return equipment.TITLE;
            }
        }

        public string Name
        {
            get
            {
                if (equipment == null)
                {
                    return null;
                }
                return equipment.NAME;
            }
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public EquipmentLocator(XEP_EQUIPMENT equipment)
        {
            this.equipment = equipment;
            children = new ISPELocator[0];
        }

        [PreserveSource(Hint = "Vehicle added", SignatureModified = true)]
        public EquipmentLocator(decimal id, Vehicle vehicle)
        {
            //[-] equipment = DatabaseProviderFactory.Instance.GetEquipmentById(id);
            //[+] equipment = XepConverter.Convert(ClientContext.GetDatabase(vehicle)?.GetEquipmentById(id.ToString(CultureInfo.InvariantCulture)));
            equipment = XepConverter.Convert(ClientContext.GetDatabase(vehicle)?.GetEquipmentById(id.ToString(CultureInfo.InvariantCulture)));
            children = new ISPELocator[0];
        }

        public string GetDataValue(string name)
        {
            if (equipment == null || string.IsNullOrEmpty(name))
            {
                return null;
            }
            switch (name.ToUpperInvariant())
            {
                case "ID":
                    return equipment.ID.ToString(CultureInfo.InvariantCulture);
                case "NODECLASS":
                    return "4675330";
                case "TITLE_DEDE":
                    return equipment.TITLE_DEDE;
                case "TITLE_ENGB":
                    return equipment.TITLE_ENGB;
                case "TITLE_ENUS":
                    return equipment.TITLE_ENUS;
                case "TITLE_FR":
                    return equipment.TITLE_FR;
                case "TITLE_TH":
                    return equipment.TITLE_TH;
                case "TITLE_SV":
                    return equipment.TITLE_SV;
                case "TITLE_IT":
                    return equipment.TITLE_IT;
                case "TITLE_ES":
                    return equipment.TITLE_ES;
                case "TITLE_ID":
                    return equipment.TITLE_ID;
                case "TITLE_KO":
                    return equipment.TITLE_KO;
                case "TITLE_EL":
                    return equipment.TITLE_EL;
                case "TITLE_TR":
                    return equipment.TITLE_TR;
                case "TITLE_ZHCN":
                    return equipment.TITLE_ZHCN;
                case "TITLE_RU":
                    return equipment.TITLE_RU;
                case "TITLE_NL":
                    return equipment.TITLE_NL;
                case "TITLE_PT":
                    return equipment.TITLE_PT;
                case "TITLE_ZHTW":
                    return equipment.TITLE_ZHTW;
                case "TITLE_JA":
                    return equipment.TITLE_JA;
                case "TITLE_CSCZ":
                    return equipment.TITLE_CSCZ;
                case "TITLE_PLPL":
                    return equipment.TITLE_PLPL;
                case "NAME":
                    return equipment.NAME;
                case "VALIDFROM":
                    return equipment.VALIDFROM.ToString(CultureInfo.InvariantCulture);
                case "VALIDTO":
                    return equipment.VALIDTO.ToString(CultureInfo.InvariantCulture);
                case "SICHERHEITSRELEVANT":
                    return equipment.SICHERHEITSRELEVANT.ToString(CultureInfo.InvariantCulture);
                default:
                    return string.Empty;
            }
        }

        public ISPELocator[] GetIncomingLinks()
        {
            return new ISPELocator[0];
        }

        public ISPELocator[] GetIncomingLinks(string incomingLinkName)
        {
            return parents;
        }

        public ISPELocator[] GetOutgoingLinks()
        {
            return children;
        }

        public ISPELocator[] GetOutgoingLinks(string outgoingLinkName)
        {
            return children;
        }

        public T GetDataValue<T>(string name)
        {
            throw new NotImplementedException();
        }
    }
}

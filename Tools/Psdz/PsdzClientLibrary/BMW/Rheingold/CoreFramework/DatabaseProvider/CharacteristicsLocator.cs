using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces;
using BmwFileReader;
using PsdzClient;
using System;
using System.Globalization;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class CharacteristicsLocator : ICharacteristicsLocator, ISPELocator
    {
        private readonly IXepCharacteristics characteristicsContainer;
        private readonly ISPELocator[] children;
        private readonly ISPELocator[] parents;
        public ISPELocator[] Children => children;
        public string Id => characteristicsContainer.Id.ToString(CultureInfo.InvariantCulture);
        public ISPELocator[] Parents => parents;

        public string DataClassName
        {
            get
            {
                if (characteristicsContainer.Nodeclass.HasValue)
                {
                    //[-] return DatabaseProviderFactory.Instance.GetXepNodeClassNameById(characteristicsContainer.Nodeclass.Value);
                    //[+] return clientContext?.Database?.GetXepNodeClassNameById(characteristicsContainer.Nodeclass.Value.ToString(CultureInfo.InvariantCulture));
                    return clientContext?.Database?.GetXepNodeClassNameById(characteristicsContainer.Nodeclass.Value.ToString(CultureInfo.InvariantCulture));
                }

                return string.Empty;
            }
        }

        public string[] OutgoingLinkNames => new string[0];
        public string[] IncomingLinkNames => new string[0];
        public string[] DataValueNames => new string[28]
        {
            "ID",
            "NODECLASS",
            "TITLEID",
            "TITLE_DEDE",
            "TITLE_ENGB",
            "TITLE_ENUS",
            "TITLE_FR",
            "TITLE_TH",
            "TITLE_SV",
            "TITLE_IT",
            "TITLE_ES",
            "TITLE_ID",
            "TITLE_KO",
            "TITLE_EL",
            "TITLE_TR",
            "TITLE_ZHCN",
            "TITLE_RU",
            "TITLE_NL",
            "TITLE_PT",
            "TITLE_ZHTW",
            "TITLE_JA",
            "TITLE_CSCZ",
            "TITLE_PLPL",
            "STATICCLASSVARIABLES",
            "STATICCLASSVARIABLESMOTORRAD",
            "PARENTID",
            "NAME",
            "LEGACY_NAME"
        };

        public decimal SignedId
        {
            get
            {
                if (characteristicsContainer == null)
                {
                    return -1m;
                }

                return characteristicsContainer.Id;
            }
        }

        public Exception Exception => null;
        public bool HasException => false;

        public decimal ParentId
        {
            get
            {
                if (characteristicsContainer == null)
                {
                    return -1m;
                }

                return characteristicsContainer.ParentId.Value;
            }
        }

        public string Title
        {
            get
            {
                if (characteristicsContainer == null)
                {
                    return null;
                }

                return characteristicsContainer.Title;
            }
        }

        public string Title_dede
        {
            get
            {
                if (characteristicsContainer == null)
                {
                    return null;
                }

                return characteristicsContainer.Title_dede;
            }
        }

        public string Name
        {
            get
            {
                if (characteristicsContainer == null)
                {
                    return null;
                }

                return characteristicsContainer.Name;
            }
        }

        [PreserveSource(Hint = "clientContext added", SignatureModified = true)]
        public CharacteristicsLocator(IXepCharacteristics characteristicsContainer, ClientContext clientContext)
        {
            this.characteristicsContainer = characteristicsContainer;
            //[+] this.clientContext = clientContext;
            this.clientContext = clientContext;
            children = new ISPELocator[0];
            parents = new ISPELocator[0];
        }

        public string GetDataValue(string name)
        {
            if (characteristicsContainer == null || string.IsNullOrEmpty(name))
            {
                return null;
            }

            switch (name.ToUpperInvariant())
            {
                case "ID":
                    return characteristicsContainer.Id.ToString(CultureInfo.InvariantCulture);
                case "NODECLASS":
                    if (!characteristicsContainer.Nodeclass.HasValue)
                    {
                        return "0";
                    }
                    return characteristicsContainer.Nodeclass.ToString();
                case "TITLEID":
                    if (!characteristicsContainer.TitleId.HasValue)
                    {
                        return "0";
                    }
                    return characteristicsContainer.TitleId.ToString();
                case "TITLE_DEDE":
                    return characteristicsContainer.Title_dede;
                case "TITLE_ENGB":
                    return characteristicsContainer.Title_engb;
                case "TITLE_ENUS":
                    return characteristicsContainer.Title_enus;
                case "TITLE_FR":
                    return characteristicsContainer.Title_fr;
                case "TITLE_TH":
                    return characteristicsContainer.Title_th;
                case "TITLE_SV":
                    return characteristicsContainer.Title_sv;
                case "TITLE_IT":
                    return characteristicsContainer.Title_it;
                case "TITLE_ES":
                    return characteristicsContainer.Title_es;
                case "TITLE_ID":
                    return characteristicsContainer.Title_id;
                case "TITLE_KO":
                    return characteristicsContainer.Title_ko;
                case "TITLE_EL":
                    return characteristicsContainer.Title_el;
                case "TITLE_TR":
                    return characteristicsContainer.Title_tr;
                case "TITLE_ZHCN":
                    return characteristicsContainer.Title_zhcn;
                case "TITLE_RU":
                    return characteristicsContainer.Title_ru;
                case "TITLE_NL":
                    return characteristicsContainer.Title_nl;
                case "TITLE_PT":
                    return characteristicsContainer.Title_pt;
                case "TITLE_ZHTW":
                    return characteristicsContainer.Title_zhtw;
                case "TITLE_JA":
                    return characteristicsContainer.Title_ja;
                case "TITLE_CSCZ":
                    return characteristicsContainer.Title_cscz;
                case "TITLE_PLPL":
                    return characteristicsContainer.Title_plpl;
                case "STATICCLASSVARIABLES":
                    if (!characteristicsContainer.StaticClassVariables.HasValue)
                    {
                        return "0";
                    }

                    return characteristicsContainer.StaticClassVariables.ToString();
                case "STATICCLASSVARIABLESMOTORRAD":
                    if (!characteristicsContainer.StaticClassVariablesMotorrad.HasValue)
                    {
                        return "0";
                    }

                    return characteristicsContainer.StaticClassVariablesMotorrad.ToString();
                case "PARENTID":
                    if (!characteristicsContainer.ParentId.HasValue)
                    {
                        return "0";
                    }

                    return characteristicsContainer.ParentId.ToString();
                case "TITLE":
                    return characteristicsContainer.Title;
                case "NAME":
                    return characteristicsContainer.Name;
                case "LEGACY_NAME":
                    return characteristicsContainer.LegacyName;
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

        [PreserveSource(Added = true)]
        private readonly ClientContext clientContext;
    }
}
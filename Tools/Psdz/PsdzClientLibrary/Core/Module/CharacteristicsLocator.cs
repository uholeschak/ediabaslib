using PsdzClient.Core;
using System;
using System.Globalization;
using BmwFileReader;
using PsdzClient;

namespace BMW.Rheingold.CoreFramework.DatabaseProvide
{
    public class CharacteristicsLocator : ICharacteristicsLocator, ISPELocator
    {
        [PreserveSource(Hint = "IXepCharacteristics", Placeholder = true)]
        private readonly PsdzDatabase.Characteristics characteristicsContainer;

        [PreserveSource(Added = true)]
        private readonly ClientContext clientContext;

        private readonly ISPELocator[] children;

        private readonly ISPELocator[] parents;

        public ISPELocator[] Children => children;

        public string Id => characteristicsContainer.Id.ToString(CultureInfo.InvariantCulture);

        public ISPELocator[] Parents => parents;

        public string DataClassName
        {
            get
            {
                //[-] if (characteristicsContainer.Nodeclass.HasValue)
                //[+] if (!string.IsNullOrEmpty(characteristicsContainer.NodeClass))
                if (!string.IsNullOrEmpty(characteristicsContainer.NodeClass))
                {
                    //[-] return DatabaseProviderFactory.Instance.GetXepNodeClassNameById(characteristicsContainer.Nodeclass.Value);
                    //[+] return clientContext?.Database?.GetNodeClassNameById(characteristicsContainer.NodeClass);
                    return clientContext?.Database?.GetNodeClassNameById(characteristicsContainer.NodeClass);
                }
                return string.Empty;
            }
        }

        public string[] OutgoingLinkNames => new string[0];

        public string[] IncomingLinkNames => new string[0];

        public string[] DataValueNames => new string[28]
        {
        "ID", "NODECLASS", "TITLEID", "TITLE_DEDE", "TITLE_ENGB", "TITLE_ENUS", "TITLE_FR", "TITLE_TH", "TITLE_SV", "TITLE_IT",
        "TITLE_ES", "TITLE_ID", "TITLE_KO", "TITLE_EL", "TITLE_TR", "TITLE_ZHCN", "TITLE_RU", "TITLE_NL", "TITLE_PT", "TITLE_ZHTW",
        "TITLE_JA", "TITLE_CSCZ", "TITLE_PLPL", "STATICCLASSVARIABLES", "STATICCLASSVARIABLESMOTORRAD", "PARENTID", "NAME", "LEGACY_NAME"
        };

        public decimal SignedId
        {
            get
            {
                if (characteristicsContainer == null)
                {
                    return -1m;
                }
                //[-] return characteristicsContainer.Id;
                //[+] return characteristicsContainer.Id.ConvertToInt();
                return characteristicsContainer.Id.ConvertToInt();
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
                //[-] return characteristicsContainer.ParentId.Value;
                //[+] return characteristicsContainer.ParentId.ConvertToInt();
                return characteristicsContainer.ParentId.ConvertToInt();
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
                //[-] return characteristicsContainer.Title;
                //[+] return characteristicsContainer.GetTitleTranslated(clientContext.Language);
                return characteristicsContainer.GetTitleTranslated(clientContext.Language);
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
                //[-] return characteristicsContainer.Title_dede;
                //[+] return characteristicsContainer.EcuTranslation.TextDe;
                return characteristicsContainer.EcuTranslation.TextDe;
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

        [PreserveSource(Hint = "IXepCharacteristics replaced", SignatureModified = true)]
        public CharacteristicsLocator(PsdzDatabase.Characteristics characteristicsContainer, ClientContext clientContext)
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
                    //[-] if (!characteristicsContainer.Nodeclass.HasValue)
                    if (string.IsNullOrEmpty(characteristicsContainer.NodeClass))
                    {
                        return "0";
                    }
                    //[-] return characteristicsContainer.Nodeclass.ToString();
                    //[+]return characteristicsContainer.NodeClass;
                    return characteristicsContainer.NodeClass;
                case "TITLEID":
                    //[-] if (!characteristicsContainer.TitleId.HasValue)
                    if (string.IsNullOrEmpty(characteristicsContainer.TitleId))
                    {
                        return "0";
                    }
                    return characteristicsContainer.TitleId.ToString();
                case "TITLE_DEDE":
                    //[-] return characteristicsContainer.Title_dede;
                    //[+] return characteristicsContainer.EcuTranslation.TextDe;
                    return characteristicsContainer.EcuTranslation.TextDe;
                case "TITLE_ENGB":
                    //[-] return characteristicsContainer.Title_engb;
                    //[+] return characteristicsContainer.EcuTranslation.TextEn;
                    return characteristicsContainer.EcuTranslation.TextEn;
                case "TITLE_ENUS":
                    //[-] return characteristicsContainer.Title_enus;
                    //[+] return characteristicsContainer.EcuTranslation.TextUs;
                    return characteristicsContainer.EcuTranslation.TextUs;
                case "TITLE_FR":
                    //[-] return characteristicsContainer.Title_fr;
                    //[+] return characteristicsContainer.EcuTranslation.TextFr;
                    return characteristicsContainer.EcuTranslation.TextFr;
                case "TITLE_TH":
                    //[-] return characteristicsContainer.Title_th;
                    //[+] return characteristicsContainer.EcuTranslation.TextTh;
                    return characteristicsContainer.EcuTranslation.TextTh;
                case "TITLE_SV":
                    //[-] return characteristicsContainer.Title_sv;
                    //[+] return characteristicsContainer.EcuTranslation.TextSv;
                    return characteristicsContainer.EcuTranslation.TextSv;
                case "TITLE_IT":
                    //[-] return characteristicsContainer.Title_it;
                    //[+] return characteristicsContainer.EcuTranslation.TextIt;
                    return characteristicsContainer.EcuTranslation.TextIt;
                case "TITLE_ES":
                    //[-] return characteristicsContainer.Title_es;
                    //[+] return characteristicsContainer.EcuTranslation.TextEs;
                    return characteristicsContainer.EcuTranslation.TextEs;
                case "TITLE_ID":
                    //[-] return characteristicsContainer.Title_id;
                    //[+] return characteristicsContainer.EcuTranslation.TextId;
                    return characteristicsContainer.EcuTranslation.TextId;
                case "TITLE_KO":
                    //[-] return characteristicsContainer.Title_ko;
                    //[+] return characteristicsContainer.EcuTranslation.TextKo;
                    return characteristicsContainer.EcuTranslation.TextKo;
                case "TITLE_EL":
                    //[-] return characteristicsContainer.Title_el;
                    //[+] return characteristicsContainer.EcuTranslation.TextEl;
                    return characteristicsContainer.EcuTranslation.TextEl;
                case "TITLE_TR":
                    //[-] return characteristicsContainer.Title_tr;
                    //[+] return characteristicsContainer.EcuTranslation.TextTr;
                    return characteristicsContainer.EcuTranslation.TextTr;
                case "TITLE_ZHCN":
                    //[-] return characteristicsContainer.Title_zhcn;
                    //[+] return characteristicsContainer.EcuTranslation.TextZh;
                    return characteristicsContainer.EcuTranslation.TextZh;
                case "TITLE_RU":
                    //[-] return characteristicsContainer.Title_ru;
                    //[+] return characteristicsContainer.EcuTranslation.TextRu;
                    return characteristicsContainer.EcuTranslation.TextRu;
                case "TITLE_NL":
                    //[-] return characteristicsContainer.Title_nl;
                    //[+] return characteristicsContainer.EcuTranslation.TextNl;
                    return characteristicsContainer.EcuTranslation.TextNl;
                case "TITLE_PT":
                    //[-] return characteristicsContainer.Title_pt;
                    //[+] return characteristicsContainer.EcuTranslation.TextPt;
                    return characteristicsContainer.EcuTranslation.TextPt;
                case "TITLE_ZHTW":
                    //[-] return characteristicsContainer.Title_zhtw;
                    //[+] return characteristicsContainer.EcuTranslation.TextZh;
                    return characteristicsContainer.EcuTranslation.TextZh;
                case "TITLE_JA":
                    //[-] return characteristicsContainer.Title_ja;
                    //[+] return characteristicsContainer.EcuTranslation.TextJa;
                    return characteristicsContainer.EcuTranslation.TextJa;
                case "TITLE_CSCZ":
                    //[-] return characteristicsContainer.Title_cscz;
                    //[+] return characteristicsContainer.EcuTranslation.TextCs;
                    return characteristicsContainer.EcuTranslation.TextCs;
                case "TITLE_PLPL":
                    //[-] return characteristicsContainer.Title_plpl;
                    //[+] return characteristicsContainer.EcuTranslation.TextPl;
                    return characteristicsContainer.EcuTranslation.TextPl;
                case "STATICCLASSVARIABLES":
                    //[-] if (!characteristicsContainer.StaticClassVariables.HasValue)
                    //[+] if (string.IsNullOrEmpty(characteristicsContainer.StaticClassVar))
                    if (string.IsNullOrEmpty(characteristicsContainer.StaticClassVar))
                    {
                        return "0";
                    }
                    //[-] return characteristicsContainer.StaticClassVariables.ToString();
                    //[+] return characteristicsContainer.StaticClassVar;
                    return characteristicsContainer.StaticClassVar;
                case "STATICCLASSVARIABLESMOTORRAD":
                    //[-] if (!characteristicsContainer.StaticClassVariablesMotorrad.HasValue)
                    //[+] if (string.IsNullOrEmpty(characteristicsContainer.StaticClassVarMotorrad))
                    if (string.IsNullOrEmpty(characteristicsContainer.StaticClassVarMCycle))
                    {
                        return "0";
                    }
                    //[-] return characteristicsContainer.StaticClassVariablesMotorrad.ToString();
                    //[+] return characteristicsContainer.StaticClassVarMCycle;
                    return characteristicsContainer.StaticClassVarMCycle;
                case "PARENTID":
                    //[-] if (!characteristicsContainer.ParentId.HasValue)
                    //[+] if (string.IsNullOrEmpty(characteristicsContainer.ParentId))
                    if (string.IsNullOrEmpty(characteristicsContainer.ParentId))
                    {
                        return "0";
                    }
                    return characteristicsContainer.ParentId.ToString();
                case "TITLE":
                    //[-] return characteristicsContainer.Title;
                    //[+] return characteristicsContainer.GetTitleTranslated(clientContext.Language);
                    return characteristicsContainer.GetTitleTranslated(clientContext.Language);
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
    }
}

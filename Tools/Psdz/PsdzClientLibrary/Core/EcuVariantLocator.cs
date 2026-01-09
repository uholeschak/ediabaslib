using BmwFileReader;
using PsdzClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
    public class EcuVariantLocator : IEcuVariantLocator, ISPELocator
    {
        [PreserveSource(Hint = "Database replaced")]
        private readonly PsdzDatabase.EcuVar ecuVariant;
        private readonly ISPELocator[] children;
        private ISPELocator[] parents;
        private readonly Vehicle vecInfo;
        private readonly IFFMDynamicResolverRuleEvaluation ffmResolver;
        [PreserveSource(Hint = "Cleaned", OriginalHash = "83D67209C35D5A5D9D545D1016A08CC0")]
        public ISPELocator[] Children
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Id => ecuVariant.Id.ToString(CultureInfo.InvariantCulture);

        public ISPELocator[] Parents
        {
            get
            {
                if (parents != null && parents.Length != 0)
                {
                    return parents;
                }
                List<ISPELocator> list = new List<ISPELocator>();
                //[-] if (ecuVariant.EcuGroupId.HasValue)
                //[+] if (string.IsNullOrEmpty(ecuVariant.EcuGroupId))
                if (string.IsNullOrEmpty(ecuVariant.EcuGroupId))
                {
                    //[-] XEP_ECUGROUPS ecuGroupById = DatabaseProviderFactory.Instance.GetEcuGroupById(ecuVariant.EcuGroupId.Value);
                    //[+] PsdzDatabase.EcuGroup ecuGroupById = ClientContext.GetDatabase(this.vecInfo)?.GetEcuGroupById(this.ecuVariant.EcuGroupId);
                    PsdzDatabase.EcuGroup ecuGroupById = ClientContext.GetDatabase(this.vecInfo)?.GetEcuGroupById(this.ecuVariant.EcuGroupId);
                    if (ecuGroupById != null)
                    {
                        list.Add(new EcuGroupLocator(ecuGroupById, vecInfo, ffmResolver));
                        parents = list.ToArray();
                    }
                }

                return parents;
            }
        }

        public string DataClassName => "ECUVariant";
        public string[] OutgoingLinkNames => new string[0];
        public string[] IncomingLinkNames => new string[0];
        public string[] DataValueNames => new string[29]
        {
            "ID",
            "FAULTMEMORYDELETEWAITINGTIME",
            "NAME",
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
            "VALIDFROM",
            "VALIDTO",
            "SICHERHEITSRELEVANT",
            "ECUGROUPID",
            "SORT"
        };

        public decimal SignedId
        {
            get
            {
                if (ecuVariant == null)
                {
                    return -1m;
                }

                //[-] return ecuVariant.Id;
                //[+] return ecuVariant.Id.ConvertToInt();
                return ecuVariant.Id.ConvertToInt();
            }
        }

        public Exception Exception => null;
        public bool HasException => false;

        [PreserveSource(Hint = "ecuVariant modified", OriginalHash = "8E5B65773EA09751505B070953098C4B")]
        public EcuVariantLocator(PsdzDatabase.EcuVar ecuVariant)
        {
            this.ecuVariant = ecuVariant;
            children = new ISPELocator[0];
        }

        [PreserveSource(Hint = "Database modified", OriginalHash = "075E47B43169E8EAEEECF27BF5B048ED")]
        public static IEcuVariantLocator CreateEcuVariantLocator(string ecuVariant, Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            PsdzDatabase.EcuVar ecuVariantByName = ClientContext.GetDatabase(vecInfo)?.GetEcuVariantByName(ecuVariant);
            if (ecuVariantByName != null)
            {
                return new EcuVariantLocator(ecuVariantByName, vecInfo, ffmResolver);
            }
            return null;
        }

        [PreserveSource(Hint = "Database modified", OriginalHash = "08BE2E1CCFA59CF2186602C6B28BF6E3")]
        public EcuVariantLocator(decimal id, Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            ecuVariant = ClientContext.GetDatabase(vecInfo)?.GetEcuVariantById(id.ToString(CultureInfo.InvariantCulture));
            this.vecInfo = vecInfo;
            this.ffmResolver = ffmResolver;
        }

        [PreserveSource(Hint = "ecuVariant modified", OriginalHash = "CC3856D70B3BE36AF29DACB2006C5BC2")]
        public EcuVariantLocator(PsdzDatabase.EcuVar ecuVariant, Vehicle vecInfo, IFFMDynamicResolverRuleEvaluation ffmResolver)
        {
            this.ecuVariant = ecuVariant;
            children = new ISPELocator[0];
            this.vecInfo = vecInfo;
            this.ffmResolver = ffmResolver;
        }

        [PreserveSource(Hint = "Modified", OriginalHash = "D0BBEA25A4E0F1D3C3997DC88102654E")]
        public string GetDataValue(string name)
        {
            if (ecuVariant == null || string.IsNullOrEmpty(name))
            {
                return null;
            }
            switch (name.ToUpperInvariant())
            {
                case "ID":
                    return ecuVariant.Id.ToString(CultureInfo.InvariantCulture);
                case "NODECLASS":
                    return "5719042";
                case "TITLE_DEDE":
                    return ecuVariant.EcuTranslation.TextDe;
                case "TITLE_ENGB":
                    return ecuVariant.EcuTranslation.TextEn;
                case "TITLE_ENUS":
                    return ecuVariant.EcuTranslation.TextEn;
                case "TITLE_FR":
                    return ecuVariant.EcuTranslation.TextFr;
                case "TITLE_TH":
                    return ecuVariant.EcuTranslation.TextTh;
                case "TITLE_SV":
                    return ecuVariant.EcuTranslation.TextSv;
                case "TITLE_IT":
                    return ecuVariant.EcuTranslation.TextIt;
                case "TITLE_ES":
                    return ecuVariant.EcuTranslation.TextEs;
                case "TITLE_ID":
                    return ecuVariant.EcuTranslation.TextId;
                case "TITLE_KO":
                    return ecuVariant.EcuTranslation.TextKo;
                case "TITLE_EL":
                    return ecuVariant.EcuTranslation.TextEl;
                case "TITLE_TR":
                    return ecuVariant.EcuTranslation.TextTr;
                case "TITLE_ZHCN":
                    return ecuVariant.EcuTranslation.TextZh;
                case "TITLE_RU":
                    return ecuVariant.EcuTranslation.TextRu;
                case "TITLE_NL":
                    return ecuVariant.EcuTranslation.TextNl;
                case "TITLE_PT":
                    return ecuVariant.EcuTranslation.TextPt;
                case "TITLE_ZHTW":
                    return ecuVariant.EcuTranslation.TextZh;
                case "TITLE_JA":
                    return ecuVariant.EcuTranslation.TextJa;
                case "TITLE_CSCZ":
                    return ecuVariant.EcuTranslation.TextCs;
                case "TITLE_PLPL":
                    return ecuVariant.EcuTranslation.TextPl;
                case "FAULTMEMORYDELETEWAITINGTIME":
                    if (string.IsNullOrEmpty(ecuVariant.FaultMemDelWaitTime))
                    {
                        return string.Empty;
                    }

                    return ecuVariant.FaultMemDelWaitTime;
                case "NAME":
                    return ecuVariant.Name;
                case "ECUGROUPID":
                    if (string.IsNullOrEmpty(ecuVariant.EcuGroupId))
                    {
                        return "0";
                    }

                    return ecuVariant.EcuGroupId;
                case "SORT":
                    if (string.IsNullOrEmpty(ecuVariant.Sort))
                    {
                        return "0";
                    }

                    return ecuVariant.Sort;
                case "VALIDFROM":
                    if (string.IsNullOrEmpty(ecuVariant.ValidFrom))
                    {
                        return string.Empty;
                    }

                    return ecuVariant.ValidFrom;
                case "VALIDTO":
                    if (string.IsNullOrEmpty(ecuVariant.ValidTo))
                    {
                        return string.Empty;
                    }

                    return ecuVariant.ValidTo;
                case "SICHERHEITSRELEVANT":
                    if (string.IsNullOrEmpty(ecuVariant.SafetyRelevant))
                    {
                        return "0";
                    }

                    return ecuVariant.SafetyRelevant;
                case "TITLE":
                    return ecuVariant.EcuTranslation.GetTitle(ClientContext.GetClientContext(vecInfo));
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

        [PreserveSource(Hint = "Modified", OriginalHash = "4A8CA5648240C30EA714ADA304782496")]
        public T GetDataValue<T>(string name)
        {
            try
            {
                if (!string.IsNullOrEmpty(name) && ecuVariant != null)
                {
                    object obj = null;
                    switch (name.ToUpperInvariant())
                    {
                        case "ID":
                            obj = ecuVariant.Id;
                            break;
                        case "NODECLASS":
                            obj = "5719042";
                            break;
                        case "TITLE_DEDE":
                            obj = ecuVariant.EcuTranslation.TextDe;
                            break;
                        case "TITLE_ENGB":
                            obj = ecuVariant.EcuTranslation.TextEn;
                            break;
                        case "TITLE_ENUS":
                            obj = ecuVariant.EcuTranslation.TextEn;
                            break;
                        case "TITLE_FR":
                            obj = ecuVariant.EcuTranslation.TextFr;
                            break;
                        case "TITLE_TH":
                            obj = ecuVariant.EcuTranslation.TextTh;
                            break;
                        case "TITLE_SV":
                            obj = ecuVariant.EcuTranslation.TextSv;
                            break;
                        case "TITLE_IT":
                            obj = ecuVariant.EcuTranslation.TextIt;
                            break;
                        case "TITLE_ES":
                            obj = ecuVariant.EcuTranslation.TextEs;
                            break;
                        case "TITLE_ID":
                            obj = ecuVariant.EcuTranslation.TextId;
                            break;
                        case "TITLE_KO":
                            obj = ecuVariant.EcuTranslation.TextKo;
                            break;
                        case "TITLE_EL":
                            obj = ecuVariant.EcuTranslation.TextEl;
                            break;
                        case "TITLE_TR":
                            obj = ecuVariant.EcuTranslation.TextTr;
                            break;
                        case "TITLE_ZHCN":
                            obj = ecuVariant.EcuTranslation.TextZh;
                            break;
                        case "TITLE_RU":
                            obj = ecuVariant.EcuTranslation.TextRu;
                            break;
                        case "TITLE_NL":
                            obj = ecuVariant.EcuTranslation.TextNl;
                            break;
                        case "TITLE_PT":
                            obj = ecuVariant.EcuTranslation.TextPt;
                            break;
                        case "TITLE_ZHTW":
                            obj = ecuVariant.EcuTranslation.TextZh;
                            break;
                        case "TITLE_JA":
                            obj = ecuVariant.EcuTranslation.TextJa;
                            break;
                        case "TITLE_CSCZ":
                            obj = ecuVariant.EcuTranslation.TextCs;
                            break;
                        case "TITLE_PLPL":
                            obj = ecuVariant.EcuTranslation.TextPl;
                            break;
                        case "FAULTMEMORYDELETEWAITINGTIME":
                            obj = ecuVariant.FaultMemDelWaitTime;
                            break;
                        case "NAME":
                            obj = ecuVariant.Name;
                            break;
                        case "ECUGROUPID":
                            obj = ecuVariant.EcuGroupId;
                            break;
                        case "SORT":
                            obj = ecuVariant.Sort;
                            break;
                        case "VALIDFROM":
                            obj = !string.IsNullOrEmpty(ecuVariant.ValidFrom);
                            break;
                        case "VALIDTO":
                            obj = !string.IsNullOrEmpty(ecuVariant.ValidTo);
                            break;
                        case "SICHERHEITSRELEVANT":
                            obj = ecuVariant.SafetyRelevant;
                            break;
                        case "TITLE":
                            obj = ecuVariant.EcuTranslation.GetTitle(ClientContext.GetClientContext(vecInfo));
                            break;
                    }
                    if (obj != null)
                    {
                        return (T)Convert.ChangeType(obj, typeof(T));
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("EcuVariantLocator.GetDataValue<T>()", exception);
            }
            return default(T);
        }
    }
}
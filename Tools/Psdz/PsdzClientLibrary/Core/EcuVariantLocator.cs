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

        [PreserveSource(Hint = "ecuVariant modified", SignatureModified = true)]
        public EcuVariantLocator(PsdzDatabase.EcuVar ecuVariant)
        {
            this.ecuVariant = ecuVariant;
            children = new ISPELocator[0];
        }

        public static IEcuVariantLocator CreateEcuVariantLocator(string ecuVariant, Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            //[-] XEP_ECUVARIANTS ecuVariantByName = DatabaseProviderFactory.Instance.GetEcuVariantByName(ecuVariant);
            //[+] PsdzDatabase.EcuVar ecuVariantByName = ClientContext.GetDatabase(vecInfo)?.GetEcuVariantByName(ecuVariant);
            PsdzDatabase.EcuVar ecuVariantByName = ClientContext.GetDatabase(vecInfo)?.GetEcuVariantByName(ecuVariant);
            if (ecuVariantByName != null)
            {
                return new EcuVariantLocator(ecuVariantByName, vecInfo, ffmResolver);
            }
            return null;
        }

        [PreserveSource(Hint = "Database modified", SignatureModified = true)]
        public EcuVariantLocator(decimal id, Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            //[-] ecuVariant = DatabaseProviderFactory.Instance.GetEcuVariantById(id);
            //[+] ecuVariant = ClientContext.GetDatabase(vecInfo)?.GetEcuVariantById(id.ToString(CultureInfo.InvariantCulture));
            ecuVariant = ClientContext.GetDatabase(vecInfo)?.GetEcuVariantById(id.ToString(CultureInfo.InvariantCulture));
            this.vecInfo = vecInfo;
            this.ffmResolver = ffmResolver;
        }

        [PreserveSource(Hint = "ecuVariant modified", SignatureModified = true)]
        public EcuVariantLocator(PsdzDatabase.EcuVar ecuVariant, Vehicle vecInfo, IFFMDynamicResolverRuleEvaluation ffmResolver)
        {
            this.ecuVariant = ecuVariant;
            children = new ISPELocator[0];
            this.vecInfo = vecInfo;
            this.ffmResolver = ffmResolver;
        }

        [PreserveSource(Hint = "Modified", SignatureModified = true)]
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
                    //[-] return ecuVariant.Title_dede;
                    //[+] return ecuVariant.EcuTranslation.TextDe;
                    return ecuVariant.EcuTranslation.TextDe;
                case "TITLE_ENGB":
                    //[-] return ecuVariant.Title_engb;
                    //[+] return ecuVariant.EcuTranslation.TextEn;
                    return ecuVariant.EcuTranslation.TextEn;
                case "TITLE_ENUS":
                    //[-] return ecuVariant.Title_enus;
                    //[+] return ecuVariant.EcuTranslation.TextEn;
                    return ecuVariant.EcuTranslation.TextEn;
                case "TITLE_FR":
                    //[-] return ecuVariant.Title_fr;
                    //[+] return ecuVariant.EcuTranslation.TextFr;
                    return ecuVariant.EcuTranslation.TextFr;
                case "TITLE_TH":
                    //[-] return ecuVariant.Title_th;
                    //[+] return ecuVariant.EcuTranslation.TextTh;
                    return ecuVariant.EcuTranslation.TextTh;
                case "TITLE_SV":
                    //[-] return ecuVariant.Title_sv;
                    //[+] return ecuVariant.EcuTranslation.TextSv;
                    return ecuVariant.EcuTranslation.TextSv;
                case "TITLE_IT":
                    //[-] return ecuVariant.Title_it;
                    //[+] return ecuVariant.EcuTranslation.TextIt;
                    return ecuVariant.EcuTranslation.TextIt;
                case "TITLE_ES":
                    //[-] return ecuVariant.Title_es;
                    //[+] return ecuVariant.EcuTranslation.TextEs;
                    return ecuVariant.EcuTranslation.TextEs;
                case "TITLE_ID":
                    //[-] return ecuVariant.Title_id;
                    //[+] return ecuVariant.EcuTranslation.TextId;
                    return ecuVariant.EcuTranslation.TextId;
                case "TITLE_KO":
                    //[-] return ecuVariant.Title_ko;
                    //[+] return ecuVariant.EcuTranslation.TextKo;
                    return ecuVariant.EcuTranslation.TextKo;
                case "TITLE_EL":
                    //[-] return ecuVariant.Title_el;
                    //[+] return ecuVariant.EcuTranslation.TextEl;
                    return ecuVariant.EcuTranslation.TextEl;
                case "TITLE_TR":
                    //[-] return ecuVariant.Title_tr;
                    //[+] return ecuVariant.EcuTranslation.TextTr;
                    return ecuVariant.EcuTranslation.TextTr;
                case "TITLE_ZHCN":
                    //[-] return ecuVariant.Title_zhcn;
                    //[+] return ecuVariant.EcuTranslation.TextZh;
                    return ecuVariant.EcuTranslation.TextZh;
                case "TITLE_RU":
                    //[-] return ecuVariant.Title_ru;
                    //[+] return ecuVariant.EcuTranslation.TextRu;
                    return ecuVariant.EcuTranslation.TextRu;
                case "TITLE_NL":
                    //[-] return ecuVariant.Title_nl;
                    //[+] return ecuVariant.EcuTranslation.TextNl;
                    return ecuVariant.EcuTranslation.TextNl;
                case "TITLE_PT":
                    //[-] return ecuVariant.Title_pt;
                    //[+] return ecuVariant.EcuTranslation.TextPt;
                    return ecuVariant.EcuTranslation.TextPt;
                case "TITLE_ZHTW":
                    //[-] return ecuVariant.Title_zhtw;
                    //[+] return ecuVariant.EcuTranslation.TextZh;
                    return ecuVariant.EcuTranslation.TextZh;
                case "TITLE_JA":
                    //[-] return ecuVariant.Title_ja;
                    //[+] return ecuVariant.EcuTranslation.TextJa;
                    return ecuVariant.EcuTranslation.TextJa;
                case "TITLE_CSCZ":
                    //[-] return ecuVariant.Title_cscz;
                    //[+] return ecuVariant.EcuTranslation.TextCs;
                    return ecuVariant.EcuTranslation.TextCs;
                case "TITLE_PLPL":
                    //[-] return ecuVariant.Title_plpl;
                    //[+] return ecuVariant.EcuTranslation.TextPl;
                    return ecuVariant.EcuTranslation.TextPl;
                case "FAULTMEMORYDELETEWAITINGTIME":
                    //[-] if (!ecuVariant.FaultMemoryDeleteWaitingTime.HasValue)
                    //[+] if (string.IsNullOrEmpty(ecuVariant.FaultMemDelWaitTime))
                    if (string.IsNullOrEmpty(ecuVariant.FaultMemDelWaitTime))
                    {
                        return string.Empty;
                    }

                    //[-] return ecuVariant.FaultMemoryDeleteWaitingTime.ToString();
                    //[+] return ecuVariant.FaultMemDelWaitTime;
                    return ecuVariant.FaultMemDelWaitTime;
                case "NAME":
                    return ecuVariant.Name;
                case "ECUGROUPID":
                    //[-] if (!ecuVariant.EcuGroupId.HasValue)
                    //[+] if (string.IsNullOrEmpty(ecuVariant.EcuGroupId))
                    if (string.IsNullOrEmpty(ecuVariant.EcuGroupId))
                    {
                        return "0";
                    }

                    //[-] return ecuVariant.EcuGroupId.ToString();
                    //[+] return ecuVariant.EcuGroupId;
                    return ecuVariant.EcuGroupId;
                case "SORT":
                    //[-] if (!ecuVariant.Sort.HasValue)
                    //[+] if (string.IsNullOrEmpty(ecuVariant.Sort))
                    if (string.IsNullOrEmpty(ecuVariant.Sort))
                    {
                        return "0";
                    }

                    //[-] return ecuVariant.Sort.ToString();
                    //[+] return ecuVariant.Sort;
                    return ecuVariant.Sort;
                case "VALIDFROM":
                    //[-] if (!ecuVariant.ValidFrom.HasValue)
                    //[+] if (string.IsNullOrEmpty(ecuVariant.ValidFrom))
                    if (string.IsNullOrEmpty(ecuVariant.ValidFrom))
                    {
                        return string.Empty;
                    }

                    //[-] return ecuVariant.ValidFrom.ToString();
                    //[+] return ecuVariant.ValidFrom;
                    return ecuVariant.ValidFrom;
                case "VALIDTO":
                    //[-] if (!ecuVariant.ValidTo.HasValue)
                    //[+] if (string.IsNullOrEmpty(ecuVariant.ValidTo))
                    if (string.IsNullOrEmpty(ecuVariant.ValidTo))
                    {
                        return string.Empty;
                    }

                    //[-] return ecuVariant.ValidTo.ToString();
                    //[+] return ecuVariant.ValidTo;
                    return ecuVariant.ValidTo;
                case "SICHERHEITSRELEVANT":
                    //[-] if (!ecuVariant.Sicherheitsrelevant.HasValue)
                    //[+] if (string.IsNullOrEmpty(ecuVariant.SafetyRelevant))
                    if (string.IsNullOrEmpty(ecuVariant.SafetyRelevant))
                    {
                        return "0";
                    }

                    //[-] return ecuVariant.Sicherheitsrelevant.ToString();
                    //[+] return ecuVariant.SafetyRelevant;
                    return ecuVariant.SafetyRelevant;
                case "TITLE":
                    //[-] return ecuVariant.Title;
                    //[+] return ecuVariant.EcuTranslation.GetTitle(ClientContext.GetClientContext(vecInfo));
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

        [PreserveSource(Hint = "Modified", SignatureModified = true)]
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
                            //[-] obj = ecuVariant.Title_dede;
                            //[+] obj = ecuVariant.EcuTranslation.TextDe;
                            obj = ecuVariant.EcuTranslation.TextDe;
                            break;
                        case "TITLE_ENGB":
                            //[-] obj = ecuVariant.Title_engb;
                            //[+] obj = ecuVariant.EcuTranslation.TextEn;
                            obj = ecuVariant.EcuTranslation.TextEn;
                            break;
                        case "TITLE_ENUS":
                            //[-] obj = ecuVariant.Title_enus;
                            //[+] obj = ecuVariant.EcuTranslation.TextEn;
                            obj = ecuVariant.EcuTranslation.TextEn;
                            break;
                        case "TITLE_FR":
                            //[-] obj = ecuVariant.Title_fr;
                            //[+] obj = ecuVariant.EcuTranslation.TextFr;
                            obj = ecuVariant.EcuTranslation.TextFr;
                            break;
                        case "TITLE_TH":
                            //[-] obj = ecuVariant.Title_th;
                            //[+] obj = ecuVariant.EcuTranslation.TextTh;
                            obj = ecuVariant.EcuTranslation.TextTh;
                            break;
                        case "TITLE_SV":
                            //[-] obj = ecuVariant.Title_sv;
                            //[+] obj = ecuVariant.EcuTranslation.TextSv;
                            obj = ecuVariant.EcuTranslation.TextSv;
                            break;
                        case "TITLE_IT":
                            //[-] obj = ecuVariant.Title_it;
                            //[+] obj = ecuVariant.EcuTranslation.TextIt;
                            obj = ecuVariant.EcuTranslation.TextIt;
                            break;
                        case "TITLE_ES":
                            //[-] obj = ecuVariant.Title_es;
                            //[+] obj = ecuVariant.EcuTranslation.TextEs;
                            obj = ecuVariant.EcuTranslation.TextEs;
                            break;
                        case "TITLE_ID":
                            //[-] obj = ecuVariant.Title_id;
                            //[+] obj = ecuVariant.EcuTranslation.TextId;
                            obj = ecuVariant.EcuTranslation.TextId;
                            break;
                        case "TITLE_KO":
                            //[-] obj = ecuVariant.Title_ko;
                            //[+] obj = ecuVariant.EcuTranslation.TextKo;
                            obj = ecuVariant.EcuTranslation.TextKo;
                            break;
                        case "TITLE_EL":
                            //[-] obj = ecuVariant.Title_el;
                            //[+] obj = ecuVariant.EcuTranslation.TextEl;
                            obj = ecuVariant.EcuTranslation.TextEl;
                            break;
                        case "TITLE_TR":
                            //[-] obj = ecuVariant.Title_tr;
                            //[+] obj = ecuVariant.EcuTranslation.TextTr;
                            obj = ecuVariant.EcuTranslation.TextTr;
                            break;
                        case "TITLE_ZHCN":
                            //[-] obj = ecuVariant.Title_zhcn;
                            //[+] obj = ecuVariant.EcuTranslation.TextZh;
                            obj = ecuVariant.EcuTranslation.TextZh;
                            break;
                        case "TITLE_RU":
                            //[-] obj = ecuVariant.Title_ru;
                            //[+] obj = ecuVariant.EcuTranslation.TextRu;
                            obj = ecuVariant.EcuTranslation.TextRu;
                            break;
                        case "TITLE_NL":
                            //[-] obj = ecuVariant.Title_nl;
                            //[+] obj = ecuVariant.EcuTranslation.TextNl;
                            obj = ecuVariant.EcuTranslation.TextNl;
                            break;
                        case "TITLE_PT":
                            //[-] obj = ecuVariant.Title_pt;
                            //[+] obj = ecuVariant.EcuTranslation.TextPt;
                            obj = ecuVariant.EcuTranslation.TextPt;
                            break;
                        case "TITLE_ZHTW":
                            //[-] obj = ecuVariant.Title_zhtw;
                            //[+] obj = ecuVariant.EcuTranslation.TextZh;
                            obj = ecuVariant.EcuTranslation.TextZh;
                            break;
                        case "TITLE_JA":
                            //[-] obj = ecuVariant.Title_ja;
                            //[+] obj = ecuVariant.EcuTranslation.TextJa;
                            obj = ecuVariant.EcuTranslation.TextJa;
                            break;
                        case "TITLE_CSCZ":
                            //[-] obj = ecuVariant.Title_cscz;
                            //[+] obj = ecuVariant.EcuTranslation.TextCs;
                            obj = ecuVariant.EcuTranslation.TextCs;
                            break;
                        case "TITLE_PLPL":
                            //[-] obj = ecuVariant.Title_plpl;
                            //[+] obj = ecuVariant.EcuTranslation.TextPl;
                            obj = ecuVariant.EcuTranslation.TextPl;
                            break;
                        case "FAULTMEMORYDELETEWAITINGTIME":
                            //[-] obj = ecuVariant.FaultMemoryDeleteWaitingTime;
                            //[+] obj = ecuVariant.FaultMemDelWaitTime;
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
                            //[-] obj = ecuVariant.ValidFrom.HasValue;
                            //[+] obj = !string.IsNullOrEmpty(ecuVariant.ValidFrom);
                            obj = !string.IsNullOrEmpty(ecuVariant.ValidFrom);
                            break;
                        case "VALIDTO":
                            //[-] obj = ecuVariant.ValidTo.HasValue;
                            //[+] obj = !string.IsNullOrEmpty(ecuVariant.ValidTo);
                            obj = !string.IsNullOrEmpty(ecuVariant.ValidTo);
                            break;
                        case "SICHERHEITSRELEVANT":
                            //[-] obj = ecuVariant.Sicherheitsrelevant;
                            //[+] obj = ecuVariant.SafetyRelevant;
                            obj = ecuVariant.SafetyRelevant;
                            break;
                        case "TITLE":
                            //[-] obj = ecuVariant.Title;
                            //[+] obj = ecuVariant.EcuTranslation.GetTitle(ClientContext.GetClientContext(vecInfo));
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
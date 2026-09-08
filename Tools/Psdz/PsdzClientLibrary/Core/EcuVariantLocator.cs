using BmwFileReader;
using PsdzClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PsdzClientLibrary;

namespace PsdzClient.Core
{
    public class EcuVariantLocator : IEcuVariantLocator, ISPELocator
    {
        private readonly IXepEcuVariants ecuVariant;
        private readonly ISPELocator[] children;
        private ISPELocator[] parents;
        private readonly Vehicle vecInfo;
        private readonly IFFMDynamicResolverRuleEvaluation ffmResolver;
        [PreserveSource(Cleaned = true, OriginalHash = "83D67209C35D5A5D9D545D1016A08CC0")]
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
                if (ecuVariant.EcuGroupId.HasValue)
                {
                    //[-] XEP_ECUGROUPS ecuGroupById = DatabaseProviderFactory.Instance.GetEcuGroupById(ecuVariant.EcuGroupId.Value);
                    //[+] XEP_ECUGROUPS ecuGroupById = XepConverter.Convert(ClientContext.GetDatabase(vecInfo)?.GetEcuGroupById(this.ecuVariant.EcuGroupId.Value.ToString(CultureInfo.InvariantCulture)));
                    XEP_ECUGROUPS ecuGroupById = XepConverter.Convert(ClientContext.GetDatabase(vecInfo)?.GetEcuGroupById(this.ecuVariant.EcuGroupId.Value.ToString(CultureInfo.InvariantCulture)));
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

                return ecuVariant.Id;
            }
        }

        public Exception Exception => null;
        public bool HasException => false;

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public EcuVariantLocator(XEP_ECUVARIANTS ecuVariant)
        {
            this.ecuVariant = ecuVariant;
            children = new ISPELocator[0];
        }

        public static IEcuVariantLocator CreateEcuVariantLocator(string ecuVariant, Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            //[-] XEP_ECUVARIANTS ecuVariantByName = DatabaseProviderFactory.Instance.GetEcuVariantByName(ecuVariant);
            //[+] XEP_ECUVARIANTS ecuVariantByName = XepConverter.Convert(ClientContext.GetDatabase(vecInfo)?.GetEcuVariantByName(ecuVariant));
            XEP_ECUVARIANTS ecuVariantByName = XepConverter.Convert(ClientContext.GetDatabase(vecInfo)?.GetEcuVariantByName(ecuVariant));
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
            //[+] ecuVariant = XepConverter.Convert(ClientContext.GetDatabase(vecInfo)?.GetEcuVariantById(id.ToString(CultureInfo.InvariantCulture)));
            ecuVariant = XepConverter.Convert(ClientContext.GetDatabase(vecInfo)?.GetEcuVariantById(id.ToString(CultureInfo.InvariantCulture)));
            this.vecInfo = vecInfo;
            this.ffmResolver = ffmResolver;
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public EcuVariantLocator(XEP_ECUVARIANTS ecuVariant, Vehicle vecInfo, IFFMDynamicResolverRuleEvaluation ffmResolver)
        {
            this.ecuVariant = ecuVariant;
            children = new ISPELocator[0];
            this.vecInfo = vecInfo;
            this.ffmResolver = ffmResolver;
        }

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
                    return ecuVariant.Title_dede;
                case "TITLE_ENGB":
                    return ecuVariant.Title_engb;
                case "TITLE_ENUS":
                    return ecuVariant.Title_enus;
                case "TITLE_FR":
                    return ecuVariant.Title_fr;
                case "TITLE_TH":
                    return ecuVariant.Title_th;
                case "TITLE_SV":
                    return ecuVariant.Title_sv;
                case "TITLE_IT":
                    return ecuVariant.Title_it;
                case "TITLE_ES":
                    return ecuVariant.Title_es;
                case "TITLE_ID":
                    return ecuVariant.Title_id;
                case "TITLE_KO":
                    return ecuVariant.Title_ko;
                case "TITLE_EL":
                    return ecuVariant.Title_el;
                case "TITLE_TR":
                    return ecuVariant.Title_tr;
                case "TITLE_ZHCN":
                    return ecuVariant.Title_zhcn;
                case "TITLE_RU":
                    return ecuVariant.Title_ru;
                case "TITLE_NL":
                    return ecuVariant.Title_nl;
                case "TITLE_PT":
                    return ecuVariant.Title_pt;
                case "TITLE_ZHTW":
                    return ecuVariant.Title_zhtw;
                case "TITLE_JA":
                    return ecuVariant.Title_ja;
                case "TITLE_CSCZ":
                    return ecuVariant.Title_cscz;
                case "TITLE_PLPL":
                    return ecuVariant.Title_plpl;
                case "FAULTMEMORYDELETEWAITINGTIME":
                    if (!ecuVariant.FaultMemoryDeleteWaitingTime.HasValue)
                    {
                        return string.Empty;
                    }

                    return ecuVariant.FaultMemoryDeleteWaitingTime.ToString();
                case "NAME":
                    return ecuVariant.Name;
                case "ECUGROUPID":
                    if (!ecuVariant.EcuGroupId.HasValue)
                    {
                        return "0";
                    }

                    return ecuVariant.EcuGroupId.ToString();
                case "SORT":
                    if (!ecuVariant.Sort.HasValue)
                    {
                        return "0";
                    }

                    return ecuVariant.Sort.ToString();
                case "VALIDFROM":
                    if (!ecuVariant.ValidFrom.HasValue)
                    {
                        return string.Empty;
                    }

                    return ecuVariant.ValidFrom.ToString();
                case "VALIDTO":
                    if (!ecuVariant.ValidTo.HasValue)
                    {
                        return string.Empty;
                    }

                    return ecuVariant.ValidTo.ToString();
                case "SICHERHEITSRELEVANT":
                    if (!ecuVariant.Sicherheitsrelevant.HasValue)
                    {
                        return "0";
                    }

                    return ecuVariant.Sicherheitsrelevant.ToString();
                case "TITLE":
                    return ecuVariant.Title;
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
                            obj = ecuVariant.Title_dede;
                            break;
                        case "TITLE_ENGB":
                            obj = ecuVariant.Title_engb;
                            break;
                        case "TITLE_ENUS":
                            obj = ecuVariant.Title_enus;
                            break;
                        case "TITLE_FR":
                            obj = ecuVariant.Title_fr;
                            break;
                        case "TITLE_TH":
                            obj = ecuVariant.Title_th;
                            break;
                        case "TITLE_SV":
                            obj = ecuVariant.Title_sv;
                            break;
                        case "TITLE_IT":
                            obj = ecuVariant.Title_it;
                            break;
                        case "TITLE_ES":
                            obj = ecuVariant.Title_es;
                            break;
                        case "TITLE_ID":
                            obj = ecuVariant.Title_id;
                            break;
                        case "TITLE_KO":
                            obj = ecuVariant.Title_ko;
                            break;
                        case "TITLE_EL":
                            obj = ecuVariant.Title_el;
                            break;
                        case "TITLE_TR":
                            obj = ecuVariant.Title_tr;
                            break;
                        case "TITLE_ZHCN":
                            obj = ecuVariant.Title_zhcn;
                            break;
                        case "TITLE_RU":
                            obj = ecuVariant.Title_ru;
                            break;
                        case "TITLE_NL":
                            obj = ecuVariant.Title_nl;
                            break;
                        case "TITLE_PT":
                            obj = ecuVariant.Title_pt;
                            break;
                        case "TITLE_ZHTW":
                            obj = ecuVariant.Title_zhtw;
                            break;
                        case "TITLE_JA":
                            obj = ecuVariant.Title_ja;
                            break;
                        case "TITLE_CSCZ":
                            obj = ecuVariant.Title_cscz;
                            break;
                        case "TITLE_PLPL":
                            obj = ecuVariant.Title_plpl;
                            break;
                        case "FAULTMEMORYDELETEWAITINGTIME":
                            obj = ecuVariant.FaultMemoryDeleteWaitingTime;
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
                            obj = ecuVariant.ValidFrom.HasValue;
                            break;
                        case "VALIDTO":
                            obj = ecuVariant.ValidTo.HasValue;
                            break;
                        case "SICHERHEITSRELEVANT":
                            obj = ecuVariant.Sicherheitsrelevant;
                            break;
                        case "TITLE":
                            obj = ecuVariant.Title;
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
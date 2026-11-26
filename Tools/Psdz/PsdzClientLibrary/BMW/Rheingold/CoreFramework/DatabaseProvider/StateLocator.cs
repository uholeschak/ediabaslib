using BMW.Rheingold.CoreFramework.Contracts;
using PsdzClient;
using System;
using System.Globalization;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class StateLocator : IStateLocator, ISPELocator
    {
        private Exception exception;
        private readonly XEP_STATEVALUES state;
        private ITextContent localTc;
        public ISPELocator[] Children => new ISPELocator[0];
        public string DataClassName => "State";
        public string[] DataValueNames => new string[28]
        {
            "ID",
            "TITLEID",
            "TITLE",
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
            "STATEVALUE",
            "VALIDFROM",
            "VALIDTO",
            "SICHERHEITSRELEVANT",
            "PARENTID"
        };
        public Exception Exception => exception;
        public bool HasException => exception != null;

        public string Id
        {
            get
            {
                if (state != null)
                {
                    return state.Id.ToString(CultureInfo.InvariantCulture);
                }

                return string.Empty;
            }
        }

        public string[] IncomingLinkNames => new string[0];
        public string[] OutgoingLinkNames => new string[0];

        public decimal? ParentId
        {
            get
            {
                if (state != null)
                {
                    return state.ParentId;
                }

                return null;
            }
        }

        public ISPELocator[] Parents => new ISPELocator[0];

        public decimal SignedId
        {
            get
            {
                if (state == null)
                {
                    return -1m;
                }

                return state.Id;
            }
        }

        public string Statevalue
        {
            get
            {
                if (state != null)
                {
                    return state.Statevalue;
                }

                return null;
            }

            set
            {
                if (state != null)
                {
                    state.Statevalue = value;
                }
            }
        }

        public ITextContent TextContent
        {
            get
            {
                if (state != null)
                {
                    if (localTc == null)
                    {
                        return new TextContent(state.Title);
                    }

                    return localTc;
                }

                return null;
            }

            set
            {
                localTc = value;
            }
        }

        [PreserveSource(Hint = "clientContext added", SignatureModified = true)]
        public StateLocator(decimal stateValueId, ClientContext clientContext)
        {
            try
            {
                //[-] state = DatabaseProviderFactory.Instance.GetXepStateValuesById(stateValueId);
                //[+] state = clientContext.Database.GetXepStateValuesById(stateValueId);
                state = clientContext.Database.GetXepStateValuesById(stateValueId);
            }
            catch (Exception ex)
            {
                Log.WarningException("State.State()", ex);
                exception = ex;
            }
        }

        [PreserveSource(Hint = "clientContext added", SignatureModified = true)]
        public StateLocator(string stateValueId, ClientContext clientContext)
        {
            try
            {
                decimal id = Convert.ToDecimal(stateValueId);
                //[-] state = DatabaseProviderFactory.Instance.GetXepStateValuesById(id);
                //[+] state = clientContext.Database.GetXepStateValuesById(id);
                state = clientContext.Database.GetXepStateValuesById(id);
            }
            catch (Exception ex)
            {
                Log.WarningException("State.State()", ex);
                exception = ex;
            }
        }

        public StateLocator(XEP_STATEVALUES xepState)
        {
            state = xepState;
        }

        public string GetDataValue(string name)
        {
            if (state != null && !string.IsNullOrEmpty(name))
            {
                switch (name.ToUpperInvariant())
                {
                    case "ID":
                        return Id;
                    case "TITLEID":
                        if (state.TitleId.HasValue)
                        {
                            return state.TitleId.ToString();
                        }

                        break;
                    case "TITLE":
                        return state.Title;
                    case "TITLE_DEDE":
                        return state.Title_dede;
                    case "TITLE_ENGB":
                        return state.Title_engb;
                    case "TITLE_ENUS":
                        return state.Title_enus;
                    case "TITLE_FR":
                        return state.Title_fr;
                    case "TITLE_TH":
                        return state.Title_th;
                    case "TITLE_SV":
                        return state.Title_sv;
                    case "TITLE_IT":
                        return state.Title_it;
                    case "TITLE_ES":
                        return state.Title_es;
                    case "TITLE_ID":
                        return state.Title_id;
                    case "TITLE_KO":
                        return state.Title_ko;
                    case "TITLE_EL":
                        return state.Title_el;
                    case "TITLE_TR":
                        return state.Title_tr;
                    case "TITLE_ZHCN":
                        return state.Title_zhcn;
                    case "TITLE_RU":
                        return state.Title_ru;
                    case "TITLE_NL":
                        return state.Title_nl;
                    case "TITLE_PT":
                        return state.Title_pt;
                    case "TITLE_ZHTW":
                        return state.Title_zhtw;
                    case "TITLE_JA":
                        return state.Title_ja;
                    case "TITLE_CSCZ":
                        return state.Title_cscz;
                    case "TITLE_PLPL":
                        return state.Title_plpl;
                    case "STATEVALUE":
                        return state.Statevalue;
                    case "VALIDFROM":
                        if (state.ValidFrom.HasValue)
                        {
                            return state.ValidFrom.ToString();
                        }

                        break;
                    case "VALIDTO":
                        if (state.ValidTo.HasValue)
                        {
                            return state.ValidTo.ToString();
                        }

                        break;
                    case "SICHERHEITSRELEVANT":
                        if (state.Sicherheitsrelevant.HasValue)
                        {
                            return state.Sicherheitsrelevant.ToString();
                        }

                        break;
                    case "PARENTID":
                        if (state.ParentId.HasValue)
                        {
                            return state.ParentId.ToString();
                        }

                        break;
                }
            }

            return null;
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

        public T GetDataValue<T>(string name)
        {
            return default;
        }
    }
}
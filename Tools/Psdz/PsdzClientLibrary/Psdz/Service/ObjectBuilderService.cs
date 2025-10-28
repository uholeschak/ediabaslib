using System;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Swt;
using BMW.Rheingold.Psdz.Model.Tal;
using BMW.Rheingold.Psdz.Model.Tal.TalFilter;
using PsdzClient.Core;
using System.Collections.Generic;
using System.Linq;
using RestSharp;

namespace BMW.Rheingold.Psdz
{
    internal class ObjectBuilderService : IObjectBuilderService
    {
        private static TalFilterActionMapper talFilterActionMapper = new TalFilterActionMapper();

        private static TaCategoryTypeMapper taCategoryTypeMapper = new TaCategoryTypeMapper();

        private static ProtocolMapper protocolMapper = new ProtocolMapper();

        private readonly IWebCallHandler webCallHandler;

        private readonly string faControllerName = "fa";

        private readonly string swtActionControllerName = "swtaction";

        private readonly string talControllerName = "tal";

        private readonly string talFilterControllerName = "talfilter";

        public ObjectBuilderService(IWebCallHandler webCallHandler)
        {
            this.webCallHandler = webCallHandler;
        }

        public IPsdzFa BuildFa(IPsdzFa faInput)
        {
            try
            {
                BuildFaRequestModel requestBodyObject = new BuildFaRequestModel
                {
                    Fa = FaMapper.Map(faInput)
                };
                return FaMapper.Map(webCallHandler.ExecuteRequest<FaModel>(faControllerName, "build", Method.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzFa BuildFaFromXml(string xmlFa)
        {
            try
            {
                BuildFaFromXmlRequestModel requestBodyObject = new BuildFaFromXmlRequestModel
                {
                    Xml = xmlFa
                };
                return FaMapper.Map(webCallHandler.ExecuteRequest<FaModel>(faControllerName, "buildfromxml", Method.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzSwtAction BuildSwtActionFromXml(string xml)
        {
            try
            {
                BuildSwtActionFromXmlRequestModel requestBodyObject = new BuildSwtActionFromXmlRequestModel
                {
                    Xml = xml
                };
                return SwtActionMapper.Map(webCallHandler.ExecuteRequest<SwtActionModel>(swtActionControllerName, "buildfromxml", Method.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzTal BuildEmptyTal()
        {
            try
            {
                return TalMapper.Map(webCallHandler.ExecuteRequest<TalModel>(talControllerName, "createempty", Method.Get).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzTal BuildTalFromXml(string xmlTal)
        {
            try
            {
                BuildTalFromXmlRequestModel requestBodyObject = new BuildTalFromXmlRequestModel
                {
                    Xml = xmlTal
                };
                return TalMapper.Map(webCallHandler.ExecuteRequest<TalModel>(talControllerName, "buildfromxml", Method.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzTal UpdateTalEcus(IPsdzTal psdzTal, IEnumerable<IPsdzEcuIdentifier> installedEcuListIst, IEnumerable<IPsdzEcuIdentifier> installedEcuListSoll)
        {
            try
            {
                UpdateEcusRequestModel requestBodyObject = new UpdateEcusRequestModel
                {
                    Tal = TalMapper.Map(psdzTal),
                    InstalledEcuListIst = installedEcuListIst.Select(EcuIdentifierMapper.Map).ToList(),
                    InstalledEcuListSoll = installedEcuListSoll.Select(EcuIdentifierMapper.Map).ToList()
                };
                return TalMapper.Map(webCallHandler.ExecuteRequest<TalModel>(talControllerName, "updateecus", Method.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzTal SetPreferredFlashProtocol(IPsdzTal tal, IPsdzEcuIdentifier ecu, PsdzProtocol psdzProtocol)
        {
            try
            {
                SetPreferredFlashProtocolRequestModel requestBodyObject = new SetPreferredFlashProtocolRequestModel
                {
                    Tal = TalMapper.Map(tal),
                    Ecu = EcuIdentifierMapper.Map(ecu),
                    Protocol = protocolMapper.GetValue(psdzProtocol)
                };
                return TalMapper.Map(webCallHandler.ExecuteRequest<TalModel>(talControllerName, "setpreferredflashprotocol", Method.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzTalFilter BuildEmptyTalFilter()
        {
            try
            {
                return TalFilterMapper.Map(webCallHandler.ExecuteRequest<TalFilterModel>(talFilterControllerName, "createempty", Method.Get).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzTalFilter DefineFilterForAllEcus(PsdzTaCategories[] psdzTaCategories, PsdzTalFilterAction talFilterAction, IPsdzTalFilter filter)
        {
            try
            {
                DefineFilterForAllEcusRequestModel requestBodyObject = new DefineFilterForAllEcusRequestModel
                {
                    TaCategories = psdzTaCategories?.Select(taCategoryTypeMapper.GetValue)?.ToList(),
                    TalfilterAction = talFilterActionMapper.GetValue(talFilterAction),
                    InputTalFilter = TalFilterMapper.Map(filter)
                };
                return TalFilterMapper.Map(webCallHandler.ExecuteRequest<TalFilterModel>(talFilterControllerName, "defineforallecus", Method.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzTalFilter DefineFilterForSelectedEcus(PsdzTaCategories[] psdzTaCategories, int[] diagAddress, PsdzTalFilterAction talFilterAction, IPsdzTalFilter filter, IDictionary<string, PsdzTalFilterAction> smacFilter = null)
        {
            try
            {
                Dictionary<string, ActionValues> smacFilter2 = null;
                if (smacFilter != null)
                {
                    smacFilter2 = smacFilter.ToDictionary((KeyValuePair<string, PsdzTalFilterAction> x) => x.Key, (KeyValuePair<string, PsdzTalFilterAction> y) => talFilterActionMapper.GetValue(y.Value));
                }
                DefineFilterForSelectedEcusRequestModel requestBodyObject = new DefineFilterForSelectedEcusRequestModel
                {
                    TaCategories = psdzTaCategories.Select(taCategoryTypeMapper.GetValue).ToList(),
                    DiagAddress = diagAddress,
                    TalfilterAction = talFilterActionMapper.GetValue(talFilterAction),
                    InputTalFilter = TalFilterMapper.Map(filter),
                    SmacFilter = smacFilter2
                };
                return TalFilterMapper.Map(webCallHandler.ExecuteRequest<TalFilterModel>(talFilterControllerName, "defineforselectedecus", Method.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzTalFilter DefineFilterForSwes(int diagAddress, PsdzTalFilterAction talFilterAction, PsdzTaCategories category, IList<IPsdzSweTalFilterOptions> sweTaFilters, IPsdzTalFilter filter)
        {
            try
            {
                DefineFilterForSwesRequestModel requestBodyObject = new DefineFilterForSwesRequestModel
                {
                    DiagAddress = diagAddress,
                    Filter = TalFilterMapper.Map(filter),
                    TalfilterAction = talFilterActionMapper.GetValue(talFilterAction),
                    TaCategory = taCategoryTypeMapper.GetValue(category),
                    SweFilter = sweTaFilters.Select((IPsdzSweTalFilterOptions x) => new SweTalFilterOptionsModel
                    {
                        ProcessClass = x.ProcessClass,
                        Ta = TaMapper.Map(x.Ta),
                        SweFilter = x.SweFilter.ToDictionary((KeyValuePair<string, PsdzTalFilterAction> key) => key.Key, (KeyValuePair<string, PsdzTalFilterAction> val) => talFilterActionMapper.GetValue(val.Value))
                    }).ToList()
                };
                return TalFilterMapper.Map(webCallHandler.ExecuteRequest<TalFilterModel>(talFilterControllerName, "definefilterforswes", Method.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }
    }
}
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Swt;
using BMW.Rheingold.Psdz.Model.Tal;
using BMW.Rheingold.Psdz.Model.Tal.TalFilter;
using PsdzClientLibrary;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace BMW.Rheingold.Psdz.Client
{
    [PreserveSource(Removed = true)]
    internal class ObjectBuilderServiceClient : PsdzClientBase<IObjectBuilderService>, IObjectBuilderService
    {
        public ObjectBuilderServiceClient(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {
        }

        public IPsdzTalFilter BuildEmptyTalFilter()
        {
            return CallFunction((IObjectBuilderService service) => service.BuildEmptyTalFilter());
        }

        public IPsdzFa BuildFa(IPsdzFa faInput)
        {
            return CallFunction((IObjectBuilderService service) => service.BuildFa(faInput));
        }

        public IPsdzFa BuildFaFromXml(string xml)
        {
            return CallFunction((IObjectBuilderService service) => service.BuildFaFromXml(xml));
        }

        public IPsdzSwtAction BuildSwtActionFromXml(string xml)
        {
            return CallFunction((IObjectBuilderService service) => service.BuildSwtActionFromXml(xml));
        }

        public IPsdzTal BuildTalFromXml(string xml)
        {
            return CallFunction((IObjectBuilderService service) => service.BuildTalFromXml(xml));
        }

        public IPsdzTal BuildEmptyTal()
        {
            return CallFunction((IObjectBuilderService service) => service.BuildEmptyTal());
        }

        public IPsdzTalFilter DefineFilterForAllEcus(PsdzTaCategories[] psdzTaCategories, PsdzTalFilterAction talFilterAction, IPsdzTalFilter filter)
        {
            return CallFunction((IObjectBuilderService service) => service.DefineFilterForAllEcus(psdzTaCategories, talFilterAction, filter));
        }

        public IPsdzTalFilter DefineFilterForSwes(int diagAddress, PsdzTalFilterAction talFilterAction, PsdzTaCategories category, IList<IPsdzSweTalFilterOptions> sweTalFilters, IPsdzTalFilter filter)
        {
            return CallFunction((IObjectBuilderService service) => service.DefineFilterForSwes(diagAddress, talFilterAction, category, sweTalFilters, filter));
        }

        public IPsdzTalFilter DefineFilterForSelectedEcus(PsdzTaCategories[] psdzTaCategories, int[] diagAddress, PsdzTalFilterAction talFilterAction, IPsdzTalFilter filter, IDictionary<string, PsdzTalFilterAction> smacFilter = null)
        {
            return CallFunction((IObjectBuilderService service) => service.DefineFilterForSelectedEcus(psdzTaCategories, diagAddress, talFilterAction, filter, smacFilter));
        }

        public IPsdzTal SetPreferredFlashProtocol(IPsdzTal tal, IPsdzEcuIdentifier ecu, PsdzProtocol psdzProtocol)
        {
            return CallFunction((IObjectBuilderService service) => service.SetPreferredFlashProtocol(tal, ecu, psdzProtocol));
        }

        public IPsdzTal UpdateTalEcus(IPsdzTal psdzTal, IEnumerable<IPsdzEcuIdentifier> installedEcuListIst, IEnumerable<IPsdzEcuIdentifier> installedEcuListSoll)
        {
            return CallFunction((IObjectBuilderService service) => service.UpdateTalEcus(psdzTal, installedEcuListIst, installedEcuListSoll));
        }
    }
}

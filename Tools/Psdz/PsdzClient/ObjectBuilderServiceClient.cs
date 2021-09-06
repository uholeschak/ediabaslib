using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
	class ObjectBuilderServiceClient : PsdzClientBase<IObjectBuilderService>, IObjectBuilderService
	{
		public ObjectBuilderServiceClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
		{
		}

		public IPsdzTalFilter BuildEmptyTalFilter()
		{
			return base.CallFunction<IPsdzTalFilter>((IObjectBuilderService service) => service.BuildEmptyTalFilter());
		}

		public IPsdzFa BuildFa(IPsdzFa faInput)
		{
			return base.CallFunction<IPsdzFa>((IObjectBuilderService service) => service.BuildFa(faInput));
		}

		public IPsdzFa BuildFaFromXml(string xml)
		{
			return base.CallFunction<IPsdzFa>((IObjectBuilderService service) => service.BuildFaFromXml(xml));
		}

		public IPsdzSwtAction BuildSwtActionFromXml(string xml)
		{
			return base.CallFunction<IPsdzSwtAction>((IObjectBuilderService service) => service.BuildSwtActionFromXml(xml));
		}

		public IPsdzTal BuildTalFromXml(string xml)
		{
			return base.CallFunction<IPsdzTal>((IObjectBuilderService service) => service.BuildTalFromXml(xml));
		}

		public IPsdzTal BuildEmptyTal()
		{
			return base.CallFunction<IPsdzTal>((IObjectBuilderService service) => service.BuildEmptyTal());
		}

		public IPsdzTalFilter DefineFilterForAllEcus(PsdzTaCategories[] psdzTaCategories, PsdzTalFilterAction talFilterAction, IPsdzTalFilter filter)
		{
			return base.CallFunction<IPsdzTalFilter>((IObjectBuilderService service) => service.DefineFilterForAllEcus(psdzTaCategories, talFilterAction, filter));
		}

		public IPsdzTalFilter DefineFilterForSelectedEcus(PsdzTaCategories[] psdzTaCategories, int[] diagAddress, PsdzTalFilterAction talFilterAction, IPsdzTalFilter filter)
		{
			return base.CallFunction<IPsdzTalFilter>((IObjectBuilderService service) => service.DefineFilterForSelectedEcus(psdzTaCategories, diagAddress, talFilterAction, filter));
		}

		public IPsdzTal SetPreferredFlashProtocol(IPsdzTal tal, IPsdzEcuIdentifier ecu, PsdzProtocol psdzProtocol)
		{
			return base.CallFunction<IPsdzTal>((IObjectBuilderService service) => service.SetPreferredFlashProtocol(tal, ecu, psdzProtocol));
		}

		public IPsdzTal UpdateTalEcus(IPsdzTal psdzTal, IEnumerable<IPsdzEcuIdentifier> installedEcuListIst, IEnumerable<IPsdzEcuIdentifier> installedEcuListSoll)
		{
			return base.CallFunction<IPsdzTal>((IObjectBuilderService service) => service.UpdateTalEcus(psdzTal, installedEcuListIst, installedEcuListSoll));
		}
	}
}

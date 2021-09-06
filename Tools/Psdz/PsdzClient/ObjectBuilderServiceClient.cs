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
		// Token: 0x06000122 RID: 290 RVA: 0x00002B72 File Offset: 0x00000D72
		public ObjectBuilderServiceClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
		{
		}

		// Token: 0x06000123 RID: 291 RVA: 0x00002B7C File Offset: 0x00000D7C
		public IPsdzTalFilter BuildEmptyTalFilter()
		{
			return base.CallFunction<IPsdzTalFilter>((IObjectBuilderService service) => service.BuildEmptyTalFilter());
		}

		// Token: 0x06000124 RID: 292 RVA: 0x00004CDC File Offset: 0x00002EDC
		public IPsdzFa BuildFa(IPsdzFa faInput)
		{
			return base.CallFunction<IPsdzFa>((IObjectBuilderService service) => service.BuildFa(faInput));
		}

		// Token: 0x06000125 RID: 293 RVA: 0x00004D08 File Offset: 0x00002F08
		public IPsdzFa BuildFaFromXml(string xml)
		{
			return base.CallFunction<IPsdzFa>((IObjectBuilderService service) => service.BuildFaFromXml(xml));
		}

		// Token: 0x06000126 RID: 294 RVA: 0x00004D34 File Offset: 0x00002F34
		public IPsdzSwtAction BuildSwtActionFromXml(string xml)
		{
			return base.CallFunction<IPsdzSwtAction>((IObjectBuilderService service) => service.BuildSwtActionFromXml(xml));
		}

		// Token: 0x06000127 RID: 295 RVA: 0x00004D60 File Offset: 0x00002F60
		public IPsdzTal BuildTalFromXml(string xml)
		{
			return base.CallFunction<IPsdzTal>((IObjectBuilderService service) => service.BuildTalFromXml(xml));
		}

		// Token: 0x06000128 RID: 296 RVA: 0x00002BA3 File Offset: 0x00000DA3
		public IPsdzTal BuildEmptyTal()
		{
			return base.CallFunction<IPsdzTal>((IObjectBuilderService service) => service.BuildEmptyTal());
		}

		// Token: 0x06000129 RID: 297 RVA: 0x00004D8C File Offset: 0x00002F8C
		public IPsdzTalFilter DefineFilterForAllEcus(PsdzTaCategories[] psdzTaCategories, PsdzTalFilterAction talFilterAction, IPsdzTalFilter filter)
		{
			return base.CallFunction<IPsdzTalFilter>((IObjectBuilderService service) => service.DefineFilterForAllEcus(psdzTaCategories, talFilterAction, filter));
		}

		// Token: 0x0600012A RID: 298 RVA: 0x00004DC8 File Offset: 0x00002FC8
		public IPsdzTalFilter DefineFilterForSelectedEcus(PsdzTaCategories[] psdzTaCategories, int[] diagAddress, PsdzTalFilterAction talFilterAction, IPsdzTalFilter filter)
		{
			return base.CallFunction<IPsdzTalFilter>((IObjectBuilderService service) => service.DefineFilterForSelectedEcus(psdzTaCategories, diagAddress, talFilterAction, filter));
		}

		// Token: 0x0600012B RID: 299 RVA: 0x00004E0C File Offset: 0x0000300C
		public IPsdzTal SetPreferredFlashProtocol(IPsdzTal tal, IPsdzEcuIdentifier ecu, PsdzProtocol psdzProtocol)
		{
			return base.CallFunction<IPsdzTal>((IObjectBuilderService service) => service.SetPreferredFlashProtocol(tal, ecu, psdzProtocol));
		}

		// Token: 0x0600012C RID: 300 RVA: 0x00004E48 File Offset: 0x00003048
		public IPsdzTal UpdateTalEcus(IPsdzTal psdzTal, IEnumerable<IPsdzEcuIdentifier> installedEcuListIst, IEnumerable<IPsdzEcuIdentifier> installedEcuListSoll)
		{
			return base.CallFunction<IPsdzTal>((IObjectBuilderService service) => service.UpdateTalEcus(psdzTal, installedEcuListIst, installedEcuListSoll));
		}
	}
}

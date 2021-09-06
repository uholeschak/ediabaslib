using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public enum PsdzTalFilterAction
    {
        AllowedToBeTreated,
        Empty,
        MustBeTreated,
        MustNotBeTreated,
        OnlyToBeTreatedAndBlockCategoryInAllEcu
    }

	[ServiceKnownType(typeof(PsdzFa))]
	[ServiceKnownType(typeof(PsdzTalFilter))]
	[ServiceKnownType(typeof(PsdzSwtAction))]
	[ServiceKnownType(typeof(PsdzEcuIdentifier))]
	[ServiceKnownType(typeof(PsdzTal))]
	[ServiceContract(SessionMode = SessionMode.Required)]
	public interface IObjectBuilderService
	{
		// Token: 0x06000056 RID: 86
		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzTalFilter BuildEmptyTalFilter();

		// Token: 0x06000057 RID: 87
		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzFa BuildFa(IPsdzFa faInput);

		// Token: 0x06000058 RID: 88
		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzFa BuildFaFromXml(string xml);

		// Token: 0x06000059 RID: 89
		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzSwtAction BuildSwtActionFromXml(string xml);

		// Token: 0x0600005A RID: 90
		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzTal BuildTalFromXml(string xml);

		// Token: 0x0600005B RID: 91
		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzTal BuildEmptyTal();

		// Token: 0x0600005C RID: 92
		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzTalFilter DefineFilterForAllEcus(PsdzTaCategories[] psdzTaCategories, PsdzTalFilterAction talFilterAction, IPsdzTalFilter filter);

		// Token: 0x0600005D RID: 93
		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzTalFilter DefineFilterForSelectedEcus(PsdzTaCategories[] psdzTaCategories, int[] diagAddress, PsdzTalFilterAction talFilterAction, IPsdzTalFilter filter);

		// Token: 0x0600005E RID: 94
		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzTal UpdateTalEcus(IPsdzTal psdzTal, IEnumerable<IPsdzEcuIdentifier> installedEcuListIst, IEnumerable<IPsdzEcuIdentifier> installedEcuListSoll);

		// Token: 0x0600005F RID: 95
		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzTal SetPreferredFlashProtocol(IPsdzTal tal, IPsdzEcuIdentifier ecu, PsdzProtocol psdzProtocol);
	}
}

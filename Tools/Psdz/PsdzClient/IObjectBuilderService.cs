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
		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzTalFilter BuildEmptyTalFilter();

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzFa BuildFa(IPsdzFa faInput);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzFa BuildFaFromXml(string xml);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzSwtAction BuildSwtActionFromXml(string xml);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzTal BuildTalFromXml(string xml);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzTal BuildEmptyTal();

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzTalFilter DefineFilterForAllEcus(PsdzTaCategories[] psdzTaCategories, PsdzTalFilterAction talFilterAction, IPsdzTalFilter filter);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzTalFilter DefineFilterForSelectedEcus(PsdzTaCategories[] psdzTaCategories, int[] diagAddress, PsdzTalFilterAction talFilterAction, IPsdzTalFilter filter);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzTal UpdateTalEcus(IPsdzTal psdzTal, IEnumerable<IPsdzEcuIdentifier> installedEcuListIst, IEnumerable<IPsdzEcuIdentifier> installedEcuListSoll);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzTal SetPreferredFlashProtocol(IPsdzTal tal, IPsdzEcuIdentifier ecu, PsdzProtocol psdzProtocol);
	}
}

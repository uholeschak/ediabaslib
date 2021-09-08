using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Core;
using PsdzClient.Programming;

namespace PsdzClient.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
	public interface IAPISecurity
	{
		string GetRefurbishFSCFromWebService(string appNo, string upIdx);

		bool WriteRefurbishFSCToECU(string appNo, string upIdx);

		[Obsolete("Function is Obsolete and will be deleted. Please use NavigationMapProcessor.GetNavigationMapsForExistingFSCs")]
		List<INavFSCProvided> GetNavigationMapsForExistingFSCs(string svin, string[] dealerNumbers);

		[Obsolete("Function is Obsolete and will be deleted. Please use NavigationMapProcessor.SaveFscLocally")]
		bool SaveFscLocally(IFSCProvided fsc);

		[Obsolete("Function is Obsolete and will be deleted. Please use NavigationMapProcessor.AreNavigationFscsAvailable")]
		bool AreNavigationFscsAvailable(string svin, string[] dealerNumbers);

		[Obsolete("Function is Obsolete and will be deleted. Please use NavigationMapProcessor.GetNavigationFscs")]
		List<IFSCProvided> GetNavigationFscs(string svin, string[] dealerNumbers);

		[Obsolete("Function is Obsolete and will be deleted. Please use NavigationMapProcessor.SaveFSCsLocally")]
		bool SaveFSCsLocally(List<IFSCProvided> fscList);

		[Obsolete("Function is Obsolete and will be deleted. Please use NavigationMapProcessor.SaveNavigationFSCsLocally")]
		bool SaveNavigationFSCsLocally(string svin, string[] dealerNumbers);

		IBoolResultObject AreEcuValidationCertificatesValid();

		IBoolResultObject AutomaticRenewEcuValidationResult();

		IBoolResultObject IsEcuValidationServerOnlineResult();

		IBoolResultObject ManualRenewEcuValidation();

		IBoolResultObject ManualRequestEcuValidation();

		[Obsolete("Deprecated. This method will not be available anymore from 4.30")]
		IBoolResultObject TryAutomaticActivateAllSecureFeaturesOffline();

		[Obsolete("Deprecated. This method will not be available anymore from 4.30")]
		IBoolResultObject TryCreateSecureTokenRequestOffline();

		[Obsolete("Deprecated. This method will not be available anymore from 4.30")]
		IBoolResultObject TryManualActivateAllSecureFeaturesOffline();

		[Obsolete("Deprecated. This method will not be available anymore from 4.30")]
		IBoolResultObject IsSecureFeatureActivationServerOnline();

		IList<IBoolResultObject> GetListLastErrors();

		IBoolResultObject GetLastErrorOfContext(string contextError);

		IList<string> GetListLastErrors(string errorCode);

		IList<IBoolResultObject> GetBackendStatus();

		IList<IBoolResultObject> GetBackendStatus(string context);
	}
}

using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IAPISecurity
    {
        string GetRefurbishFSCFromWebService(string appNo, string upIdx);
        bool WriteRefurbishFSCToECU(string appNo, string upIdx);
        IBoolResultObject StartCertificateManagement();
        IBoolResultObject AreEcuValidationCertificatesValid();
        IFetchEcuCertCheckingResult GetFetchEcuCertCheckingResult();
        IBoolResultObject<IFetchEcuCertCheckingResult> AreEcuValidationCertificatesValidWithFetchResult();
        IBoolResultObject AutomaticRenewEcuValidationResult();
        IBoolResultObject AutomaticRenewEcuValidationSpecial();
        IBoolResultObject IsEcuValidationServerOnlineResult();
        IBoolResultObject ManualRenewEcuValidation();
        IBoolResultObject ManualRequestEcuValidation();
        IBoolResultObject StartSecureFeatureActivationManagement();
        IBoolResultObject RemoveAllSFA();
        IBoolResultObject SetFeatureIdListSFA(IList<long> featureIds, bool isWhitelist);
        IList<long> GetFeatureIdListSFA(bool isWhiteList);
        IBoolResultObject RemoveSFAByDiagnosisAddress(IList<int> diagnosisAddressList);
        IBoolResultObject RebuildSFATokenPackage();
        IBoolResultObject StartSecureCodingManagement();
        IList<IBoolResultObject> GetListLastErrors();
        IBoolResultObject GetLastErrorOfContext(string contextError);
        IList<string> GetListLastErrors(string errorCode);
        IList<IBoolResultObject> GetBackendStatus();
        IList<IBoolResultObject> GetBackendStatus(string context);
        IBoolResultObject StartActivateHeadUnit();
    }
}
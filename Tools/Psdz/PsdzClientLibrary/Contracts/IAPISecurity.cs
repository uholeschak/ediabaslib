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
        IBoolResultObject StartCertificateManagement();
        IBoolResultObject AreEcuValidationCertificatesValid();
        IFetchEcuCertCheckingResult GetFetchEcuCertCheckingResult();
        IBoolResultObject<IFetchEcuCertCheckingResult> AreEcuValidationCertificatesValidWithFetchResult();
        IBoolResultObject AutomaticRenewEcuValidationResult();
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
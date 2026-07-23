using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.Psdz.Model.Sfa;
using PsdzClient.Contracts;
using PsdzClient.Core;
using PsdzClient.Programming;
using System.Collections.Generic;
using BMW.Rheingold.CoreFramework.AutomotiveSecurity;

namespace BMW.Rheingold.CoreFramework
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IComponentTheftProtectionService
    {
        Dictionary<int, IBoolResultObject> GetKdsClientsIdsForRefurbish(int retries, int timeBetweenRetries);

        IReadPublicKeyResult ReadPublicKey(int kdsId, int retries, int timeBetweenRetries);

        IBoolResultObject GenerateSecureTokenAndPerfromRefurbishProcess(string hexECUadresse, long featureid, int kdsid, IList<IFeatureSpecificField> featureSpecificFields, IList<IValidityCondition> validityConditions, int enableType, int retries, int timeBetweenRetries);

        IBoolResultObject GenerateSecureTokenAndSaveLocallyWithoutBackendRequest(string hexEcuAddress, long featureId, int kdsId, IList<IFeatureSpecificField> featureSpecificFields, IList<IValidityCondition> validityConditions, int enableType);

        IBoolResultObject PerformRefurbishProcess(int kdsId, IPsdzSecureTokenEto secureToken, int retries, int timeBetweenRetries);

        bool PerformQuickKdsCheck(int kdsId, int retries, int timeBetweenRetries);

        bool PerformQuickKdsCheckSP25(int kdsId, int retries = 3, int timeBetweenRetries = 10000);

        bool SwitchOnComponentTheftProtection(int kdsId, int retries, int timeBetweenRetries);

        IBoolResultObject GenerateSecureTokenRequestZip_KDS();

        IBoolResultObject ClearTokenFiles_KDS();

        IKdsCollectionResult StartComponentTheftProtectionOfflineSimplifiedProcess(int retries, int timeBetweenRetries);

        IBoolResultObject StartComponentTheftProtectionOfflineProcess();
    }
}

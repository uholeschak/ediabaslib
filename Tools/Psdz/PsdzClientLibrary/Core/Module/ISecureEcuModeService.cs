using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.Psdz;
using PsdzClient.Contracts;
using PsdzClient.Core;
using PsdzClient.Programming;
using System.Collections.Generic;
using BMW.Rheingold.CoreFramework.AutomotiveSecurity;

namespace BMW.Rheingold.CoreFramework
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISecureEcuModeService
    {
        IBoolResultObject WriteSecureTokensAutomatic(string hexEcuAddress, long featureId, IList<IFeatureSpecificField> featureSpecificFields, IList<IValidityCondition> validityConditions, int enableType);

        IList<IFeatureStatusResult> WriteSecureTokensAutomatic(IList<ISecureTokenMetaObject> stmoList);

        IList<IFeatureStatusResult> DiscoverAllFeaturesStatus();

        IList<IFeatureStatusResult> DeleteSecureTokens(IList<ISecureTokenMetaObject> stmoList);

        Dictionary<long, IBoolResultObject> DeleteSecureTokens(string hexEcuAddress, List<long> featureIds);

        IBoolResultObject GenerateSecureTokenRequestFile(string hexEcuAddress, long featureId, IList<IFeatureSpecificField> featureSpecificFields, IList<IValidityCondition> validityConditions, int enableType);

        IBoolResultObject GenerateSecureTokenRequestFileForVehicle(IList<ISecureTokenMetaObject> secureTokenMetaObjects, bool isInBackground);

        EcuMode GetECUMode(IEcuIdentifier ecuIdentifier);

        IList<IFeatureStatusResult> RequestTokenStatus(IList<ISecureTokenMetaObject> stmoList);

        IList<IFeatureStatusResult> RequestTokenStatus(string hexEcuAddress, List<long> featureID);

        bool SwitchECUToFieldMode(IEcuIdentifier ecuIdentifier);

        IBoolResultObject GenerateSecureTokenRequestZip_SecureToken();

        IBoolResultObject GenerateSecureTokenRequestZipInSubFolder(string folderName);

        IBoolResultObject ClearTokenFiles_SecureTokens();

        BoolResultObject<string> RebuildTokenPackageAndCalculateMP();

        IBoolResultObject WriteSfaNewFeatureForVehicleAutomatic(string hexEcuAddress, long featureId, IList<IFeatureSpecificField> featureSpecificFields, IList<IValidityCondition> validityConditions, int enableType);

        IBoolResultObject WriteSfaNewFeatureForVehicleAutomatic(IList<ISecureTokenMetaObject> secureTokenMetaObjects);

        IBoolResultObject GenerateSecureTokenForMapInBackend(string hexEcuAddress, long featureId, IList<IFeatureSpecificField> featureSpecificFields, IList<IValidityCondition> validityConditions, int enableType);

        IBoolResultObject GenerateSecureTokensInBackend(IList<ISecureTokenMetaObject> secureTokenMetaObjects);

        ISecureTokenMetaObject CreateSecureTokenMetaObject(string hexEcuAddress, long featureId, IList<IFeatureSpecificField> featureSpecificFields, IList<IValidityCondition> validityConditions, int enableType, bool addDummyFeatureSpecificField);

        IBoolResultObject DownloadAndActivateTokens(List<long> featureIdsToActivate = null);

        IBoolResultObject DownloadAndActivateTokens(List<long> featureIdsToActivate, bool rebuildTokens);

        IBoolResultObject<IProgrammingProtectionTokenResult> WriteProgrammingProtectionTokens(List<long> featureIdsToActivate);

        IBoolResultObject<IProgrammingProtectionTokenResult> WriteProgrammingProtectionTokens(List<long> featureIdsToActivate, List<int> blackListOfECUs);

        IBoolResultObject<IProgrammingProtectionTokenResult> GenerateProgrammingProtectionTokenRequestFile(List<long> featureIdsToActivate);

        IBoolResultObject<IProgrammingProtectionTokenResult> GenerateProgrammingProtectionTokenRequestFile(List<long> featureIdsToActivate, List<int> blackListOfECUs);

        IBoolResultObject<IProgrammingProtectionTokenResult> ImportProgrammingProtectionTokens();

        IBoolResultObject<IProgrammingProtectionTokenResult> ImportProgrammingProtectionTokens(List<int> blackListOfECUs);

        IBoolResultObject ImportSecureToken();

        IBoolResultObject<IEcuFailureResponseSet> ResetEcus(List<string> hexEcuAddress);

        IBoolResultObject<IEcuFailureResponseSet> PerformEcuSwitchResetWithFlashMode(List<string> hexEcuAddress, List<EcuResetMapping> ecusToBeReset, bool performWithFlashMode);
    }
}

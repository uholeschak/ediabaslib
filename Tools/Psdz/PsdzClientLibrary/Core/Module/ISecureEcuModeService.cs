using System;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.Psdz;
using PsdzClient.Core;
using PsdzClient.Programming;
using System.Collections.Generic;
using BMW.Rheingold.CoreFramework.AutomotiveSecurity;
using BMW.Rheingold.CoreFramework.Contracts;

namespace BMW.Rheingold.CoreFramework
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISecureEcuModeService
    {
        [Obsolete("This API-Function is deprecated. Please use 'WriteSecureTokensAutomatic(IList<ISecureTokenMetaObject>)' instead.")]
        IBoolResultObject WriteSecureTokensAutomatic(string hexEcuAddress, long featureId, IList<IFeatureSpecificField> featureSpecificFields, IList<IValidityCondition> validityConditions, int enableType);
        IList<IFeatureStatusResult> WriteSecureTokensAutomatic(IList<ISecureTokenMetaObject> stmoList);
        IBoolResultObject WriteSecureTokensAutomaticToOBDFirewall(ISecureTokenMetaObject secureTokenMetaObject);
        IList<IFeatureStatusResult> DiscoverAllFeaturesStatus();
        IList<IFeatureStatusResult> DeleteSecureTokens(IList<ISecureTokenMetaObject> stmoList);
        [Obsolete("This API-Function is deprecated. Please use 'DeleteSecureTokens(IList<ISecureTokenMetaObject>)' instead.")]
        Dictionary<long, IBoolResultObject> DeleteSecureTokens(string hexEcuAddress, List<long> featureIds);
        IBoolResultObject GenerateSecureTokenRequestFile(string hexEcuAddress, long featureId, IList<IFeatureSpecificField> featureSpecificFields, IList<IValidityCondition> validityConditions, int enableType);
        IBoolResultObject GenerateSecureTokenRequestFileForVehicle(IList<ISecureTokenMetaObject> secureTokenMetaObjects, bool isInBackground);
        EcuMode GetECUMode(IEcuIdentifier ecuIdentifier);
        IList<IFeatureStatusResult> RequestTokenStatus(IList<ISecureTokenMetaObject> stmoList);
        [Obsolete("This API-Function is deprecated. Please use 'RequestTokenStatus(IList<ISecureTokenMetaObject>)' instead.")]
        IList<IFeatureStatusResult> RequestTokenStatus(string hexEcuAddress, List<long> featureID);
        IList<IFeatureStatusResult> RequestTokenStatusUsingActualSvt(string hexEcuAddress, List<long> featureID);
        bool SwitchECUToFieldMode(IEcuIdentifier ecuIdentifier);
        IBoolResultObject GenerateSecureTokenRequestZip_SecureToken();
        IBoolResultObject GenerateSecureTokenRequestZipInSubFolder(string folderName);
        IBoolResultObject ClearTokenFiles_SecureTokens();
        BoolResultObject<string> RebuildTokenPackageAndCalculateMP();
        [Obsolete("This API-Function is deprecated. Please use 'WriteSfaNewFeatureForVehicleAutomatic(IList<ISecureTokenMetaObject>)' instead.")]
        IBoolResultObject WriteSfaNewFeatureForVehicleAutomatic(string hexEcuAddress, long featureId, IList<IFeatureSpecificField> featureSpecificFields, IList<IValidityCondition> validityConditions, int enableType);
        IBoolResultObject WriteSfaNewFeatureForVehicleAutomatic(IList<ISecureTokenMetaObject> secureTokenMetaObjects);
        IBoolResultObject GenerateSecureTokenForMapInBackend(string hexEcuAddress, long featureId, IList<IFeatureSpecificField> featureSpecificFields, IList<IValidityCondition> validityConditions, int enableType);
        IBoolResultObject GenerateSecureTokensInBackend(IList<ISecureTokenMetaObject> secureTokenMetaObjects);
        ISecureTokenMetaObject CreateSecureTokenMetaObject(string hexEcuAddress, long featureId, IList<IFeatureSpecificField> featureSpecificFields, IList<IValidityCondition> validityConditions, int enableType, bool addDummyFeatureSpecificField);
        IBoolResultObject DownloadAndActivateTokens(List<long> featureIdsToActivate = null);
        IBoolResultObject DownloadAndActivateTokens(List<long> featureIdsToActivate, bool rebuildTokens);
        IBoolResultObject<IProgrammingProtectionTokenResult> WriteProgrammingProtectionTokens(List<long> featureIdsToActivate);
        IBoolResultObject<IProgrammingProtectionTokenResult> WriteProgrammingProtectionTokens(List<long> featureIdsToActivate, List<int> blackListOfECUs);
        IBoolResultObject<IProgrammingProtectionTokenResult> GenerateSweListForProgrammingProtection();
        IBoolResultObject<IProgrammingProtectionTokenResult> GenerateProgrammingProtectionTokenRequestFile(List<long> featureIdsToActivate);
        IBoolResultObject<IProgrammingProtectionTokenResult> GenerateProgrammingProtectionTokenRequestFile(List<long> featureIdsToActivate, List<int> blackListOfECUs);
        IBoolResultObject<IProgrammingProtectionTokenResult> ImportProgrammingProtectionTokens();
        IBoolResultObject<IProgrammingProtectionTokenResult> ImportProgrammingProtectionTokens(List<int> blackListOfECUs);
        IBoolResultObject ImportSecureToken();
        IBoolResultObject ImportSecureTokenToOBDFirewall();
        IBoolResultObject<IEcuFailureResponseSet> ResetEcus(List<string> hexEcuAddress);
        IBoolResultObject<IEcuFailureResponseSet> PerformEcuSwitchResetWithFlashMode(List<string> hexEcuAddress, List<EcuResetMapping> ecusToBeReset, bool performWithFlashMode);
    }
}

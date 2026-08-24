using BMW.Authoring;
using BMW.Authoring.API.Implementation.Sfa.Models.Request;
using BMW.Authoring.API.Interface.Sfa.Models;
using PsdzClient.Contracts;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using BMW.Authoring.API.Implementation.Sfa.Models;

namespace BMW.Authoring.API.Interface.Sfa
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface ISfaHandler : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        ISfaFeature CreatePsdzFeature(string hexEcuAdr, string hexEcuUid, string hexFeatureId, int enabledType, Dictionary<int, string> featureSpecificFieldList, Dictionary<SfaValidityConditionType, string> validityConditionList);

        [Obsolete("Use GetNewestPackageForVehicle(bool rebuildToken) instead.")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        ISfaTokenPackageResponse GetNewestPackageForVehicle(string vin, bool rebuildToken);

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        ISfaTokenPackageResponse GetNewestPackageForVehicle(bool rebuildToken);

        [Obsolete("Use GetTokenDirect(List<ISfaFeature> sfaFeatures) instead.")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        ISfaTokenPackageResponse GetTokenDirect(string vin, List<ISfaFeature> sfaFeatures);

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        ISfaTokenPackageResponse GetTokenDirect(List<ISfaFeature> sfaFeatures);

        [Obsolete("Use WriteSecureTokensAutomatic(ISfaFeature sfaFeature) instead.")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IBoolResultObject WriteSecureTokensAutomatic(string vin, ISfaFeature sfaFeature);

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IBoolResultObject WriteSecureTokensAutomatic(ISfaFeature sfaFeature);
    }
}

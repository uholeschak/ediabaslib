using PsdzClient.Core;
using System.Collections.Generic;
using System.ComponentModel;
using BMW.Authoring.API.Implementation.Sfa.Models.Request;

namespace BMW.Authoring.API.Interface.Sfa.Models
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface ISfaFeature
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string DiagnosisAddress { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string ECU_UID { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        int EnableType { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Feature { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        List<SfaFeatureSpecificField> FeatureSpecificFields { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        List<SfaValidityCondition> ValidityConditions { get; }
    }
}

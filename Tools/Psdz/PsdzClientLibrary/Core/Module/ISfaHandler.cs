using BMW.Authoring;
using PsdzClient.Contracts;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using PsdzClient;

namespace PsdzClientLibrary.Core.Module;

[AuthorAPI(SelectableTypeDeclaration = true)]
[EditorBrowsable(EditorBrowsableState.Advanced)]
[PreserveSource(Hint = "No update", SuppressWarning = true)]
public interface ISfaHandler : IHideObjectMembers
{
    [Obsolete("Use WriteSecureTokensAutomatic(ISfaFeature sfaFeature) instead.")]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    IBoolResultObject WriteSecureTokensAutomatic(string vin, ISfaFeature sfaFeature);

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    IBoolResultObject WriteSecureTokensAutomatic(ISfaFeature sfaFeature);
}

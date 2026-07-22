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
}

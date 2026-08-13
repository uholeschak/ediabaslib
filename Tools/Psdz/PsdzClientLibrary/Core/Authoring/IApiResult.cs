using PsdzClient.Core;
using System;
using System.ComponentModel;

namespace BMW.Authoring.API
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IApiResult : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        bool State { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string ErrorCode { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string ErrorMessage { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Context { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        DateTime Time { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        int StatusCode { get; }
    }
}

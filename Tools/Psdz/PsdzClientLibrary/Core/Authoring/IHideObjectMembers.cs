using System;
using System.ComponentModel;
using BMW.Rheingold.CoreFramework;

#pragma warning disable CS0109
namespace BMW.Authoring
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IHideObjectMembers
    {
        [AuthorAPIHidden]
        [EditorBrowsable(EditorBrowsableState.Never)]
        new bool Equals(object obj);

        [AuthorAPIHidden]
        [EditorBrowsable(EditorBrowsableState.Never)]
        new int GetHashCode();

        [AuthorAPIHidden]
        [EditorBrowsable(EditorBrowsableState.Never)]
        new string ToString();

        [AuthorAPIHidden]
        [EditorBrowsable(EditorBrowsableState.Never)]
        new Type GetType();
    }
}

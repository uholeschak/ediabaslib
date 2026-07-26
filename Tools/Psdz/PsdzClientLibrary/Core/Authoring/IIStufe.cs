using BMW.Authoring;
using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public interface IIStufe : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        string IStufeHO { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string IStufeWerk { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool IStufeHO_Equals(int YYMMIII, params int[] YYMMIII_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool IStufeHO_Equals(int[] YYMMIII);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool IStufeHO_GreaterThan(int YYMMIII);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool IStufeHO_GreaterThanOrEqual(int YYMMIII);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool IStufeHO_LowerThan(int YYMMIII);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool IStufeHO_LowerThanOrEqual(int YYMMIII);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool IStufeWerk_Equals(int YYMMIII, params int[] YYMMIII_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool IStufeWerk_Equals(int[] YYMMIII);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool IStufeWerk_GreaterThan(int YYMMIII);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool IStufeWerk_LowerThan(int YYMMIII);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool IStufeWerk_GreaterThanOrEqual(int YYMMIII);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool IStufeWerk_LowerThanOrEqual(int YYMMIII);
    }
}

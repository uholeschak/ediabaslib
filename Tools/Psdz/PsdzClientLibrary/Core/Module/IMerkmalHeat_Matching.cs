using BMW.Authoring;
using BMW.Authoring.Vehicle;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IMerkmalHeat_Matching : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Result { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        IMerkmalHeat_Matching And(MerkmalHeat merkmal, string wert, params string[] wert_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IMerkmalHeat_Matching And(MerkmalHeat merkmal, string[] wert);
    }
}

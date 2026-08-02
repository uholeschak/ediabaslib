using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public enum SvkProzessklasse
    {
        HWEL = 1,
        HWAP = 2,
        HWFR = 3,
        GWTB = 4,
        CAFD = 5,
        BTLD = 6,
        FLSL = 7,
        SWFL = 8,
        SWFF = 9,
        SWPF = 10,
        ONPS = 11,
        IBAD = 12,
        SWFK = 13,
        FAFP = 15,
        TLRT = 26,
        TPRG = 27,
        FCFA = 16,
        BLUP = 28,
        FLUP = 29,
        SWUP = 192,
        SWIP = 193,
        ENTD = 160,
        NAVD = 161,
        FCFN = 162,
        _ = 255
    }
}

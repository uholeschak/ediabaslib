using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public enum FehlerklasseWert
    {
        KeineFehlerklasse = 0,
        Zustandsanzeige = 0x10,
        Konfigurationsfehler = 0x20,
        InbetriebnahmeUndKalibrierfehler = 0x40,
        ElektrischeDiagnoseUndSpannungsversorgung = 0x80,
        PlausibilitätUndFunktionaleDiagnose = 0x100,
        EcuHardwarefehler = 0x200,
        EcuSoftwarefehler = 0x400,
        Funktionszustandsfehler = 0x800,
        BusdiagnosePhysikalisch = 0x1000,
        SubBusFehler = 0x2000,
        BotschaftsTimeOut = 0x4000,
        SignalUngültigOderUndefiniert = 0x8000,
        BotschaftsCrcOderAlive = 0x10000,
        KundenFehlbedienung = 0x20000,
        UmwelteinflüsseUndSystemgrenzen = 0x40000,
        Infrastruktureinflüsse = 0x80000
    }
}

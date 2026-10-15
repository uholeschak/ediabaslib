using BMW.Rheingold.CoreFramework.Localization;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider.EcuClique
{
    public sealed class InvalidEcuClique : XEP_ECUCLIQUES
    {
        public override string Title => FormatedData.Localize("Unknown", "ISTAGui", false);

        public override bool IsValid => false;
    }
}

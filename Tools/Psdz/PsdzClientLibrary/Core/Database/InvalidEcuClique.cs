using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public sealed class InvalidEcuClique : XEP_ECUCLIQUES
    {
        public override string Title => FormatedData.Localize("Unknown", "ISTAGui", false);

        public override bool IsValid => false;
    }
}

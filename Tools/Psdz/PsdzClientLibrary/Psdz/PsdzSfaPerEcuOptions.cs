using BMW.Rheingold.Psdz.Model.Tal.TalFilter;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal.TalFilter
{
    public class PsdzSfaPerEcuOptions : IPsdzSfaPerEcuOptions
    {
        public PsdzTalFilterAction CategoryAction { get; set; }

        public PsdzTalFilterAction SfaWriteAction { get; set; }

        public PsdzTalFilterAction SfaDeleteAction { get; set; }
    }
}

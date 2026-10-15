namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal.TalFilter
{
    public interface IPsdzSfaPerEcuOptions
    {
        PsdzTalFilterAction CategoryAction { get; set; }

        PsdzTalFilterAction SfaWriteAction { get; set; }

        PsdzTalFilterAction SfaDeleteAction { get; set; }
    }
}

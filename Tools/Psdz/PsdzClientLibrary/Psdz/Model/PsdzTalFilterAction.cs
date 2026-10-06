namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal.TalFilter
{
    public enum PsdzTalFilterAction
    {
        AllowedToBeTreated,
        Empty,
        MustBeTreated,
        MustNotBeTreated,
        OnlyToBeTreatedAndBlockCategoryInAllEcu
    }
}
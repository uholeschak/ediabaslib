using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Tal.TalFilter;
using PsdzClient.Core;

namespace BMW.Rheingold.Psdz.Mapper
{
    public static class TalFilterOptionMapper
    {
        public static PsdzTalFilterAction Map(TalFilterOptions input)
        {
            switch (input)
            {
                case TalFilterOptions.Allowed:
                    return PsdzTalFilterAction.AllowedToBeTreated;
                case TalFilterOptions.Empty:
                    return PsdzTalFilterAction.Empty;
                case TalFilterOptions.Must:
                    return PsdzTalFilterAction.MustBeTreated;
                case TalFilterOptions.MustNot:
                    return PsdzTalFilterAction.MustNotBeTreated;
                case TalFilterOptions.Only:
                    return PsdzTalFilterAction.OnlyToBeTreatedAndBlockCategoryInAllEcu;
                default:
                    Log.Warning(Log.CurrentMethod(), string.Format("Unsupported {0} value: {1}.", "TalFilterOptions", input));
                    return PsdzTalFilterAction.Empty;
            }
        }
    }
}

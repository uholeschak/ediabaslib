using RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal.TalFilter;

namespace BMW.Rheingold.Psdz
{
    internal static class SfaPerEcuOptionsMapper
    {
        internal static SfaPerEcuOptionsModel Map(IPsdzSfaPerEcuOptions input)
        {
            TalFilterActionMapper talFilterActionMapper = new TalFilterActionMapper();
            if (input != null)
            {
                return new SfaPerEcuOptionsModel
                {
                    CategoryAction = talFilterActionMapper.GetValue(input.CategoryAction),
                    SfaWriteAction = talFilterActionMapper.GetValue(input.SfaWriteAction),
                    SfaDeleteAction = talFilterActionMapper.GetValue(input.SfaDeleteAction)
                };
            }
            return null;
        }
    }
}

using BMW.Rheingold.Psdz.Model;

namespace BMW.Rheingold.Psdz
{
    internal static class ILevelMapper
    {
        public static IPsdzIstufe Map(ILevelModel model)
        {
            if (model == null)
            {
                return null;
            }
            return new PsdzIstufe
            {
                IsValid = model.Valid,
                Value = model.Value
            };
        }

        public static ILevelModel Map(IPsdzIstufe psdzILevel)
        {
            if (psdzILevel == null)
            {
                return null;
            }
            return new ILevelModel
            {
                Valid = psdzILevel.IsValid,
                Value = psdzILevel.Value
            };
        }
    }
}
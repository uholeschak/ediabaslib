using BMW.Rheingold.Psdz.Model;

namespace BMW.Rheingold.Psdz
{
    internal static class FpMapper
    {
        public static IPsdzFp Map(FpModel model)
        {
            if (model == null)
            {
                return null;
            }
            IPsdzStandardFp psdzStandardFp = StandardFpMapper.Map(model);
            return new PsdzFp
            {
                Baureihenverbund = model.Baureihenverbund,
                Entwicklungsbaureihe = model.Entwicklungsbaureihe,
                AsString = model.AsString,
                Category2Criteria = psdzStandardFp.Category2Criteria,
                CategoryId2CategoryName = psdzStandardFp.CategoryId2CategoryName,
                IsValid = psdzStandardFp.IsValid
            };
        }

        public static FpModel Map(IPsdzFp psdzFp)
        {
            if (psdzFp == null)
            {
                return null;
            }
            StandardFpModel standardFpModel = StandardFpMapper.Map(psdzFp);
            return new FpModel
            {
                AsString = psdzFp.AsString,
                IsValid = psdzFp.IsValid,
                Entwicklungsbaureihe = psdzFp.Entwicklungsbaureihe,
                Baureihenverbund = psdzFp.Baureihenverbund,
                Category2Criteria = standardFpModel.Category2Criteria,
                CategoryId2CategoryName = standardFpModel.CategoryId2CategoryName
            };
        }
    }
}
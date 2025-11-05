using BMW.Rheingold.Psdz.Model;

namespace BMW.Rheingold.Psdz
{
    internal static class StandardFpCriterionMapper
    {
        public static IPsdzStandardFpCriterion Map(StandardFpCriterionModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzStandardFpCriterion
            {
                Name = model.Name,
                NameEn = model.NameEn,
                Value = model.Value
            };
        }

        public static StandardFpCriterionModel Map(IPsdzStandardFpCriterion psdzStandardFpCriterion)
        {
            if (psdzStandardFpCriterion == null)
            {
                return null;
            }

            return new StandardFpCriterionModel
            {
                Name = psdzStandardFpCriterion.Name,
                NameEn = psdzStandardFpCriterion.NameEn,
                Value = psdzStandardFpCriterion.Value
            };
        }
    }
}
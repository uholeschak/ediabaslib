using BMW.Rheingold.Psdz.Model.Tal.TalFilter;

namespace BMW.Rheingold.Psdz
{
    internal static class TalFilterMapper
    {
        public static IPsdzTalFilter Map(TalFilterModel talFilterModel)
        {
            if (talFilterModel != null)
            {
                return new PsdzTalFilter
                {
                    AsXml = talFilterModel.AsXml
                };
            }

            return null;
        }

        public static TalFilterModel Map(IPsdzTalFilter psdzTalFilter)
        {
            if (psdzTalFilter != null)
            {
                return new TalFilterModel
                {
                    AsXml = psdzTalFilter.AsXml
                };
            }

            return null;
        }
    }
}
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal.TalFilter;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
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
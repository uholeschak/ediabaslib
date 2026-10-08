using RheingoldPsdzWebApi.Adapter.Contracts.Model;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class ResponseMapper
    {
        public static IPsdzResponse Map(ResponseCtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzResponse
            {
                Cause = model.Cause,
                Result = model.Result,
                IsSuccessful = model.Successful
            };
        }

        public static ResponseCtoModel Map(IPsdzResponse psdzResponse)
        {
            if (psdzResponse == null)
            {
                return null;
            }

            return new ResponseCtoModel
            {
                Cause = psdzResponse.Cause,
                Result = psdzResponse.Result,
                Successful = psdzResponse.IsSuccessful
            };
        }
    }
}
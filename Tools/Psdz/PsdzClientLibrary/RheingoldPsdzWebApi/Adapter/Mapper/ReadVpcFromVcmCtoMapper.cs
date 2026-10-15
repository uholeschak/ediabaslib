using System.Linq;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Vcm;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class ReadVpcFromVcmCtoMapper
    {
        public static IPsdzReadVpcFromVcmCto Map(ReadVpcFromVcmCtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzReadVpcFromVcmCto
            {
                IsSuccessful = model.IsSuccessful,
                VpcCrc = model.VpcCrc,
                VpcVersion = model.VpcVersion,
                FailedEcus = model.FailedEcus?.Select(EcuFailureResponseCtoMapper.MapCto).ToList()
            };
        }

        public static ReadVpcFromVcmCtoModel Map(IPsdzReadVpcFromVcmCto psdzReadVpcFromVcmCto)
        {
            if (psdzReadVpcFromVcmCto == null)
            {
                return null;
            }

            return new ReadVpcFromVcmCtoModel
            {
                IsSuccessful = psdzReadVpcFromVcmCto.IsSuccessful,
                VpcCrc = psdzReadVpcFromVcmCto.VpcCrc,
                VpcVersion = psdzReadVpcFromVcmCto.VpcVersion,
                FailedEcus = psdzReadVpcFromVcmCto.FailedEcus?.Select(EcuFailureResponseCtoMapper.MapCto).ToList()
            };
        }
    }
}
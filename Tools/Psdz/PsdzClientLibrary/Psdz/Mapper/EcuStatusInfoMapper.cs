using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Ecu;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class EcuStatusInfoMapper
    {
        public static IPsdzEcuStatusInfo Map(EcuStatusInfoModel ecuStatusInfoModel)
        {
            if (ecuStatusInfoModel == null)
            {
                return null;
            }

            return new PsdzEcuStatusInfo
            {
                ByteValue = ecuStatusInfoModel.ByteValue,
                HasIndividualData = ecuStatusInfoModel.HasIndividualData
            };
        }

        public static EcuStatusInfoModel Map(IPsdzEcuStatusInfo ecuStatusInfo)
        {
            if (ecuStatusInfo == null)
            {
                return null;
            }

            return new EcuStatusInfoModel
            {
                ByteValue = ecuStatusInfo.ByteValue,
                HasIndividualData = ecuStatusInfo.HasIndividualData
            };
        }
    }
}
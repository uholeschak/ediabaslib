using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz
{
    internal static class EcuDetailInfoMapper
    {
        public static IPsdzEcuDetailInfo Map(EcuDetailInfoModel ecuDetailInfoModel)
        {
            if (ecuDetailInfoModel == null)
            {
                return null;
            }

            return new PsdzEcuDetailInfo
            {
                ByteValue = ecuDetailInfoModel.ByteValue
            };
        }

        public static EcuDetailInfoModel Map(IPsdzEcuDetailInfo ecuDetailInfo)
        {
            if (ecuDetailInfo == null)
            {
                return null;
            }

            return new EcuDetailInfoModel
            {
                ByteValue = ecuDetailInfo.ByteValue
            };
        }
    }
}
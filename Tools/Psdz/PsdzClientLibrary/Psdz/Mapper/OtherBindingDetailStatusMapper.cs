using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Certificate;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Certificate;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class OtherBindingDetailStatusMapper
    {
        private static readonly EcuSecCheckingStatusEtoMapper ecuSecCheckingStatusEtoMapper = new EcuSecCheckingStatusEtoMapper();
        public static PsdzOtherBindingDetailsStatus Map(OtherBindingDetailStatusModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzOtherBindingDetailsStatus
            {
                EcuName = model.EcuName,
                RollenName = model.RollenName,
                OtherBindingStatus = ecuSecCheckingStatusEtoMapper.GetValue(model.OtherBindingStatus)
            };
        }

        public static OtherBindingDetailStatusModel Map(PsdzOtherBindingDetailsStatus status)
        {
            if (status == null)
            {
                return null;
            }

            return new OtherBindingDetailStatusModel
            {
                EcuName = status.EcuName,
                RollenName = status.RollenName,
                OtherBindingStatus = ecuSecCheckingStatusEtoMapper.GetValue(status.OtherBindingStatus)
            };
        }
    }
}
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Certificate;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class KeypackDetailStatusMapper
    {
        private static readonly EcuSecCheckingStatusEtoMapper ecuSecCheckingStatusEtoMapper = new EcuSecCheckingStatusEtoMapper();
        public static KeypackDetailStatusModel Map(PsdzKeypackDetailStatus status)
        {
            if (status == null)
            {
                return null;
            }

            return new KeypackDetailStatusModel
            {
                KeyId = status.KeyId,
                KeypackStatus = ecuSecCheckingStatusEtoMapper.GetValue(status.KeyPackStatus)
            };
        }

        public static PsdzKeypackDetailStatus Map(KeypackDetailStatusModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzKeypackDetailStatus
            {
                KeyId = model.KeyId,
                KeyPackStatus = ecuSecCheckingStatusEtoMapper.GetValue(model.KeypackStatus)
            };
        }
    }
}
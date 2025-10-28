using BMW.Rheingold.Psdz.Model.Certificate;
using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal static class EcuSecCheckingResponseEtoMapper
    {
        private static readonly EcuSecCheckingStatusEtoMapper _ecuSecCheckingStatusEtoMapper = new EcuSecCheckingStatusEtoMapper();

        public static PsdzEcuCertCheckingResponse Map(EcuSecCheckingResponseEtoModel model)
        {
            if (model == null)
            {
                return null;
            }
            return new PsdzEcuCertCheckingResponse
            {
                Ecu = EcuIdentifierMapper.Map(model.Ecu),
                CertificateStatus = _ecuSecCheckingStatusEtoMapper.GetValue(model.CertificateStatus),
                BindingsStatus = _ecuSecCheckingStatusEtoMapper.GetValue(model.BindingsStatus),
                OtherBindingsStatus = _ecuSecCheckingStatusEtoMapper.GetValue(model.OtherBindingsStatus),
                OnlineCertificateStatus = _ecuSecCheckingStatusEtoMapper.GetValue(model.OnlineCertificateStatus),
                OnlineBindingsStatus = _ecuSecCheckingStatusEtoMapper.GetValue(model.OnlineBindingsStatus),
                KeyPackStatus = _ecuSecCheckingStatusEtoMapper.GetValue(model.KeypackStatus),
                BindingDetailStatus = model.CertificateBindingDetailStatus?.Select(BindingDetailStatusMapper.Map).ToArray(),
                OnlineBindingDetailStatus = model.OnlineBindingDetailStatus?.Select(BindingDetailStatusMapper.Map).ToArray(),
                OtherBindingDetailStatus = model.OtherBindingDetailStatus?.Select(OtherBindingDetailStatusMapper.Map).ToArray(),
                KeyPackDatailedStatus = model.KeypackDetailStatus?.Select(KeypackDetailStatusMapper.Map).ToArray(),
                CreationTimestamp = model.CreationTimestamp
            };
        }
    }
}
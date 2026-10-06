using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Certificate;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Certificate;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;
using RheingoldPsdzWebApi.Adapter.Mapper;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class BindingDetailStatusMapper
    {
        private static readonly EcuSecCheckingStatusEtoMapper ecuSecCheckingStatusEtoMapper = new EcuSecCheckingStatusEtoMapper();
        public static PsdzBindingDetailsStatus Map(BindingDetailStatusModel bindingDetailStatus)
        {
            if (bindingDetailStatus == null)
            {
                return null;
            }

            return new PsdzBindingDetailsStatus
            {
                BindingStatus = (bindingDetailStatus.BindingStatus.HasValue ? ecuSecCheckingStatusEtoMapper.GetValue(bindingDetailStatus.BindingStatus) : ((PsdzEcuCertCheckingStatus? )null)),
                CertificateStatus = (bindingDetailStatus.CertificateStatus.HasValue ? ecuSecCheckingStatusEtoMapper.GetValue(bindingDetailStatus.CertificateStatus) : ((PsdzEcuCertCheckingStatus? )null)),
                RollenName = bindingDetailStatus.RollenName
            };
        }
    }
}
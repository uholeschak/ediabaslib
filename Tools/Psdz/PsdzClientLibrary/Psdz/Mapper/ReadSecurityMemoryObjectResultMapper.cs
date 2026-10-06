using BMW.Rheingold.Psdz.Model.Certificate;
using System.Linq;
using BMW.Rheingold.Psdz;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Certificate;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;
using RheingoldPsdzWebApi.Adapter.Mapper;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class ReadSecurityMemoryObjectResultMapper
    {
        public static PsdzReadCertMemoryObjectResult Map(ReadSecurityMemoryObjectResultModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzReadCertMemoryObjectResult
            {
                FailedEcus = model.FailedEcus?.Select(EcuFailureResponseCtoMapper.Map).ToArray(),
                MemoryObjects = model.MemoryObjects?.Select(SecurityMemoryObjectEtoMapper.Map).ToArray()
            };
        }

        public static ReadSecurityMemoryObjectResultModel Map(PsdzReadCertMemoryObjectResult psdzObject)
        {
            if (psdzObject == null)
            {
                return null;
            }

            return new ReadSecurityMemoryObjectResultModel
            {
                FailedEcus = psdzObject.FailedEcus?.Select(EcuFailureResponseCtoMapper.Map).ToList(),
                MemoryObjects = psdzObject.MemoryObjects?.Select(SecurityMemoryObjectEtoMapper.Map).ToList()
            };
        }
    }
}
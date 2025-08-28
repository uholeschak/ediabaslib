using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Certificate;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz.Client
{
    internal class CertificateManagementServiceClient : PsdzClientBase<ICertificateManagementService>, ICertificateManagementService
    {
        public CertificateManagementServiceClient(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {
        }

        public CertificateManagementServiceClient(ChannelFactory<ICertificateManagementService> channelFactory)
            : base(channelFactory)
        {
        }

        public PsdzRequestEcuSecCheckingResult RequestEcuSecChecking(IPsdzConnection connection, IPsdzSvt svtIst, IPsdzEcuIdentifier[] ecus, int retries)
        {
            return CallFunction((ICertificateManagementService service) => service.RequestEcuSecChecking(connection, svtIst, ecus, retries));
        }

        public PsdzFetchEcuCertCheckingResult FetchEcuSecChecking(IPsdzConnection connection, IPsdzSvt svtIst, IPsdzEcuIdentifier[] ecus, int retries)
        {
            return CallFunction((ICertificateManagementService service) => service.FetchEcuSecChecking(connection, svtIst, ecus, retries));
        }

        public PsdzCertMemoryObject[] CalculateBindingDistribution(PsdzCertMemoryObject[] bindingsFromCbb, PsdzCertMemoryObject[] bindingsFromVehicle)
        {
            return CallFunction((ICertificateManagementService service) => service.CalculateBindingDistribution(bindingsFromCbb, bindingsFromVehicle));
        }

        public PsdzCertMemoryObject[] DeleteCertificatesWithRole(string role, IPsdzEcuIdentifier[] ecus, PsdzCertMemoryObject[] memoryObjects)
        {
            return CallFunction((ICertificateManagementService service) => service.DeleteCertificatesWithRole(role, ecus, memoryObjects));
        }

        public PsdzFetchBindingCalculationResult FetchCertificatesBindingsAndKeypacks(PsdzBindingCalculationRequestId requestId)
        {
            return CallFunction((ICertificateManagementService service) => service.FetchCertificatesBindingsAndKeypacks(requestId));
        }

        public PsdzReadCertMemoryObjectResult ReadSecurityMemoryObjects(IPsdzConnection connection, IPsdzSvt svtIst, IPsdzEcuIdentifier[] ecus, PsdzCertMemoryObjectType certMemoryObjectType)
        {
            return CallFunction((ICertificateManagementService service) => service.ReadSecurityMemoryObjects(connection, svtIst, ecus, certMemoryObjectType));
        }

        public PsdzEcuFailureResponse[] WriteSecurityMemoryObjects(IPsdzConnection connection, IPsdzSvt svtIst, PsdzCertMemoryObject[] certificates)
        {
            return CallFunction((ICertificateManagementService service) => service.WriteSecurityMemoryObjects(connection, svtIst, certificates));
        }

        public PsdzBindingCalculationFailure[] RequestCertificatesBindingsAndKeypacksOffline(PsdzCertMemoryObject[] certificates, string requestFile, string client, string system, IPsdzVin vin, IPsdzSvt svt)
        {
            return CallFunction((ICertificateManagementService service) => service.RequestCertificatesBindingsAndKeypacksOffline(certificates, requestFile, client, system, vin, svt));
        }

        public PsdzFetchBindingCalculationResult FetchCertificatesBindingsAndKeypacksOffline(string bindingsFile)
        {
            return CallFunction((ICertificateManagementService service) => service.FetchCertificatesBindingsAndKeypacksOffline(bindingsFile));
        }

        public PsdzResponse[] CheckBackendConnection(string[] cbbUrls, int timeout)
        {
            return CallFunction((ICertificateManagementService service) => service.CheckBackendConnection(cbbUrls, timeout));
        }
    }
}

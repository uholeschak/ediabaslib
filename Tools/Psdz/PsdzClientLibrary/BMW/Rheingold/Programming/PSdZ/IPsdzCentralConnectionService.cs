using System;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;
using RheingoldPsdzWebApi.Adapter.Contracts.Services;

namespace BMW.Rheingold.Programming.PSdZ
{
    public interface IPsdzCentralConnectionService
    {
        IPsdzConnection OpenConnection(Func<IPsdzConnection> connectionAction);

        void ReleaseConnection();

        IPsdzConnection GetConnection();

        string GetLocalIpAddress();

        void FillLocalIpAddress(IHttpConfigurationService service);
    }
}
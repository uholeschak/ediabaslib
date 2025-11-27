using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using System;

namespace PsdzClient.Psdz
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
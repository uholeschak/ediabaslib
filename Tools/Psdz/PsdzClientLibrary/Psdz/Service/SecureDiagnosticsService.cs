using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz.Model;
using PsdzClient.Core;
using System;
using System.Net.Http;
using RestSharp;

namespace BMW.Rheingold.Psdz
{
    internal class SecureDiagnosticsService : ISecureDiagnosticsService
    {
        private readonly IWebCallHandler _webCallHandler;
        private readonly string endpointService = "securediagnostics";
        public SecureDiagnosticsService(IWebCallHandler webCallHandler)
        {
            _webCallHandler = webCallHandler;
        }

        public void RegisterAuthService29Callback(byte[] s29CertificateChainByteArray, byte[] serializedPrivateKey, IPsdzConnection connection)
        {
            Log.Info(Log.CurrentMethod(), $"Called. Connection: {connection}");
            try
            {
                if (connection == null)
                {
                    throw new ArgumentNullException("connection");
                }

                RegisterAuthService29CallbackRequestModel requestBodyObject = new RegisterAuthService29CallbackRequestModel
                {
                    S29CertificateChainByteArray = s29CertificateChainByteArray,
                    SerializedPrivateKey = serializedPrivateKey
                };
                ApiResult apiResult = _webCallHandler.ExecuteRequest(endpointService, $"registerauthservice29callback/{connection.Id}", HttpMethod.Post, requestBodyObject);
                Log.Info(Log.CurrentMethod(), $"Finished. ResultIsSuccessful: {apiResult.IsSuccessful}");
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public void UnlockGateway(IPsdzConnection connection)
        {
            Log.Info(Log.CurrentMethod(), $"Called. Connection: {connection}");
            try
            {
                if (connection == null)
                {
                    throw new ArgumentNullException("connection");
                }

                ApiResult apiResult = _webCallHandler.ExecuteRequest(endpointService, $"unlockgateway/{connection.Id}", HttpMethod.Post);
                Log.Info(Log.CurrentMethod(), $"Finished. ResultIsSuccessful: {apiResult.IsSuccessful}");
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }
    }
}
using System;
using System.Net.Http;
using BMW.Rheingold.Psdz;
using PsdzClient.Core;
using RestSharp;

namespace BMW.Rheingold.Psdz
{
    internal class HttpConfigurationService : IHttpConfigurationService
    {
        private readonly IWebCallHandler _webCallHandler;
        private readonly string _endpointService = "httpconfiguration";
        public HttpConfigurationService(IWebCallHandler webCallHandler)
        {
            _webCallHandler = webCallHandler;
        }

        public string GetHttpServerAddress()
        {
            try
            {
                return _webCallHandler.ExecuteRequest<string>(_endpointService, "gethttpserveraddress", HttpMethod.Get).Data;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public string GetNetworkEndpointSet()
        {
            try
            {
                return _webCallHandler.ExecuteRequest<string>(_endpointService, "getnetworkendpointset", HttpMethod.Get).Data;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public void SetHttpServerAddress(string address)
        {
            try
            {
                SetHttpServerAddressRequestModel requestBodyObject = new SetHttpServerAddressRequestModel
                {
                    ServerAddress = address
                };
                _webCallHandler.ExecuteRequest(_endpointService, "sethttpserveraddress", HttpMethod.Post, requestBodyObject);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public int GetHttpServerPort()
        {
            try
            {
                return _webCallHandler.ExecuteRequest<int>(_endpointService, "gethttpserverport", HttpMethod.Get).Data;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public void SetHttpServerPort(int port)
        {
            try
            {
                SetHttpServerPortRequestModel requestBodyObject = new SetHttpServerPortRequestModel
                {
                    ServerPort = port
                };
                _webCallHandler.ExecuteRequest(_endpointService, "sethttpserverport", HttpMethod.Post, requestBodyObject);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }
    }
}
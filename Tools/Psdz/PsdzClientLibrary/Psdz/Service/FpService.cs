using BMW.Rheingold.CoreFramework;
using System;
using System.Net.Http;
using RheingoldPsdzWebApi.Adapter.Contracts;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;
using RheingoldPsdzWebApi.Adapter.Contracts.Services;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;
using RheingoldPsdzWebApi.Adapter.Mapper;

namespace RheingoldPsdzWebApi.Adapter.Services
{
    public class FpService : IFpService
    {
        private readonly IWebCallHandler webCallHandler;

        private readonly string serviceName = "fp";

        public FpService(IWebCallHandler webCallHandler)
        {
            this.webCallHandler = webCallHandler;
        }

        public IPsdzFp parseXml(string xmlPathString)
        {
            try
            {
                ParseXmlRequestModel requestBodyObject = new ParseXmlRequestModel
                {
                    XmlPathString = xmlPathString
                };
                return FpMapper.Map(webCallHandler.ExecuteRequest<FpModel>(serviceName, "parseXML", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }
    }
}
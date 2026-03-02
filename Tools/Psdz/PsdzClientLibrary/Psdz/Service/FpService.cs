using System;
using System.Net.Http;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using PsdzClient.Core;

namespace BMW.Rheingold.Psdz
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
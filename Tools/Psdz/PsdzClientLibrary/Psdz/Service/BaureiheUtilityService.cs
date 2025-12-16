using System;
using System.Net.Http;
using BMW.Rheingold.Psdz;
using PsdzClient.Core;
using PsdzClient.Utility;

namespace BMW.Rheingold.Psdz
{
    internal class BaureiheUtilityService : IBaureiheUtilityService
    {
        private readonly IWebCallHandler _webCallHandler;

        private readonly string _endpointService = "baureihe";

        public BaureiheUtilityService(IWebCallHandler webCallHandler)
        {
            _webCallHandler = webCallHandler;
        }

        public string GetBaureihe(string baureihe)
        {
            try
            {
                return _webCallHandler.ExecuteRequest<string>(_endpointService, baureihe ?? "", HttpMethod.Get).Data;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return FormatConverter.ConvertToBn2020ConformModelSeries(baureihe);
            }
        }
    }
}
using System;
using BMW.Rheingold.Psdz;
using PsdzClient.Core;
using System.Collections.Generic;
using System.IO;
using RestSharp;

namespace BMW.Rheingold.Psdz
{
    internal class ConfigurationService : IConfigurationService
    {
        private const string DefaultDealerId = "1234";

        private readonly IWebCallHandler _webCallHandler;

        private readonly IHttpServerService _httpServerService;

        private readonly string _endpointService = "configuration";

        public ConfigurationService(IWebCallHandler webCallHandler, IHttpServerService httpServerService)
        {
            _webCallHandler = webCallHandler;
            _httpServerService = httpServerService;
        }

        public bool IsReady()
        {
            try
            {
                _webCallHandler.IgnorePrepareExecuteRequest = true;
                return _webCallHandler.ExecuteRequest<bool>(_endpointService, "isready", Method.Get).Data;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
            finally
            {
                _webCallHandler.IgnorePrepareExecuteRequest = false;
            }
        }

        public RootDirectorySetupResultModel GetRootDirectorySetupResult()
        {
            try
            {
                _webCallHandler.IgnorePrepareExecuteRequest = true;
                return _webCallHandler.ExecuteRequest<RootDirectorySetupResultModel>(_endpointService, "rootdirectorysetupresultmodel", Method.Get).Data;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
            finally
            {
                _webCallHandler.IgnorePrepareExecuteRequest = false;
            }
        }

        // [UH] For backward compatibility
        public string GetExpectedPsdzVersion()
        {
            throw new NotImplementedException();
        }

        public string GetPsdzVersion()
        {
            try
            {
                return _webCallHandler.ExecuteRequest<string>(_endpointService, "getpsdzversion", Method.Get).Data;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public string GetRootDirectory()
        {
            try
            {
                return _webCallHandler.ExecuteRequest<string>(_endpointService, "getrootdirectory", Method.Get).Data;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public bool ImportPdx(string pathToPdxContainer, string projectName)
        {
            try
            {
                PsdzHelper.CheckString("pathToPdxContainer", pathToPdxContainer);
                PsdzHelper.CheckString("projectName", projectName);
                List<string> pathToPdxContainer2 = new List<string> { Path.GetFullPath(pathToPdxContainer) };
                string rootDirectory = GetRootDirectory();
                ImportPdxRequestModel requestBodyObject = new ImportPdxRequestModel
                {
                    RootDirectory = rootDirectory,
                    PathToPdxContainer = pathToPdxContainer2,
                    ProjectName = projectName
                };
                _webCallHandler.ExecuteRequest(_endpointService, "importpdx", Method.Post, requestBodyObject);
                SetRootDirectory(rootDirectory);
                return true;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return false;
            }
        }

        public string RequestBaureihenverbund(string baureihe)
        {
            try
            {
                return _webCallHandler.ExecuteRequest<string>(_endpointService, "requestbaureihenverbund/" + baureihe, Method.Get).Data;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        internal void SetPsdzProperties(string dealerId, string plantId = "0", string programmierGeraeteSeriennummer = "1000", string testerEinsatzKennung = "FA")
        {
            try
            {
                SetPSdZPropertiesRequestModel requestBodyObject = new SetPSdZPropertiesRequestModel
                {
                    DealerId = ConvertDealerIdToHex(dealerId),
                    PlantId = plantId,
                    ProgrammierGeraeteSeriennummer = programmierGeraeteSeriennummer,
                    TesterEinsatzKennung = testerEinsatzKennung
                };
                _webCallHandler.ExecuteRequest(_endpointService, "setpsdzproperties", Method.Post, requestBodyObject);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public void SetRootDirectory(string rootDir)
        {
            try
            {
                SetRootDirectoryRequestModel requestBodyObject = new SetRootDirectoryRequestModel
                {
                    RootDirectoryPath = rootDir
                };
                _webCallHandler.ExecuteRequest(_endpointService, "setrootdirectory", Method.Post, requestBodyObject);
                _httpServerService.Start();
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public void UnsetRootDirectory()
        {
            try
            {
                _httpServerService.Stop();
                _webCallHandler.ExecuteRequest(_endpointService, "unsetrootdirectory", Method.Post);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        private string ConvertDealerIdToHex(string dealerId)
        {
            string text;
            if (dealerId != null && ushort.TryParse(dealerId, out var result))
            {
                text = result.ToString("X");
                Log.Info(Log.CurrentMethod(), "Dealer ID " + text + " is used.");
            }
            else
            {
                text = "1234";
                Log.Info(Log.CurrentMethod(), "dealerId " + dealerId + " cannot be converted to a Hex value. The default Hex value (1234) is used.");
            }
            return text;
        }
    }
}
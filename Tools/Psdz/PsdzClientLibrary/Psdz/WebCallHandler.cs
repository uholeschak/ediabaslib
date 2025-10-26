using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using PsdzClient.Contracts;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using RestSharp;

namespace BMW.Rheingold.Psdz
{
    public class WebCallHandler : IWebCallHandler, ILifeCycleDependencyProvider
    {
        private readonly Action _prepareExecuteRequestAction;

        private readonly RestClient _client;

        private readonly string _baseUrl;

        private readonly Func<bool> _isPsdzInitialized;

        private const int DefaultTimoutForPsdzWebService = 5;

        private int _activeWebRequests;

        private readonly List<string> methodsNotToBeLogged = new List<string> { "seteventid", "getevents", "startlistening", "stoplistening", "getcurrentlogidordefault", "setlogid" };

        public string Description { get; }

        public string Name { get; }

        public bool IgnorePrepareExecuteRequest { get; set; }

        public event EventHandler<DependencyCountChangedEventArgs> ActiveDependencyCountChanged;

        public WebCallHandler(string baseUrl, Action prepareExecuteRequestAction, Func<bool> isPsdzInitialized)
        {
            Name = "WebCallHandler";
            Description = "Tracks/Counts all client-requests";
            _prepareExecuteRequestAction = prepareExecuteRequestAction;
            _baseUrl = baseUrl;
            RestClientOptions options = new RestClientOptions(baseUrl)
            {
                ThrowOnAnyError = true,
#pragma warning disable CS0618 // Type or member is obsolete
                MaxTimeout = (int)TimeSpan.FromMinutes(5.0).TotalMilliseconds,
#pragma warning restore CS0618 // Type or member is obsolete
                RemoteCertificateValidationCallback = ValidateWebserviceCertificate
            };
            _client = new RestClient(options);
            _isPsdzInitialized = isPsdzInitialized;
        }

        public ApiResult ExecuteRequest(string serviceName, string endpoint, Method method, object requestBodyObject = null, IDictionary<string, string> queryParameters = null)
        {
            return ExecuteRequestImpl<object>(serviceName, endpoint, method, requestBodyObject, queryParameters);
        }

        public ApiResult<T> ExecuteRequest<T>(string serviceName, string endpoint, Method method, object requestBodyObject = null, IDictionary<string, string> queryParameters = null)
        {
            return ExecuteRequestImpl<T>(serviceName, endpoint, method, requestBodyObject, queryParameters);
        }

        private ApiResult<T> ExecuteRequestImpl<T>(string serviceName, string endpoint, Method method, object requestBodyObject = null, IDictionary<string, string> queryParameters = null)
        {
            if (!endpoint.Equals("isready") && !_isPsdzInitialized())
            {
                Log.Debug(Log.CurrentMethod(), "WebService is not initialized. Cannot execute request");
                return new ApiResult<T>(default(T), isSuccessful: false);
            }
            RestResponse restResponse = null;
            try
            {
                if (!IgnorePrepareExecuteRequest && _prepareExecuteRequestAction != null)
                {
                    _prepareExecuteRequestAction();
                }
                IncrementRequestCounts();
                RestRequest restRequest = new RestRequest(Path.Combine(serviceName, endpoint) ?? "", method);
                if (queryParameters != null)
                {
                    foreach (KeyValuePair<string, string> queryParameter in queryParameters)
                    {
                        restRequest.AddQueryParameter(queryParameter.Key, queryParameter.Value);
                    }
                }
                if (requestBodyObject != null)
                {
                    JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.None,
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };
                    jsonSerializerSettings.Converters.Add(new StringEnumConverter());
                    string obj = JsonConvert.SerializeObject(requestBodyObject, jsonSerializerSettings);
                    restRequest.RequestFormat = DataFormat.Json;
                    restRequest.AddJsonBody(obj);
                }
                Log.Debug(Log.CurrentMethod(), CreateRequestLogMessage("API-Request", _client, restRequest, requestBodyObject));
                restResponse = _client.Execute(restRequest);
                if (restResponse.IsSuccessful)
                {
                    T responseData = GetResponseData<T>(restResponse);
                    Log.Debug(Log.CurrentMethod(), CreateResponseLogMessage("API-Response-Valid", restResponse, responseData));
                    return new ApiResult<T>(responseData, isSuccessful: true);
                }
                Log.Error(Log.CurrentMethod(), CreateResponseLogMessage("API-Response-Invalid", restResponse, GetResponseData<T>(restResponse)));
            }
            catch (WebException ex)
            {
                if ((HttpWebResponse)ex.Response == null)
                {
                    Dictionary<string, string> additionalInformation = new Dictionary<string, string> { { "ErrorMessage", "No response from server" } };
                    Log.Error(Log.CurrentMethod(), CreateResponseLogMessage("API-Error", restResponse, GetResponseData<T>(restResponse), additionalInformation));
                }
                string msg = CreateResponseLogMessage("API-Error", restResponse, GetResponseData<T>(restResponse), new Dictionary<string, string> { { "Message", ex.Message } });
                Log.Error(Log.CurrentMethod(), msg);
            }
            catch (JsonSerializationException ex2)
            {
                Dictionary<string, string> additionalInformation2 = new Dictionary<string, string>
            {
                { "Message", ex2.Message },
                { "Path", ex2.Path },
                {
                    "LineNumber",
                    ex2.ToString()
                },
                {
                    "LinePosition",
                    ex2.LinePosition.ToString()
                }
            };
                string msg2 = CreateResponseLogMessage("API-JSON-Error", restResponse, GetResponseData<T>(restResponse), additionalInformation2);
                Log.Error(Log.CurrentMethod(), msg2);
            }
            catch (JsonReaderException ex3)
            {
                Dictionary<string, string> additionalInformation3 = new Dictionary<string, string>
            {
                { "Message", ex3.Message },
                { "Path", ex3.Path },
                {
                    "LineNumber",
                    ex3.LineNumber.ToString()
                },
                {
                    "LinePosition",
                    ex3.LinePosition.ToString()
                }
            };
                string msg3 = CreateResponseLogMessage("API-JSON-Error", restResponse, GetResponseData<T>(restResponse), additionalInformation3);
                Log.Error(Log.CurrentMethod(), msg3);
            }
            catch (Exception ex4)
            {
                Dictionary<string, string> additionalInformation4 = new Dictionary<string, string> { { "Message", ex4.Message } };
                string msg4 = CreateResponseLogMessage("API-Error", restResponse, GetResponseData<T>(restResponse), additionalInformation4);
                Log.Error(Log.CurrentMethod(), msg4);
            }
            finally
            {
                DecrementRequestCounts();
            }
            return new ApiResult<T>(default(T), isSuccessful: false);
        }

        private static T GetResponseData<T>(RestResponse response)
        {
            T result = default(T);
            if (response != null && response.StatusCode != HttpStatusCode.NoContent)
            {
                return JsonConvert.DeserializeObject<GenericApiResponse<T>>(response.Content).Data;
            }
            return result;
        }

        private string CreateRequestLogMessage(string loggerKeys, RestClient client, RestRequest request, object requestBodyObject)
        {
            try
            {
                dynamic val = new ExpandoObject();
                val.type = loggerKeys;
                val.method = request.Method.ToString().ToUpperInvariant();
                val.uri = client.BuildUri(request);
                if (ShouldRequestDataBeLogged(request))
                {
                    val.requestObject = requestBodyObject;
                }
                return Regex.Replace(JsonConvert.SerializeObject(val, Formatting.None), "[{}']", "").Replace(",", " - ");
            }
            catch (Exception ex)
            {
                return "Unexpected error while creating the log message in 'CreateRequestLogMessage':" + Environment.NewLine + ex.Message;
            }
        }

        private string CreateResponseLogMessage(string loggerKeys, RestResponse response, object dataObject, IDictionary<string, string> additionalInformation = null)
        {
            try
            {
                dynamic val = new ExpandoObject();
                val.Type = loggerKeys;
                val.StatusCode = response?.StatusCode;
                val.ResponseUri = response?.ResponseUri;
                if (ShouldResponseDataBeLogged(response))
                {
                    val.RequestObject = dataObject;
                }
                IDictionary<string, object> dictionary = (IDictionary<string, object>)val;
                if (!string.IsNullOrWhiteSpace(response?.ErrorMessage))
                {
                    dictionary.Add("ErrorMessage", response.ErrorMessage);
                }
                if (additionalInformation != null && additionalInformation.Any())
                {
                    foreach (KeyValuePair<string, string> item in additionalInformation)
                    {
                        dictionary.Add(item.Key, item.Value);
                    }
                }
                JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.None,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
                return Regex.Replace(JsonConvert.SerializeObject(val, jsonSerializerSettings), "[{}']", "").Replace(",", " - ");
            }
            catch (Exception ex)
            {
                return "Unexpected error while creating the log message in 'CreateResponseLogMessage':" + Environment.NewLine + ex.Message;
            }
        }

        private bool ShouldResponseDataBeLogged(RestResponse response)
        {
            if (response == null)
            {
                return false;
            }
            string path = response.ResponseUri?.AbsolutePath ?? string.Empty;
            return !methodsNotToBeLogged.Any((string cmd) => path.Contains(cmd));
        }

        private bool ShouldRequestDataBeLogged(RestRequest request)
        {
            if (request == null)
            {
                return false;
            }
            string path = request.Resource ?? string.Empty;
            return !methodsNotToBeLogged.Any((string cmd) => path.Contains(cmd));
        }

        private void DecrementRequestCounts()
        {
            int dependencyCount = Interlocked.Decrement(ref _activeWebRequests);
            OnActiveDependencyCountChanged(dependencyCount);
        }

        private void IncrementRequestCounts()
        {
            int dependencyCount = Interlocked.Increment(ref _activeWebRequests);
            OnActiveDependencyCountChanged(dependencyCount);
        }

        private void OnActiveDependencyCountChanged(int dependencyCount)
        {
            this.ActiveDependencyCountChanged?.Invoke(this, new DependencyCountChangedEventArgs(dependencyCount));
        }

        private bool ValidateWebserviceCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if ((sender as HttpRequestMessage).RequestUri.AbsoluteUri.StartsWith(_baseUrl) && certificate.GetCertHashString() != "5B:D6:82:8D:38:E1:E9:94:7D:FC:67:2C:B8:59:5B:C4:69:B6:C0:A9".Replace(":", string.Empty))
            {
                Log.Error(Log.CurrentMethod(), $"Certificate Validation Error: {sslPolicyErrors}");
                return false;
            }
            return true;
        }
    }
}
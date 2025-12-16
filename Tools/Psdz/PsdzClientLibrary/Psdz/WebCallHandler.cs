using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using PsdzClient.Contracts;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace BMW.Rheingold.Psdz
{
    public class WebCallHandler : IWebCallHandler, ILifeCycleDependencyProvider
    {
        private readonly Action _prepareExecuteRequestAction;

        private readonly HttpClient _httpClient;

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
            HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (HttpRequestMessage message, X509Certificate2 cert, X509Chain chain, SslPolicyErrors errors) => ValidateWebserviceCertificate(message, cert, chain, errors)
            };
            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromMinutes(5.0)
            };
            _isPsdzInitialized = isPsdzInitialized;
        }

        public ApiResult ExecuteRequest(string serviceName, string endpoint, HttpMethod method, object requestBodyObject = null, IDictionary<string, string> queryParameters = null)
        {
            return ExecuteRequestImpl<object>(serviceName, endpoint, method, requestBodyObject, queryParameters);
        }

        public ApiResult<T> ExecuteRequest<T>(string serviceName, string endpoint, HttpMethod method, object requestBodyObject = null, IDictionary<string, string> queryParameters = null)
        {
            return ExecuteRequestImpl<T>(serviceName, endpoint, method, requestBodyObject, queryParameters);
        }

        private ApiResult<T> ExecuteRequestImpl<T>(string serviceName, string endpoint, HttpMethod method, object requestBodyObject = null, IDictionary<string, string> queryParameters = null)
        {
            HttpResponseMessage httpResponseMessage = null;
            try
            {
                if (!endpoint.Equals("isready") && !_isPsdzInitialized())
                {
                    Log.Debug(Log.CurrentMethod(), "WebService is not initialized. Cannot execute request");
                    return new ApiResult<T>(default(T), isSuccessful: false);
                }
                HttpRequestMessage request = PrepareRequest(serviceName, endpoint, method, requestBodyObject, queryParameters);
                Log.Debug(Log.CurrentMethod(), CreateRequestLogMessage("API-Request", _httpClient, request, requestBodyObject));
                httpResponseMessage = _httpClient.SendAsync(request).GetAwaiter().GetResult();
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    T responseData = GetResponseData<T>(httpResponseMessage);
                    Log.Debug(Log.CurrentMethod(), CreateResponseLogMessage("API-Response-Valid", httpResponseMessage, responseData));
                    return new ApiResult<T>(responseData, isSuccessful: true);
                }
                Log.Error(Log.CurrentMethod(), CreateResponseLogMessage("API-Response-Invalid", httpResponseMessage, GetResponseData<T>(httpResponseMessage)));
            }
            catch (Exception ex)
            {
                HandleException<T>(ex, httpResponseMessage, serviceName, endpoint, method, Log.CurrentMethod());
            }
            finally
            {
                DecrementRequestCounts();
            }
            return new ApiResult<T>(default(T), isSuccessful: false);
        }

        private static T GetResponseData<T>(HttpResponseMessage response)
        {
            T result = default(T);
            if (response != null && response.StatusCode != HttpStatusCode.NoContent)
            {
                return JsonConvert.DeserializeObject<GenericApiResponse<T>>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult()).Data;
            }
            return result;
        }

        private string CreateRequestLogMessage(string loggerKeys, HttpClient client, HttpRequestMessage request, object requestBodyObject)
        {
            try
            {
                dynamic val = new ExpandoObject();
                val.type = loggerKeys;
                val.method = request.Method.Method.ToUpperInvariant();
                val.uri = request.RequestUri;
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

        private string CreateResponseLogMessage(string loggerKeys, HttpResponseMessage response, object dataObject, IDictionary<string, string> additionalInformation = null)
        {
            try
            {
                dynamic val = new ExpandoObject();
                val.Type = loggerKeys;
                val.StatusCode = response?.StatusCode;
                val.ResponseUri = response?.RequestMessage?.RequestUri;
                if (ShouldResponseDataBeLogged(response))
                {
                    val.RequestObject = dataObject;
                }
                IDictionary<string, object> dictionary = (IDictionary<string, object>)val;
                if (!string.IsNullOrWhiteSpace(response?.ReasonPhrase))
                {
                    dictionary.Add("ReasonPhrase", response.ReasonPhrase);
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

        private bool ShouldResponseDataBeLogged(HttpResponseMessage response)
        {
            if (response == null)
            {
                return false;
            }
            string path = response.RequestMessage?.RequestUri?.OriginalString ?? string.Empty;
            return !methodsNotToBeLogged.Any((string cmd) => path.Contains(cmd));
        }

        private bool ShouldRequestDataBeLogged(HttpRequestMessage request)
        {
            if (request == null)
            {
                return false;
            }
            string path = request.RequestUri?.OriginalString ?? string.Empty;
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
            string text = (sender as HttpRequestMessage)?.RequestUri?.AbsoluteUri ?? string.Empty;
            if (!string.IsNullOrEmpty(text) && text.StartsWith(_baseUrl) && certificate.GetCertHashString() != "5B:D6:82:8D:38:E1:E9:94:7D:FC:67:2C:B8:59:5B:C4:69:B6:C0:A9".Replace(":", string.Empty))
            {
                Log.Error(Log.CurrentMethod(), $"Certificate Validation Error: {sslPolicyErrors}");
                return false;
            }
            return true;
        }

        private HttpRequestMessage PrepareRequest(string serviceName, string endpoint, HttpMethod method, object requestBodyObject = null, IDictionary<string, string> queryParameters = null)
        {
            if (!IgnorePrepareExecuteRequest && _prepareExecuteRequestAction != null)
            {
                _prepareExecuteRequestAction();
            }
            IncrementRequestCounts();
            string text = serviceName.TrimEnd(new char[1] { '/' }) + "/" + endpoint.TrimStart(new char[1] { '/' });
            if (queryParameters != null && queryParameters.Any())
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append('?');
                stringBuilder.Append(string.Join("&", queryParameters.Select((KeyValuePair<string, string> kv) => Uri.EscapeDataString(kv.Key) + "=" + Uri.EscapeDataString(kv.Value))));
                text += stringBuilder.ToString();
            }
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(method, text);
            if (requestBodyObject != null)
            {
                JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.None,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
                jsonSerializerSettings.Converters.Add(new StringEnumConverter());
                string content = JsonConvert.SerializeObject(requestBodyObject, jsonSerializerSettings);
                httpRequestMessage.Content = new StringContent(content, Encoding.UTF8, "application/json");
            }
            return httpRequestMessage;
        }

        private void HandleException<T>(Exception ex, HttpResponseMessage response, string serviceName, string endpoint, HttpMethod method, string methodName)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>
        {
            {
                "Endpoint",
                serviceName + "/" + endpoint
            },
            {
                "Method",
                method.ToString()
            },
            { "Message", ex.Message }
        };
            if (ex is JsonSerializationException || ex is JsonReaderException)
            {
                dictionary.Add("Path", (ex as JsonSerializationException)?.Path ?? (ex as JsonReaderException)?.Path);
                dictionary.Add("LineNumber", (ex as JsonSerializationException)?.LineNumber.ToString() ?? (ex as JsonReaderException)?.LineNumber.ToString());
                dictionary.Add("LinePosition", (ex as JsonSerializationException)?.LinePosition.ToString() ?? (ex as JsonReaderException)?.LinePosition.ToString());
            }
            else if (ex is WebException ex2 && (HttpWebResponse)ex2.Response == null)
            {
                dictionary.Add("ErrorMessage", "No response from server");
                Log.Error(methodName, CreateResponseLogMessage("API-Error", response, GetResponseData<T>(response), dictionary));
            }
            string msg = CreateResponseLogMessage("API-Error", response, GetResponseData<T>(response), dictionary);
            Log.Error(methodName, msg);
        }
    }
}
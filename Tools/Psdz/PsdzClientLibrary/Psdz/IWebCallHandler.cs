using PsdzClient.Contracts;
using System.Collections.Generic;
using RestSharp;

namespace BMW.Rheingold.Psdz
{
    public interface IWebCallHandler : ILifeCycleDependencyProvider
    {
        bool IgnorePrepareExecuteRequest { get; set; }

        ApiResult ExecuteRequest(string serviceName, string endpoint, Method method, object requestBodyObject = null, IDictionary<string, string> queryParameters = null);

        ApiResult<T> ExecuteRequest<T>(string serviceName, string endpoint, Method method, object requestBodyObject = null, IDictionary<string, string> queryParameters = null);
    }
}

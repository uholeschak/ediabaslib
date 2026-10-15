using System.Collections.Generic;
using System.Net.Http;

namespace RheingoldPsdzWebApi.Adapter.Contracts
{
    public interface IWebCallHandler : ILifeCycleDependencyProvider
    {
        bool IgnorePrepareExecuteRequest { get; set; }

        ApiResult ExecuteRequest(string serviceName, string endpoint, HttpMethod method, object requestBodyObject = null, IDictionary<string, string> queryParameters = null);

        ApiResult<T> ExecuteRequest<T>(string serviceName, string endpoint, HttpMethod method, object requestBodyObject = null, IDictionary<string, string> queryParameters = null);
    }
}

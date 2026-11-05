namespace RheingoldPsdzWebApi.Adapter.Contracts
{
    public class ApiResult
    {
        public bool IsSuccessful { get; }
        public string ErrorMessage { get; }

        public ApiResult(bool isSuccessful, string errorMessage)
        {
            IsSuccessful = isSuccessful;
            ErrorMessage = errorMessage;
        }
    }

    public class ApiResult<T> : ApiResult
    {
        public T Data { get; }

        public ApiResult(T data, bool isSuccessful, string errorMessage)
            : base(isSuccessful, errorMessage)
        {
            Data = data;
        }
    }
}
namespace BMW.Rheingold.Psdz
{
    public class ApiResult
    {
        public bool IsSuccessful { get; }

        public ApiResult(bool isSuccessful)
        {
            IsSuccessful = isSuccessful;
        }
    }

    public class ApiResult<T> : ApiResult
    {
        public T Data { get; }

        public ApiResult(T data, bool isSuccessful)
            : base(isSuccessful)
        {
            Data = data;
        }
    }
}
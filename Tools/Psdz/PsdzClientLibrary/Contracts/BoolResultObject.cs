using System;

namespace PsdzClient.Contracts
{
    public class BoolResultObject : IBoolResultObject
    {
        public static readonly BoolResultObject SuccessResult = new BoolResultObject
        {
            Result = true,
            ErrorCode = "",
            ErrorMessage = "",
            Context = "",
            ErrorCodeInt = 0
        };

        public string ErrorCode { get; set; }

        public int ErrorCodeInt { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;

        public bool Result { get; set; }

        public string Context { get; set; }

        public DateTime Time { get; set; }

        public int StatusCode { get; set; }

        public override string ToString()
        {
            if (Result)
            {
                return Result.ToString();
            }
            return $"{Result} [({ErrorCode}) {ErrorMessage}]";
        }

        public void SetValues(bool result, string errorCode, string errorMessage)
        {
            Result = result;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }
    }
}
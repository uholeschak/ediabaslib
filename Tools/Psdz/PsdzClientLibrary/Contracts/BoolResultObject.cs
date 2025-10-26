using System;

namespace PsdzClient.Contracts
{
    public class BoolResultObject : IBoolResultObject
    {
        public static BoolResultObject SuccessResult => new BoolResultObject
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

        public static BoolResultObject FailResult(string message)
        {
            return new BoolResultObject
            {
                Result = false,
                ErrorMessage = message
            };
        }

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

    public class BoolResultObject<T> : BoolResultObject, IBoolResultObject<T>, IBoolResultObject
    {
        public T ResultObject { get; set; }

        public static BoolResultObject<T> Success(T ResultObject)
        {
            return new BoolResultObject<T>
            {
                Result = true,
                ResultObject = ResultObject
            };
        }

        public static BoolResultObject<T> Fail(string ErrorMessage)
        {
            return new BoolResultObject<T>
            {
                Result = false,
                ErrorMessage = ErrorMessage
            };
        }

        public BoolResultObject()
        {
        }

        public BoolResultObject(IBoolResultObject boolResultObject, T result)
        {
            CopyBoolObjectResultValues(boolResultObject);
            ResultObject = result;
        }

        public void CopyBoolObjectResultValues(IBoolResultObject boolResultObject)
        {
            base.ErrorCode = boolResultObject.ErrorCode;
            base.ErrorMessage = boolResultObject.ErrorMessage;
            base.Result = boolResultObject.Result;
            base.Context = boolResultObject.Context;
            base.Time = boolResultObject.Time;
            base.StatusCode = boolResultObject.StatusCode;
        }
    }
}
using java.lang;
using System.Collections.Generic;
using BMW.Rheingold.CoreFramework.Contracts;

namespace BMW.Rheingold.CoreFramework.Programming.Error
{
    public interface IErrorManager
    {
        BoolResultObject GetBoolResultObject(ErrorCode code, ContextError contextError, params string[] descriptionParams);
        BoolResultObject GetBoolResultObjectAndLogError(string methodName, ErrorCode code, ContextError contextError, params string[] descriptionParams);
        BoolResultObject GetBoolResultObjectAndLogException(string methodName, ErrorCode code, ContextError contextError, Exception ex);
        BoolResultObject GetBoolResultObjectAndLogException(string methodName, ErrorCode code, ContextError contextError, Exception ex, params string[] descriptionParams);
        BoolResultObject GetBoolResultObjectAndLogWarning(string methodName, ErrorCode code, ContextError contextError, params string[] descriptionParams);
        IError GetError(ErrorCode code, ContextError context, params string[] descriptionParams);
        IBoolResultObject GetErrorValidationBoolResultWithLoopingMessage(bool result, ErrorCode code, string messageSeparator, ContextError contextError, params string[][] descriptionParams);
        IError GetErrorWithLoopingMessage(ErrorCode code, ContextError context, string messageSeparator, params string[][] descriptionParams);
        Dictionary<ContextError, IBoolResultObject> getLastErrorContext();
    }
}

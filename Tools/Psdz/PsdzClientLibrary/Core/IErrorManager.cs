using java.lang;
using PsdzClient.Contracts;
using System.Collections.Generic;

namespace PsdzClient.Core
{
    public enum ContextError
    {
        SFA,
        SecureCoding,
        EcuValidation,
        SecureEcu,
        ComponentTheftProtection,
        Generic,
        Programming,
        VehicleOrderImport,
        Sec4Diag
    }

    public enum ErrorCode
    {
        AST00000,
        AST00001,
        AST00005,
        AST00006,
        AST00010,
        AST00011,
        AST00016,
        AST00017,
        ERR00000,
        ERR00001,
        ERR00002,
        ERR00003,
        ERR00004,
        ERR00005,
        ERR00006,
        ERR00007,
        EVS00000,
        EVS00001,
        EVS00002,
        EVS00003,
        EVS00004,
        EVS00005,
        EVS00006,
        EVS00007,
        EVS00008,
        EVS00009,
        EVS00010,
        EVS00011,
        EVS00012,
        FST00000,
        FST00001,
        FST00002,
        FST00003,
        FST00004,
        FST00005,
        FST00006,
        FST00007,
        FST00008,
        FST00009,
        FST00010,
        FST00011,
        PSZ00003,
        PSZ00004,
        PSZ00005,
        PSZ00006,
        PSZ00007,
        PSZ00008,
        PSZ00009,
        PSZ00010,
        PSZ00011,
        SEM00000,
        SEM00001,
        SEM00002,
        SEM00003,
        SEM00004,
        SEM00005,
        SEM00006,
        SEM00007,
        SEM00008,
        SEM00009,
        SEM00010,
        SEM00011,
        SEM00012,
        SEM00013,
        USR00000,
        SCB00000,
        SCB00001,
        SCB00002,
        SCB00003,
        SCB00004,
        SCB00005,
        CPS00000,
        CPS00001,
        CPS00002,
        STC00000,
        STC00001,
        STC00002,
        PVM00000,
        PVM00001,
        PVM00002,
        SWI00001,
        SWI00002,
        SWI00003,
        SWI20001,
        KDS00001,
        SDP00000,
        SDP00001,
        SDP00002,
        SDP00003,
        SDP00004
    }

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
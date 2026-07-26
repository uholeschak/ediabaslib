using PsdzClient.Contracts;
using PsdzClient.Core;
using System;
using System.ComponentModel;
using PsdzClient;

namespace BMW.Authoring.API.Interface.Rita
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [PreserveSource(Hint = "No update", SuppressWarning = true)]
    public interface IRitaFunctionsProvider
    {
        [Obsolete("This function is not supported")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IBoolResultObject ClearAllFaults();

        [Obsolete("This function is not supported please use DeleteFaultsStatusUpdate")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IBoolResultObject DeleteFaultsFailed(string errorCode);

        [Obsolete("This function is not supported")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IBoolResultObject ReadAndCalculateFaults();

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IBoolResultObject StartDeleteAllFaults();

        [Obsolete("This function is not supported please use DeleteFaultsStatusUpdate")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IBoolResultObject UpdateDtcAndFaultList();

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IBoolResultObject IsSilentDarkModeEnabled();

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IBoolResultObject DeleteFaultsStatusUpdate(string statusCode);
    }
}

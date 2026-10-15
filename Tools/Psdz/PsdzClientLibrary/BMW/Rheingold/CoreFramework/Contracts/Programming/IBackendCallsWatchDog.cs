using System.Collections.Generic;
using System.Net;
using System;
using BMW.ISPI.TRIC.ISTA.Contracts.Enums;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.CoreFramework.OnlinePatch;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    public interface IBackendCallsWatchDog
    {
        Dictionary<BackendServiceType, HttpStatusCode> LatestBackendResponse { get; }

        List<BackendServiceType> NotAvailableBackends { get; }

        int GetTotalCallCounter(BackendServiceType serviceType);
        int GetSpecificStatusCallCounter(BackendServiceType serviceType, HttpStatusCode statusCode);
        void AddBackendCall(BackendServiceType serviceType, HttpStatusCode? status, string refVersion = "", Exception ex = null);
        void AddBackendCall(BackendServiceType serviceType, HttpStatusCode? status, IFasta2Service fasta2Service, List<OnlinePatchDownloadStatus> patchFiles, List<string> nameFiles = null, string refVersion = "");
    }
}
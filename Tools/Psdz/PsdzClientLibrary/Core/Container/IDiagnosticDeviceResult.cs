using System;
using System.ComponentModel;
using PsdzClient.Core;
using PsdzClient.Core.Container;

namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IDiagnosticDeviceResult
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEcuJob ECUJob { get; }

        IAdapterError Error { get; }

        object getISTAResult(string resultName);

        T getISTAResultAs<T>(string resultName);

        object getISTAResultAsType(string resultName, Type targetType);

        T getResultAs<T>(string resultName);

        T getResultAs<T>(ushort set, string resultName);
    }
}

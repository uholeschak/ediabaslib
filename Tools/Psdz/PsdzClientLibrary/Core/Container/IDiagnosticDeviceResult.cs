using System;
using System.ComponentModel;

namespace PsdzClient.Core.Container
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

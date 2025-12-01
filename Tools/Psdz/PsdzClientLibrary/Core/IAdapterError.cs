using System;

namespace PsdzClient.Core
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IAdapterError
    {
        string AdapterFullClassName { get; }

        string Description { get; }

        Exception Exception { get; }
        long ID { get; }

        INativeError NativeError { get; }
    }
}

using System;
using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.CoreFramework.Contracts
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

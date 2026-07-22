using PsdzClient.Core;
using System;

namespace BMW.Rheingold.CoreFramework.Module
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    public interface IInputListener : IDisposable
    {
        bool InputReceived { get; }

        void StartListening();

        void StopListening();

        void Reset();
    }
}

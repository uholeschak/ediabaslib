using System;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [Obsolete("Use BMW.Authoring.Programming.API.Models.Interfaces.IIstufenTriple instead.")]
    public interface IIstufenTriple
    {
        string Current { get; }

        string Last { get; }

        string Shipment { get; }
    }
}
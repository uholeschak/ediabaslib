using System;

#pragma warning disable CS0618
namespace BMW.Authoring.Programming.API.Models.Implementation
{
    public class IstufenTriple : BMW.Authoring.Programming.API.Models.Interfaces.IIstufenTriple, IHideObjectMembers
    {
        public string Current { get; }

        public string Last { get; }

        public string Shipment { get; }

        public IstufenTriple(BMW.Rheingold.CoreFramework.Contracts.Programming.IIstufenTriple source)
        {
            Current = source?.Current;
            Last = source?.Last;
            Shipment = source?.Shipment;
        }

        Type IHideObjectMembers.GetType()
        {
            return GetType();
        }
    }
}

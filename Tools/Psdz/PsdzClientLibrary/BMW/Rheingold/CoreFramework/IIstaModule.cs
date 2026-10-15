using System;

namespace BMW.Rheingold.CoreFramework
{
    public interface IIstaModule : IDisposable
    {
        IResult ResultSet { get; }
    }
}

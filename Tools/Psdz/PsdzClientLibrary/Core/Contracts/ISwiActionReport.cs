using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    public interface ISwiActionReport
    {
        string Name { get; }

        string Category { get; }

        bool IsVisible { get; }

        IList<string> LinkedTo { get; }
    }

}

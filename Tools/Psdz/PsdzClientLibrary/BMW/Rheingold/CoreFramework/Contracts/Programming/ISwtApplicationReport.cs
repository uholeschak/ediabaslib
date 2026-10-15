using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    public interface ISwtApplicationReport
    {
        int DiagAddrAsInt { get; set; }

        FscState FscState { get; }

        ISwtApplicationId Id { get; }

        string Title { get; set; }

        IDictionary<string, string> TitleDictionary { get; set; }
    }
}

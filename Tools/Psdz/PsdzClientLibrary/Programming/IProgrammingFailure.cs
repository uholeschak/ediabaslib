using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    public interface IProgrammingFailure
    {
        string Id { get; }

        string MessageId { get; }

        string Message { get; }

        string CauseId { get; }

        string Source { get; }

        IDictionary<string, string> ContextInfo { get; }
    }
}
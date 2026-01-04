using PsdzClient.Programming;
using System.Collections.Generic;

namespace PsdzClient.Programming
{
    internal class ProgrammingFailure : IProgrammingFailure
    {
        private readonly IDictionary<string, string> contextInfo;

        public string Id { get; internal set; }

        public string MessageId { get; internal set; }

        public string Message { get; internal set; }

        public string CauseId { get; internal set; }

        public string Source { get; internal set; }

        public IDictionary<string, string> ContextInfo => contextInfo;

        public ProgrammingFailure()
        {
            contextInfo = new Dictionary<string, string>();
        }
    }
}
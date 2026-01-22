using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzHddUpdateTA : PsdzTa
    {
        public long SecondsToCompletion { get; set; }
    }
}
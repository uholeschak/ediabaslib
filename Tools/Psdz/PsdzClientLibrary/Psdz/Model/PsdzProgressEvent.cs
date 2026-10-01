using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Events
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzProgressEvent : PsdzEvent
    {
    }
}

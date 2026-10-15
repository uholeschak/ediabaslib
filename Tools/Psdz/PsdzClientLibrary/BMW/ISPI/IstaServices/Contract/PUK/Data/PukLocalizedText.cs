using System.Runtime.Serialization;

namespace BMW.ISPI.IstaServices.Contract.PUK.Data
{
    [DataContract]
    public class PukLocalizedText
    {
        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public string Language { get; set; }
    }
}

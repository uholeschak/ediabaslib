using System.Runtime.Serialization;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [DataContract]
    public class DealerSessionProperty
    {
        [DataMember]
        public string SessionPropertyName { get; set; }

        [DataMember]
        public string SessionPropertyValue { get; set; }

        public DealerSessionProperty()
        {
        }

        public DealerSessionProperty(string name, string value)
        {
            SessionPropertyName = name;
            SessionPropertyValue = value;
        }
    }
}
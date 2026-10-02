using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Programming.Controller.SecureCoding.Model
{
    [DataContract]
    internal class Status
    {
        [DataMember(Name = "code")]
        public readonly string code;
        [DataMember(Name = "errorMessage")]
        public readonly string errorMessage;
        [DataMember(Name = "appErrorId")]
        public readonly string appErrorId;
        public override string ToString()
        {
            return "StatusCode: " + code + " - ErrorMessage :" + errorMessage + " - AppErrorId:" + appErrorId;
        }
    }
}
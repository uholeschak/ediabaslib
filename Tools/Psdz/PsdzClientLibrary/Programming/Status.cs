using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Programming.Controller.SecureCoding.Model
{
    [DataContract]
    public class Status
    {
        internal new string ToString
        {
            get
            {
                return string.Concat(new string[]
                {
                    "StatusCode: ",
                    this.code,
                    " - ErrorMessage :",
                    this.errorMessage,
                    " - AppErrorId:",
                    this.appErrorId
                });
            }
        }

        [DataMember(Name = "code")]
        public readonly string code;

        [DataMember(Name = "errorMessage")]
        public readonly string errorMessage;

        [DataMember(Name = "appErrorId")]
        public readonly string appErrorId;
    }
}

﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model
{
    [DataContract]
    public class PsdzIstufe : IComparable<IPsdzIstufe>, IPsdzIstufe
    {
        [DataMember]
        public bool IsValid { get; set; }

        [DataMember(IsRequired = true)]
        public string Value { get; set; }

        public int CompareTo(IPsdzIstufe other)
        {
            if (other != null)
            {
                return string.Compare(this.Value, other.Value, true, CultureInfo.InvariantCulture);
            }
            return 1;
        }

        public override string ToString()
        {
            return this.Value;
        }
    }
}

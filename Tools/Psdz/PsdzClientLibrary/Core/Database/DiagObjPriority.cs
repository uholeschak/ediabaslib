using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    [DataContract]
    public class DiagObjPriority : IComparable
    {
        private string p;

        [DataMember]
        public string Prio
        {
            get
            {
                return p;
            }
            set
            {
                p = value;
            }
        }

        public DiagObjPriority()
        {
            p = "0";
        }

        public DiagObjPriority(DiagObjPrioritySymbol symbol)
        {
            if (symbol == DiagObjPrioritySymbol.F)
            {
                p = "F";
            }
            else
            {
                p = "M";
            }
        }

        public DiagObjPriority(decimal number)
        {
            p = Convert.ToString(number, CultureInfo.InvariantCulture);
        }

        public bool Equals(DiagObjPriority other)
        {
            if (other == null)
            {
                return false;
            }
            return string.Equals(p, other.p, StringComparison.Ordinal);
        }

        public int CompareTo(object obj)
        {
            if (!(obj is DiagObjPriority))
            {
                return -1;
            }
            DiagObjPriority diagObjPriority = (DiagObjPriority)obj;
            if (string.Equals(p, diagObjPriority.p, StringComparison.Ordinal))
            {
                return 0;
            }
            if (string.Equals(diagObjPriority.p, "M", StringComparison.Ordinal))
            {
                return 1;
            }
            if (string.Equals(p, "M", StringComparison.Ordinal))
            {
                return -1;
            }
            if (string.Equals(diagObjPriority.p, "F", StringComparison.Ordinal))
            {
                return 1;
            }
            if (string.Equals(p, "F", StringComparison.Ordinal))
            {
                return -1;
            }
            if (string.Equals(p, "0", StringComparison.Ordinal))
            {
                return 1;
            }
            if (string.Equals(diagObjPriority.p, "0", StringComparison.Ordinal))
            {
                return -1;
            }
            if (Convert.ToDecimal(p, CultureInfo.InvariantCulture) > Convert.ToDecimal(diagObjPriority.p, CultureInfo.InvariantCulture))
            {
                return 1;
            }
            return -1;
        }
    }
}

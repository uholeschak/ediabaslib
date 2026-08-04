using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace BMW.Rheingold.CoreFramework.Contracts.FASTA
{
    public struct JobResultData
    {
        public IList<LocalizedText> Titel;

        public IList<LocalizedText> ValueLocalized;

        public string Name;

        public string Value;

        public string Unit;

        public decimal? ValueAsDecimal
        {
            get
            {
                try
                {
                    return Convert.ToDecimal(Value, CultureInfo.InvariantCulture);
                }
                catch (Exception exception)
                {
                    Log.ErrorException("JobResultData.ValueAsDecimal", exception);
                    return null;
                }
            }
        }

        public bool IsEmpty => string.IsNullOrEmpty(Value);

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Unit.GetHashCode() ^ (ValueAsDecimal.HasValue ? ValueAsDecimal.GetHashCode() : Value.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is JobResultData jobResultData))
            {
                return false;
            }
            if (object.Equals(Name, jobResultData.Name) && object.Equals(Value, jobResultData.Value))
            {
                return object.Equals(Unit, jobResultData.Unit);
            }
            return false;
        }
    }
}

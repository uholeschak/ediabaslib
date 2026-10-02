using System;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Interfaces
{
    public interface IXepFaultCode
    {
        decimal ID { get; set; }

        string AUSBLENDINDEX { get; set; }

        string CODE { get; set; }

        string DATATYPE { get; set; }

        string DIAGNOSEINDEX { get; set; }

        decimal? ECUVARIANTID { get; set; }

        decimal? RELEVANCE { get; set; }

        string SCHEINFEHLER { get; set; }

        decimal? SICHERHEITSRELEVANT { get; set; }

        DateTime? VALIDFROM { get; set; }

        DateTime? VALIDTO { get; set; }

        decimal? WEIGHTING { get; set; }
    }
}

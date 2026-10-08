using System;
using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Obd
{
    public interface IPsdzObdData
    {
        IEnumerable<IPsdzObdTripleValue> ObdTripleValues { get; }
    }
}

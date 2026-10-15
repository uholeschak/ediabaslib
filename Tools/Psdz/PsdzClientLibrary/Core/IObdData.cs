using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.Programming.Data.Obd
{
    public interface IObdData
    {
        ICollection<IObdTripleValue> ObdTripleValues { get; }
    }
}

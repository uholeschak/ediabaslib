using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.CoreFramework.Programming.Data.Obd
{
    public interface IObdData
    {
        ICollection<IObdTripleValue> ObdTripleValues { get; }
    }
}

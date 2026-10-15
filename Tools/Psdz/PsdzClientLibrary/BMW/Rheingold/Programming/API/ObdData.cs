using System.Collections.Generic;
using System.Collections.ObjectModel;
using BMW.Rheingold.CoreFramework.Programming.Data.Obd;

namespace BMW.Rheingold.Programming.API
{
    public class ObdData : IObdData
    {
        public ICollection<IObdTripleValue> ObdTripleValues { get; set; }

        public ObdData()
        {
            ObdTripleValues = new Collection<IObdTripleValue>();
        }
    }
}
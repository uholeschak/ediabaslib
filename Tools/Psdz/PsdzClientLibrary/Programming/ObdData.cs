using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
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
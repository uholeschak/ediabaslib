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
        public ObdData()
        {
            this.ObdTripleValues = new Collection<IObdTripleValue>();
        }

        public ICollection<IObdTripleValue> ObdTripleValues { get; set; }
    }
}

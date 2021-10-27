using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
    [Serializable]
    public class CharacteristicSet : ICloneable
    {
        public Dictionary<long, long> Characteristics
        {
            get
            {
                return this.characteristics;
            }
            set
            {
                this.characteristics = value;
            }
        }

        public List<long> ProdDates
        {
            get
            {
                return this.prodDates;
            }
            set
            {
                this.prodDates = value;
            }
        }

        public object Clone()
        {
            return new CharacteristicSet
            {
                characteristics = new Dictionary<long, long>(this.characteristics),
                prodDates = new List<long>(this.prodDates)
            };
        }

        private Dictionary<long, long> characteristics = new Dictionary<long, long>();

        private List<long> prodDates = new List<long>();
    }
}

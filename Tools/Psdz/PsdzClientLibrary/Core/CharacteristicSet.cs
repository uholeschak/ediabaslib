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
        private Dictionary<long, long> characteristics = new Dictionary<long, long>();
        private List<long> prodDates = new List<long>();
        public Dictionary<long, long> Characteristics
        {
            get
            {
                return characteristics;
            }

            set
            {
                characteristics = value;
            }
        }

        public List<long> ProdDates
        {
            get
            {
                return prodDates;
            }

            set
            {
                prodDates = value;
            }
        }

        public object Clone()
        {
            CharacteristicSet characteristicSet = new CharacteristicSet();
            characteristicSet.characteristics = new Dictionary<long, long>(characteristics);
            characteristicSet.prodDates = new List<long>(prodDates);
            return characteristicSet;
        }
    }
}
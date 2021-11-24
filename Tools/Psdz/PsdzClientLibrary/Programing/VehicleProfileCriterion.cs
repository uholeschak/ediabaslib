using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public class VehicleProfileCriterion : IVehicleProfileCriterion
    {
        internal VehicleProfileCriterion(int value, string name, string nameEn)
        {
            this.Value = value;
            this.Name = name;
            this.NameEn = nameEn;
        }

        public string Name { get; private set; }

        public string NameEn { get; private set; }

        public int Value { get; private set; }
    }
}

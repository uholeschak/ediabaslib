using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    internal class VehicleProfileCriterion : IVehicleProfileCriterion
    {
        public string Name { get; private set; }
        public string NameEn { get; private set; }
        public int Value { get; private set; }

        internal VehicleProfileCriterion(int value, string name, string nameEn)
        {
            Value = value;
            Name = name;
            NameEn = nameEn;
        }
    }
}
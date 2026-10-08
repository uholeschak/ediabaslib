using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    public class PsdzFeatureSpecificFieldCto : IPsdzFeatureSpecificFieldCto
    {
        public int FieldType { get; set; }

        public string FieldValue { get; set; }
    }
}

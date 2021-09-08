using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public class PsdzFeatureSpecificFieldCto : IPsdzFeatureSpecificFieldCto
    {
        public int FieldType { get; set; }

        public string FieldValue { get; set; }
    }
}

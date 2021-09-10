using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    public interface IPsdzFeatureSpecificFieldCto
    {
        int FieldType { get; set; }

        string FieldValue { get; set; }
    }
}

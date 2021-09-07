using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public interface IPsdzFeatureSpecificFieldCto
    {
        int FieldType { get; set; }

        string FieldValue { get; set; }
    }
}

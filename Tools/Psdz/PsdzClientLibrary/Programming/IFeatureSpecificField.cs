using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    //[AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IFeatureSpecificField
    {
        int FieldType { get; set; }

        string FieldValue { get; set; }
    }
}

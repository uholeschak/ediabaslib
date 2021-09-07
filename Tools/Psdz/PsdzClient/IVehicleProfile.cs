using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    //[AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IVehicleProfile
    {
        string AsString { get; }

        IEnumerable<int> CategoryIds { get; }

        string GetCategoryNameById(int categoryId);

        IEnumerable<IVehicleProfileCriterion> GetCriteriaByCategoryId(int categoryId);

        string Entwicklungsbaureihe { get; }

        string Baureihenverbund { get; }
    }
}

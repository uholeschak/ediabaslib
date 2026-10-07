using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IVehicleProfile
    {
        string AsString { get; }

        IEnumerable<int> CategoryIds { get; }

        string Entwicklungsbaureihe { get; }

        string Baureihenverbund { get; }

        string GetCategoryNameById(int categoryId);
        IEnumerable<IVehicleProfileCriterion> GetCriteriaByCategoryId(int categoryId);
    }
}
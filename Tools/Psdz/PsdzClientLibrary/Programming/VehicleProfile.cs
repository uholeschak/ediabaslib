using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    internal class VehicleProfile : IVehicleProfile
    {
        private readonly IDictionary<int, IEnumerable<IVehicleProfileCriterion>> category2Criteria;
        private readonly IDictionary<int, string> categoryId2CategoryName;
        public string AsString { get; private set; }
        public IEnumerable<int> CategoryIds => categoryId2CategoryName.Keys;
        public string Entwicklungsbaureihe { get; internal set; }
        public string Baureihenverbund { get; internal set; }

        internal VehicleProfile(string asString, IDictionary<int, string> categoryId2CategoryName, IDictionary<int, IEnumerable<IVehicleProfileCriterion>> category2Criteria)
        {
            if (asString == null)
            {
                throw new ArgumentNullException("asString");
            }

            if (categoryId2CategoryName == null)
            {
                throw new ArgumentNullException("categoryId2CategoryName");
            }

            if (category2Criteria == null)
            {
                throw new ArgumentNullException("category2Criteria");
            }

            this.categoryId2CategoryName = categoryId2CategoryName;
            this.category2Criteria = category2Criteria;
            AsString = asString;
        }

        public string GetCategoryNameById(int categoryId)
        {
            if (!categoryId2CategoryName.ContainsKey(categoryId))
            {
                return null;
            }

            return categoryId2CategoryName[categoryId];
        }

        public IEnumerable<IVehicleProfileCriterion> GetCriteriaByCategoryId(int categoryId)
        {
            if (!category2Criteria.ContainsKey(categoryId))
            {
                return null;
            }

            return category2Criteria[categoryId];
        }
    }
}
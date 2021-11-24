using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public class VehicleProfile : IVehicleProfile
    {
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
            this.AsString = asString;
        }

        public string AsString { get; private set; }

        public IEnumerable<int> CategoryIds
        {
            get
            {
                return this.categoryId2CategoryName.Keys;
            }
        }

        public string GetCategoryNameById(int categoryId)
        {
            if (!this.categoryId2CategoryName.ContainsKey(categoryId))
            {
                return null;
            }
            return this.categoryId2CategoryName[categoryId];
        }

        public IEnumerable<IVehicleProfileCriterion> GetCriteriaByCategoryId(int categoryId)
        {
            if (!this.category2Criteria.ContainsKey(categoryId))
            {
                return null;
            }
            return this.category2Criteria[categoryId];
        }

        public string Entwicklungsbaureihe { get; internal set; }

        public string Baureihenverbund { get; internal set; }

        private readonly IDictionary<int, IEnumerable<IVehicleProfileCriterion>> category2Criteria;

        private readonly IDictionary<int, string> categoryId2CategoryName;
    }
}

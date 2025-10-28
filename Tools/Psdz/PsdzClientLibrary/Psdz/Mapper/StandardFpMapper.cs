using BMW.Rheingold.Psdz.Model;
using System.Collections.Generic;
using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal static class StandardFpMapper
    {
        public static IPsdzStandardFp Map(StandardFpModel model)
        {
            if (model == null)
            {
                return null;
            }
            Dictionary<int, IList<IPsdzStandardFpCriterion>> dictionary = new Dictionary<int, IList<IPsdzStandardFpCriterion>>();
            if (model.Category2Criteria != null)
            {
                foreach (KeyValuePair<string, ICollection<StandardFpCriterionModel>> category2Criterion in model.Category2Criteria)
                {
                    dictionary.Add(int.Parse(category2Criterion.Key), category2Criterion.Value?.Select(StandardFpCriterionMapper.Map)?.ToList());
                }
            }
            Dictionary<int, string> dictionary2 = new Dictionary<int, string>();
            if (model.CategoryId2CategoryName != null)
            {
                foreach (KeyValuePair<string, string> item in model.CategoryId2CategoryName)
                {
                    dictionary2.Add(int.Parse(item.Key), item.Value);
                }
            }
            return new PsdzStandardFp
            {
                AsString = model.AsString,
                Category2Criteria = dictionary,
                CategoryId2CategoryName = dictionary2,
                IsValid = model.IsValid
            };
        }

        public static StandardFpModel Map(IPsdzStandardFp model)
        {
            if (model == null)
            {
                return null;
            }
            Dictionary<string, ICollection<StandardFpCriterionModel>> dictionary = new Dictionary<string, ICollection<StandardFpCriterionModel>>();
            if (model.Category2Criteria != null)
            {
                foreach (KeyValuePair<int, IList<IPsdzStandardFpCriterion>> category2Criterion in model.Category2Criteria)
                {
                    dictionary.Add(category2Criterion.Key.ToString(), category2Criterion.Value?.Select(StandardFpCriterionMapper.Map)?.ToList());
                }
            }
            Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
            if (model.CategoryId2CategoryName != null)
            {
                foreach (KeyValuePair<int, string> item in model.CategoryId2CategoryName)
                {
                    dictionary2.Add(item.Key.ToString(), item.Value);
                }
            }
            return new StandardFpModel
            {
                AsString = model.AsString,
                Category2Criteria = dictionary,
                CategoryId2CategoryName = dictionary2,
                IsValid = model.IsValid
            };
        }
    }
}
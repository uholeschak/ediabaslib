using System.Linq;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class StandardSvkMapper
    {
        public static IPsdzStandardSvk Map(StandardSvkModel standardSvkModel)
        {
            if (standardSvkModel == null)
            {
                return null;
            }

            return new PsdzStandardSvk
            {
                ProgDepChecked = standardSvkModel.ProgDepChecked,
                SgbmIds = standardSvkModel.SgbmIds?.Select(SgbmIdMapper.Map),
                SvkVersion = standardSvkModel.SvkVersion
            };
        }

        public static StandardSvkModel Map(IPsdzStandardSvk standardSvk)
        {
            if (standardSvk == null)
            {
                return null;
            }

            return new StandardSvkModel
            {
                ProgDepChecked = standardSvk.ProgDepChecked,
                SgbmIds = standardSvk.SgbmIds?.Select(SgbmIdMapper.Map).ToList(),
                SvkVersion = standardSvk.SvkVersion
            };
        }
    }
}
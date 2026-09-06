using System;
using PsdzClient.Core;
using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class XEP_ECUJOBSEX_ParameterSequenceComparer : IComparer<XEP_ECUPARAMETERSEX>
    {
        public int Compare(XEP_ECUPARAMETERSEX x, XEP_ECUPARAMETERSEX y)
        {
            try
            {
                return SortUtils.CompareString(x.Name, y.Name, ignoreLength: false);
            }
            catch (Exception exception)
            {
                Log.WarningException("XEP_ECUJOBSEX_ParameterSequenceComparer.Compare()", exception);
            }
            return 0;
        }
    }
}

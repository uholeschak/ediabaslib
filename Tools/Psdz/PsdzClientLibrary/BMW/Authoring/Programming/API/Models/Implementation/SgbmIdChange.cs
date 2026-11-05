using System;

#pragma warning disable CS0618
namespace BMW.Authoring.Programming.API.Models.Implementation
{
    public class SgbmIdChange : BMW.Authoring.Programming.API.Models.Interfaces.ISgbmIdChange, IHideObjectMembers
    {
        public string Actual { get; }

        public string Target { get; }

        public SgbmIdChange(BMW.Rheingold.CoreFramework.Contracts.Programming.ISgbmIdChange source)
        {
            if (source != null)
            {
                Actual = source.Actual;
                Target = source.Target;
            }
        }

        Type IHideObjectMembers.GetType()
        {
            return GetType();
        }
    }
}

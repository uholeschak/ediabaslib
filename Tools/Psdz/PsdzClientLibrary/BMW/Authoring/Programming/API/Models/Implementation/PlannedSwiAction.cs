using System;

namespace BMW.Authoring.Programming.API.Models.Implementation
{
    public class PlannedSwiAction : BMW.Authoring.Programming.API.Models.Interfaces.IPlannedSwiAction, IHideObjectMembers
    {
        public string SwiActionName { get; }

        public bool IsDisabled { get; }

        public PlannedSwiAction(BMW.Rheingold.CoreFramework.Contracts.Vehicle.IPlannedSwiAction source)
        {
            if (source != null)
            {
                SwiActionName = source.SwiActionName;
                IsDisabled = source.IsDisabled;
            }
        }

        public PlannedSwiAction(string swiActionName, bool isDisabled)
        {
            SwiActionName = swiActionName;
            IsDisabled = isDisabled;
        }

        Type IHideObjectMembers.GetType()
        {
            return GetType();
        }
    }
}

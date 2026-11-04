using System;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [Obsolete("Use BMW.Authoring.Programming.API.Models.Interfaces.IPlannedSwiAction instead.")]
    public interface IPlannedSwiAction
    {
        string SwiActionName { get; set; }

        bool IsDisabled { get; set; }
    }
}
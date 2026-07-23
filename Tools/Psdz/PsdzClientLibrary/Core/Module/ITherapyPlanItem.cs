using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;
using System.ComponentModel;
using BMW.Rheingold.CoreFramework.Contracts.Programming;

namespace PBMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ITherapyPlanItem : INotifyPropertyChanged
    {
        ITherapyPlanItemSource Source { get; }

        TherapyPlanItemOrigin Origin { get; }

        string OriginText { get; }

        TherapyPlanItemCategory Category { get; }

        IEcu Ecu { get; }

        string Identifikator { get; }
    }
}

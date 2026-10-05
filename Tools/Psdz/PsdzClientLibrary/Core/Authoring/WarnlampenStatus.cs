using PsdzClient.Core;
using System.ComponentModel;
using BMW.Rheingold.CoreFramework;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public enum WarnlampenStatus
    {
        Unknown,
        DoesNotActivateMalfunctionIndicatorLamp,
        ActivatedMalfunctionIndicatorLamp,
        ActivatesMalfunctionIndicatorLamp,
        TriggersCheckControlMessage,
        TriggersCheckControlMessageOrEmlWarningLight
    }
}

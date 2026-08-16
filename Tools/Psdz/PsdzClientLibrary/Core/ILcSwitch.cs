using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ILcSwitch
    {
        string Number { get; set; }

        string NumberText { get; set; }

        string Value { get; set; }

        string ValueText { get; set; }
    }
}

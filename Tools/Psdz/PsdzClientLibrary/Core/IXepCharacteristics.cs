using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Interfaces
{
    public interface IXepCharacteristics : IMultilanguageTitle
    {
        string DriveId { get; }

        decimal Id { get; }

        decimal? IstaVisible { get; }

        string LegacyName { get; }

        string Name { get; }

        decimal? Nodeclass { get; }

        decimal? ParentId { get; }

        decimal RootNodeClass { get; set; }

        decimal? StaticClassVariables { get; }

        decimal? StaticClassVariablesMotorrad { get; }

        string Title { get; }

        decimal? TitleId { get; }
    }
}

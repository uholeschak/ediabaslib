using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces;
using System;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public interface IXepEcuVariants : IMultilanguageTitle
    {
        decimal Id { get; }

        string Name { get; }

        decimal? FaultMemoryDeleteWaitingTime { get; }

        DateTime? ValidFrom { get; }

        DateTime? ValidTo { get; }

        decimal? EcuGroupId { get; }

        decimal? Sort { get; }

        decimal? Sicherheitsrelevant { get; }

        string Title { get; }

        decimal? TitleId { get; }
    }
}

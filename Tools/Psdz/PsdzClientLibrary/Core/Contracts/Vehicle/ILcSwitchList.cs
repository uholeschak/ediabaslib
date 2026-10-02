using PsdzClient.Core;
using System.Collections;
using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ILcSwitchList : IList<ILcSwitch>, ICollection<ILcSwitch>, IEnumerable<ILcSwitch>, IEnumerable
    {
        new int Count { get; }
    }
}

using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISwt
    {
        IEnumerable<ISwtEcu> Ecus { get; }

        ISwtApplication GetSwtApplication(int diagAddrAsInt, ISwtApplicationId swtApplicationId);
    }
}

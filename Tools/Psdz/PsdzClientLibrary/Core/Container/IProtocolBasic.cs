using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using PsdzClient;

namespace BMW.Rheingold.CoreFramework.Contracts.FASTA
{
    [PreserveSource(Hint = "Dummy interface", SuppressWarning = true)]
    public interface IProtocolBasic : IProtocolBasicBase, IFastaGroupingBase, IFastaGrouping
    {
        //object AddMultiLanguageEFuseInfoTable(string infoTitle, Dictionary<string, TableData> multiLanguageTableData, DateTime startTime);
        IAction<IUiDialog> CreateAndAddUiDialogFromServiceProgram(string type, string methodName);
    }
}
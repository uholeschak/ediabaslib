using BMW.Rheingold.CoreFramework.Contracts.FASTA;

namespace PsdzClient.Core.Container
{
    [PreserveSource(Hint = "Dummy interface", SuppressWarning = true)]
    public interface IProtocolBasic : IProtocolBasicBase, IFastaGroupingBase, IFastaGrouping
    {
        //object AddMultiLanguageEFuseInfoTable(string infoTitle, Dictionary<string, TableData> multiLanguageTableData, DateTime startTime);
        IAction<IUiDialog> CreateAndAddUiDialogFromServiceProgram(string type, string methodName);
    }
}
using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework
{
    [AuthorAPI]
    public enum CollectiveResultSet
    {
        Ok,
        Verified,
        NotOk,
        Unknown,
        Repaired,
        None,
        ServiceProgramInvokeException
    }
}

using PsdzClient.Programming;
using System.ComponentModel;

namespace BMW.Authoring.Session
{
    [EditorBrowsable(EditorBrowsableState.Always)]
    public interface ITestPlan : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Calculated { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        int Count { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        typeDiagObjectState GetExecutionStatus(long controlId);

        [EditorBrowsable(EditorBrowsableState.Always)]
        typeDiagObjectState GetExecutionStatus(string identifier);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Contains(long id);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Contains(string identifier);
    }
}

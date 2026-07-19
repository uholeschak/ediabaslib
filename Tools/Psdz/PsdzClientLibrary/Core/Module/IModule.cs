using PsdzClient.Programming;
using System.ComponentModel;
using BMW.Rheingold.CoreFramework.DatabaseProvider;

namespace BMW.Rheingold.CoreFramework
{
    public interface IModule : INotifyPropertyChanged
    {
        bool IsActive { get; }

        bool IsExecutionCompleted { get; }

        ModuleExecutionStateType ModuleState { get; }

        string Title { get; }

        InfoObject InfoObj { get; }

        ModuleExecutionOrigin ExecutedFrom { get; }

        bool IsIdesModule { get; }

        bool IsModuleExecutionRunning { get; }

        string VisibleName { get; }

        typeDiagObjectState Status { get; }

        string Name { get; }

        bool IsMinimizable { get; }

        bool IsModuleExecutionMinimized { get; }

        string InfoType { get; }
    }
}

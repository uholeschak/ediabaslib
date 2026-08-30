using BMW.Rheingold.CoreFramework.Contracts;
using System.Threading.Tasks;

namespace BMW.Rheingold.CoreFramework
{
    public interface IModuleExecutionHandle : IAbortable
    {
        IModule ExecutingModule { get; }

        Task<IResult> ModuleTask { get; }
    }
}

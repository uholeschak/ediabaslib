using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.DatabaseProvider;

namespace BMW.Rheingold.CoreFramework.Module
{
    public interface IModuleImpl
    {
        ModuleData Data { get; }

        void Initialize(ModuleExecutionOrigin origin, IXepInfoObject infoObjToStart, bool gui, string subModulePath, string testmoduleType, IServiceProgramProgramming infoObjPrg);

        void Initialize(ModuleExecutionOrigin origin, InfoObject infoObjToStart, bool gui, string subModulePath, string testmoduleType);

        IModuleExecutionHandle Execute(bool foreground, bool overall = false, bool exception = false);
    }
}

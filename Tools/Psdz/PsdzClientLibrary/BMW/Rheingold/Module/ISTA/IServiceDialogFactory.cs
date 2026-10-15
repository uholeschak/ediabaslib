using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.Module.ISTA
{
    public interface IServiceDialogFactory
    {
        IServiceDialog CreateServiceDialog(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo, ParameterContainer inParameters, ParameterContainer inoutParameters);
    }
}

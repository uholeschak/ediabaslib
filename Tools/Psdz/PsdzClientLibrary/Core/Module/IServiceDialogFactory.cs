using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core.Container;

namespace BMW.Rheingold.Module.ISTA
{
    public interface IServiceDialogFactory
    {
        IServiceDialog CreateServiceDialog(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo, ParameterContainer inParameters, ParameterContainer inoutParameters);
    }
}

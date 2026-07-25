using PsdzClient.Core.Container;

namespace BMW.Rheingold.Module.ISTA
{
    public interface IServiceDialog
    {
        void Invoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam);
    }
}

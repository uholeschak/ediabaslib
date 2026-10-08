using BMW.ISPI.TRIC.ISTA.Contracts.Enums;

namespace BMW.Rheingold.InfoProvider.Sec4Diag.Factories
{
    public class Sec4DiagProcessorFactory
    {
        public static ISec4DiagProcessor Create(IBackendCallsWatchDog backendCallWatchDog)
        {
            return new Sec4DiagProcessor(Sec4DiagProcessorImplFactory.Create(backendCallWatchDog));
        }
    }
}
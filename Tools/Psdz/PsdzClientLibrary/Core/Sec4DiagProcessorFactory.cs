namespace PsdzClient.Core
{
    public class Sec4DiagProcessorFactory
    {
        public static ISec4DiagProcessor Create(IBackendCallsWatchDog backendCallWatchDog)
        {
            return new Sec4DiagProcessor(Sec4DiagProcessorImplFactory.Create(backendCallWatchDog));
        }
    }
}
using PsdzClient;

namespace BMW.iLean.CommonServices.Logging
{
    [PreserveSource(Hint = "Don't update, only used for logging", SuppressWarning = true)]
    public enum EventKind
    {
        Technical,
        Functional
    }
}
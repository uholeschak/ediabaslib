namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    public enum DeviceState
    {
        Init,
        Booted,
        Lost,
        Sleep,
        Free,
        Reserved,
        Selftest,
        Fail,
        Found,
        Transit,
        Updated,
        Unregistered,
        Blocked,
        FreeNvm,
        FirmwareOutdated,
        Unsupported
    }
}
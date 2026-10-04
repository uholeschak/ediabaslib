namespace RheingoldPsdzWebApi.Adapter.Contracts.Services
{
    public interface IHttpServerService
    {
        bool Start();

        bool Stop();
    }
}
namespace RheingoldPsdzWebApi.Adapter
{
    internal interface IPsdzWebservicePreflightChecker
    {
        void Execute(int sessionWebservicePort, string jarPath, string javaExePath);
    }
}
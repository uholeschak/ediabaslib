namespace BMW.Rheingold.Psdz
{
    internal interface IPsdzWebservicePreflightChecker
    {
        void Execute(int sessionWebservicePort, string jarPath, string javaExePath);
    }
}
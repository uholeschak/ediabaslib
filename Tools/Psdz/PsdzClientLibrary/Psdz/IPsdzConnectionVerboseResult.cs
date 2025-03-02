namespace BMW.Rheingold.Psdz.Client
{
    public interface IPsdzConnectionVerboseResult
    {
        bool CheckConnection { get; }

        string Message { get; }
    }
}
namespace BMW.Rheingold.Psdz.Model
{
    public interface IPsdzSvt : IPsdzStandardSvt
    {
        bool IsValid { get; }

        string Vin { get; }
    }
}

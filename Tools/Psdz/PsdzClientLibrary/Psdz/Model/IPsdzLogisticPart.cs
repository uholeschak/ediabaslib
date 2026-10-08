namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Svb
{
    public interface IPsdzLogisticPart
    {
        string NameTais { get; }

        string SachNrTais { get; }

        int Typ { get; }

        string ZusatzTextRef { get; }
    }
}

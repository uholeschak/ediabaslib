
namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Svb
{
    public interface IPsdzSollverbauung
    {
        string AsXml { get; }

        IPsdzSvt Svt { get; }

        IPsdzOrderList PsdzOrderList { get; }
    }
}

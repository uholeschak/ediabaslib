using RheingoldPsdzWebApi.Adapter.Contracts.Model.Svb;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal class LogisticPartMapper
    {
        internal static TTarget Map<TTarget>(LogisticPartModel model)
            where TTarget : PsdzLogisticPart, new()
        {
            if (model == null)
            {
                return null;
            }

            return new TTarget
            {
                NameTais = model.NameTais,
                SachNrTais = model.SachNrTais,
                Typ = model.Typ,
                ZusatzTextRef = model.ZusatzTextRef
            };
        }

        internal static TTarget Map<TTarget>(IPsdzLogisticPart psdzLogisticPart)
            where TTarget : LogisticPartModel, new()
        {
            if (psdzLogisticPart == null)
            {
                return null;
            }

            return new TTarget
            {
                NameTais = psdzLogisticPart.NameTais,
                SachNrTais = psdzLogisticPart.SachNrTais,
                Typ = psdzLogisticPart.Typ,
                ZusatzTextRef = psdzLogisticPart.ZusatzTextRef
            };
        }
    }
}
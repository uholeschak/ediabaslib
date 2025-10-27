using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz.Model;

namespace BMW.Rheingold.Psdz
{
    internal static class ConnectionMapper
    {
        public static IPsdzConnection Map(ConnectionModel connectionModel)
        {
            if (connectionModel == null)
            {
                return null;
            }
            return new PsdzConnection
            {
                Id = connectionModel.Id,
                Port = connectionModel.Port,
                TargetSelector = TargetSelectorMapper.Map(connectionModel.TargetSelector)
            };
        }

        public static IPsdzConnectionVerboseResult Map(CheckConnectionVerboseResultModel result)
        {
            return new PsdzConnectionVerboseResult
            {
                CheckConnection = result.ConnectionWorking,
                Message = result.Message
            };
        }
    }
}
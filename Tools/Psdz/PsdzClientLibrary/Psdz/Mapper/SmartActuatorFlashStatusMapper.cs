using System.Collections.Generic;
using System.Linq;
using PsdzClient.Programming;

namespace BMW.Rheingold.Psdz
{
    internal static class SmartActuatorFlashStatusMapper
    {
        public static IList<PsdzSmartActuatorFlashStatusResult> Map(ICollection<SmartActuatorFlashStatusModel> model)
        {
            return model?.Select((SmartActuatorFlashStatusModel item) => (item != null) ? new PsdzSmartActuatorFlashStatusResult
            {
                DebugInformation = (item?.DebugInformation ?? 0),
                ProgrammingStatus = item?.ProgrammingStatus,
                SmartActuatorId = item?.SmartActuatorID
            } : null).ToList();
        }

        public static ICollection<SmartActuatorFlashStatusModel> Map(IList<PsdzSmartActuatorFlashStatusResult> psdzSmartActuatorFlashStatusResult)
        {
            return psdzSmartActuatorFlashStatusResult?.Select((PsdzSmartActuatorFlashStatusResult item) => (item != null) ? new SmartActuatorFlashStatusModel
            {
                SmartActuatorID = item.SmartActuatorId,
                ProgrammingStatus = item.ProgrammingStatus,
                DebugInformation = item.DebugInformation
            } : null).ToList();
        }
    }
}

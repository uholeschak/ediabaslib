using BMW.Rheingold.Psdz.Model.Tal;
using BMW.Rheingold.Psdz.Model.Tal.TalStatus;

namespace BMW.Rheingold.Psdz
{
    internal static class FailureCauseMapper
    {
        public static IPsdzFailureCause Map(FailureCauseModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzFailureCause
            {
                Id = model.Id,
                IdReference = model.IdReference,
                Message = model.Message,
                MessageId = model.MessageId,
                Timestamp = model.Timestamp,
                TalElement = TalElementMapper.Map<PsdzTalElement>(model.TalElement)
            };
        }

        public static FailureCauseModel Map(IPsdzFailureCause psdzFailureCause)
        {
            if (psdzFailureCause == null)
            {
                return null;
            }

            return new FailureCauseModel
            {
                Id = psdzFailureCause.Id,
                IdReference = psdzFailureCause.IdReference,
                Message = psdzFailureCause.Message,
                MessageId = psdzFailureCause.MessageId,
                Timestamp = psdzFailureCause.Timestamp,
                TalElement = TalElementMapper.Map(psdzFailureCause.TalElement)
            };
        }
    }
}
using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Events;

namespace BMW.Rheingold.Psdz
{
    internal class TransactionInfoMapper : MapperBase<PsdzTransactionInfo, TransactionInfoModel>
    {
        protected override IDictionary<PsdzTransactionInfo, TransactionInfoModel> CreateMap()
        {
            return new Dictionary<PsdzTransactionInfo, TransactionInfoModel>
            {
                {
                    PsdzTransactionInfo.Started,
                    TransactionInfoModel.ACTION_STARTED
                },
                {
                    PsdzTransactionInfo.Repeating,
                    TransactionInfoModel.ACTION_REPEATING
                },
                {
                    PsdzTransactionInfo.Finished,
                    TransactionInfoModel.ACTION_FINISHED
                },
                {
                    PsdzTransactionInfo.FinishedWithError,
                    TransactionInfoModel.ACTION_FINISHED_WITH_ERROR
                },
                {
                    PsdzTransactionInfo.ProgressInfo,
                    TransactionInfoModel.ACTION_PROGRESSINFO
                }
            };
        }
    }
}
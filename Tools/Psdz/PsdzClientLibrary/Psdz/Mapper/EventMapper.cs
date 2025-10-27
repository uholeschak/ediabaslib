using System;
using BMW.Rheingold.Psdz.Model.Events;

namespace BMW.Rheingold.Psdz
{
    internal static class EventMapper
    {
        private static TaCategoryTypeMapper _taCategoryTypeMapper = new TaCategoryTypeMapper();

        private static TransactionInfoMapper _transactionInfoMapper = new TransactionInfoMapper();

        public static IPsdzEvent Map(EventModel eventModel)
        {
            if (eventModel == null)
            {
                return null;
            }
            Type type = eventModel.GetType();
            if (type == typeof(MCDDiagServiceEventModel))
            {
                MCDDiagServiceEventModel mCDDiagServiceEventModel = (MCDDiagServiceEventModel)eventModel;
                return new PsdzMcdDiagServiceEvent
                {
                    EcuId = EcuIdentifierMapper.Map(mCDDiagServiceEventModel.EcuId),
                    ErrorId = mCDDiagServiceEventModel.ErrorId,
                    ErrorName = mCDDiagServiceEventModel.ErrorName,
                    EventId = mCDDiagServiceEventModel.EventId,
                    IsTimingEvent = mCDDiagServiceEventModel.IsTimingEvent,
                    JobName = mCDDiagServiceEventModel.JobName,
                    LinkName = mCDDiagServiceEventModel.LinkName,
                    Message = mCDDiagServiceEventModel.Message,
                    MessageId = mCDDiagServiceEventModel.MessageId,
                    ResponseType = mCDDiagServiceEventModel.ResponseType.ToString(),
                    ServiceName = mCDDiagServiceEventModel.ServiceName,
                    Timestamp = mCDDiagServiceEventModel.Timestamp
                };
            }
            if (type == typeof(ProgressEventModel))
            {
                ProgressEventModel progressEventModel = (ProgressEventModel)eventModel;
                return new PsdzProgressEvent
                {
                    EcuId = EcuIdentifierMapper.Map(progressEventModel.EcuId),
                    EventId = progressEventModel.EventId,
                    Message = progressEventModel.Message,
                    MessageId = progressEventModel.MessageId,
                    Timestamp = progressEventModel.Timestamp
                };
            }
            if (type == typeof(TransactionEventModel))
            {
                TransactionEventModel transactionEventModel = (TransactionEventModel)eventModel;
                return new PsdzTransactionEvent
                {
                    EcuId = EcuIdentifierMapper.Map(transactionEventModel.EcuId),
                    EventId = transactionEventModel.EventId,
                    Message = transactionEventModel.Message,
                    MessageId = transactionEventModel.MessageId,
                    Timestamp = transactionEventModel.Timestamp,
                    TransactionInfo = _transactionInfoMapper.GetValue(transactionEventModel.TransactionInfo),
                    TransactionType = _taCategoryTypeMapper.GetValue(transactionEventModel.TransactionType)
                };
            }
            if (type == typeof(TransactionProgressEventModel))
            {
                TransactionProgressEventModel transactionProgressEventModel = (TransactionProgressEventModel)eventModel;
                return new PsdzTransactionProgressEvent
                {
                    EcuId = EcuIdentifierMapper.Map(transactionProgressEventModel.EcuId),
                    EventId = transactionProgressEventModel.EventId,
                    Message = transactionProgressEventModel.Message,
                    MessageId = transactionProgressEventModel.MessageId,
                    Timestamp = transactionProgressEventModel.Timestamp,
                    TransactionInfo = _transactionInfoMapper.GetValue(transactionProgressEventModel.BaseTransactionEvent.TransactionInfo),
                    TransactionType = _taCategoryTypeMapper.GetValue(transactionProgressEventModel.BaseTransactionEvent.TransactionType),
                    Progress = transactionProgressEventModel.Progress,
                    TaProgress = transactionProgressEventModel.TaProgress
                };
            }
            return new PsdzEvent
            {
                EcuId = EcuIdentifierMapper.Map(eventModel.EcuId),
                EventId = eventModel.EventId,
                Message = eventModel.Message,
                MessageId = eventModel.MessageId,
                Timestamp = eventModel.Timestamp
            };
        }
    }
}
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Events;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal class ConnectionLossEventListener : IPsdzEventListener, IConnectionLossEventListener
    {
        private readonly List<(DateTime timeRecieved, PsdzMcdDiagServiceEvent serviceEvent)> mcdDiagServiceEvents = new List<(DateTime, PsdzMcdDiagServiceEvent)>();

        public void SetPsdzEvent(IPsdzEvent psdzEvent)
        {
            if (psdzEvent is PsdzMcdDiagServiceEvent item)
            {
                mcdDiagServiceEvents.Add((DateTime.Now, item));
            }
        }

        public void LogConnectionLossEventMessages()
        {
            Log.Info(Log.CurrentMethod(), "All Recieved PsdzMcdDiagServiceEvents :\r\n(DateTime) : (Message);\r\n" + string.Join("\r\n", mcdDiagServiceEvents.Select(((DateTime timeRecieved, PsdzMcdDiagServiceEvent serviceEvent) c) => $"{c.timeRecieved} : {c.serviceEvent.Message}")));
            IEnumerable<IGrouping<string, PsdzMcdDiagServiceEvent>> groupedMcdDiagServiceEvents = from c in mcdDiagServiceEvents
                                                                                                  select c.serviceEvent into c
                                                                                                  group c by c.Message;
            LogMcdDiagServiceEntriesByGroupedList(groupedMcdDiagServiceEvents, "Message");
            IEnumerable<IGrouping<string, PsdzMcdDiagServiceEvent>> groupedMcdDiagServiceEvents2 = from c in mcdDiagServiceEvents
                                                                                                   select c.serviceEvent into c
                                                                                                   group c by c.ServiceName;
            LogMcdDiagServiceEntriesByGroupedList(groupedMcdDiagServiceEvents2, "ServiceName");
            IEnumerable<IGrouping<string, PsdzMcdDiagServiceEvent>> groupedMcdDiagServiceEvents3 = from c in mcdDiagServiceEvents
                                                                                                   select c.serviceEvent into c
                                                                                                   group c by c.LinkName;
            LogMcdDiagServiceEntriesByGroupedList(groupedMcdDiagServiceEvents3, "LinkName");
            IEnumerable<IGrouping<string, PsdzMcdDiagServiceEvent>> groupedMcdDiagServiceEvents4 = from c in mcdDiagServiceEvents
                                                                                                   select c.serviceEvent into c
                                                                                                   group c by c.JobName;
            LogMcdDiagServiceEntriesByGroupedList(groupedMcdDiagServiceEvents4, "JobName");
            _ = from c in mcdDiagServiceEvents
                select c.serviceEvent into c
                group c by c.EventId;
            LogMcdDiagServiceEntriesByGroupedList(groupedMcdDiagServiceEvents4, "EventId");
            _ = from c in mcdDiagServiceEvents
                select c.serviceEvent into c
                group c by c.ErrorId;
            LogMcdDiagServiceEntriesByGroupedList(groupedMcdDiagServiceEvents4, "ErrorId");
        }

        private void LogMcdDiagServiceEntriesByGroupedList(IEnumerable<IGrouping<string, PsdzMcdDiagServiceEvent>> groupedMcdDiagServiceEvents, string groupedBy)
        {
            List<string> list = new List<string>();
            foreach (IGrouping<string, PsdzMcdDiagServiceEvent> groupedMcdDiagServiceEvent in groupedMcdDiagServiceEvents)
            {
                string text = string.Join(", ", from c in groupedMcdDiagServiceEvent.Select((PsdzMcdDiagServiceEvent c) => c.EcuId).Distinct()
                                                select (c != null) ? $"{c.BaseVariant}:{c.DiagnosisAddress}" : "null");
                string text2 = string.Join(", ", groupedMcdDiagServiceEvent.Select((PsdzMcdDiagServiceEvent c) => c.ErrorId).Distinct());
                string text3 = string.Join(", ", groupedMcdDiagServiceEvent.Select((PsdzMcdDiagServiceEvent c) => c.ErrorName).Distinct());
                string.Join(", ", groupedMcdDiagServiceEvent.Select((PsdzMcdDiagServiceEvent c) => c.EventId).Distinct());
                string text4 = string.Join(", ", groupedMcdDiagServiceEvent.Select((PsdzMcdDiagServiceEvent c) => c.IsTimingEvent).Distinct());
                string text5 = string.Join(", ", groupedMcdDiagServiceEvent.Select((PsdzMcdDiagServiceEvent c) => c.JobName).Distinct());
                string text6 = string.Join(", ", groupedMcdDiagServiceEvent.Select((PsdzMcdDiagServiceEvent c) => c.LinkName).Distinct());
                string text7 = string.Join(", ", groupedMcdDiagServiceEvent.Select((PsdzMcdDiagServiceEvent c) => c.ResponseType).Distinct());
                string text8 = string.Join(", ", groupedMcdDiagServiceEvent.Select((PsdzMcdDiagServiceEvent c) => c.ServiceName).Distinct());
                list.Add($"{groupedMcdDiagServiceEvent.Count()}: {groupedMcdDiagServiceEvent.Key}; " + "ecuIds:(" + text + "); errorIds:(" + text2 + "); errorNames:(" + text3 + "); isTimingEvents:(" + text4 + "); jobNames:(" + text5 + "); linkNames:(" + text6 + "); responseTypes:(" + text7 + "); serviceNames:(" + text8 + "); ");
            }
            Log.Info(Log.CurrentMethod(), "PsdzMcdDiagServiceEvents grouped by " + groupedBy + ":\r\n(" + groupedBy + "Count) : (Distinct Events Infos)\r\n" + string.Join("\r\n", list));
        }
    }
}
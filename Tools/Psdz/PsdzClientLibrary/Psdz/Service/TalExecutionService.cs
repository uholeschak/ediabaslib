using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Events;
using BMW.Rheingold.Psdz.Model.Tal;
using PsdzClient.Contracts;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace BMW.Rheingold.Psdz
{
    public class TalExecutionService : ITalExecutionService, ILifeCycleDependencyProvider
    {
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly IWebCallHandler _webCallHandler;

        private readonly IProgrammingService _programmingService;

        private readonly IObjectBuilderService _objectBuilderService;

        private readonly IEventManagerService eventManagerService;

        private readonly IPsdzProgressListener progressListener;

        private int _activeTalExecutions;

        private bool _ignoreTalRelease = ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Psdz.IgnoreTalRelease", defaultValue: true);

        public string Description { get; }

        public string Name { get; }

        public event EventHandler<DependencyCountChangedEventArgs> ActiveDependencyCountChanged;

        public TalExecutionService(IWebCallHandler webCallHandler, IProgrammingService programmingService, IObjectBuilderService objectBuilderService, IEventManagerService eventManagerService, IPsdzProgressListener progressListener)
        {
            _webCallHandler = webCallHandler;
            _programmingService = programmingService;
            _objectBuilderService = objectBuilderService;
            this.eventManagerService = eventManagerService;
            this.progressListener = progressListener;
            Name = "TalExecutionService";
            Description = "Performs/Tracks TAL execution";
        }

        public IPsdzTal ExecuteTal(IPsdzConnection connection, IPsdzTal tal, IPsdzSvt svtTarget, IPsdzVin vin, IPsdzFa faTarget, TalExecutionSettings talExecutionConfig, string backupDataPath, CancellationToken ct)
        {
            return ExecuteTalInternal(connection, tal, svtTarget, vin, faTarget, talExecutionConfig, backupDataPath, ct, "ExecuteTal");
        }

        public IPsdzTal ExecuteTalFile(IPsdzConnection connection, string pathToTal, string vin, string pathToFa, TalExecutionSettings talExecutionSettings, CancellationToken ct)
        {
            if (connection == null)
            {
                throw new ArgumentException("connection");
            }
            PsdzHelper.CheckString("pathToTal", pathToTal);
            PsdzHelper.CheckString("vin", vin);
            string fullPath = Path.GetFullPath(pathToTal);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("Path to TAL ('" + fullPath + "') could not be found!");
            }
            string xml = File.ReadAllText(fullPath, Encoding.UTF8);
            IPsdzTal tal = _objectBuilderService.BuildTalFromXml(xml);
            IPsdzFa fa = null;
            if (!string.IsNullOrEmpty(pathToFa))
            {
                string fullPath2 = Path.GetFullPath(pathToFa);
                if (!File.Exists(fullPath2))
                {
                    throw new FileNotFoundException($"Path to FA ('{fullPath2}') could not be found!");
                }
                string xml2 = File.ReadAllText(fullPath2, Encoding.UTF8);
                fa = _objectBuilderService.BuildFaFromXml(xml2);
            }
            return ExecuteTalInternal(connection, tal, null, new PsdzVin
            {
                Value = vin
            }, fa, talExecutionSettings, null, ct, "ExecuteTalFile");
        }

        public IPsdzTal ExecuteHDDUpdate(IPsdzConnection connection, IPsdzTal tal, IPsdzFa fa, IPsdzVin vin, TalExecutionSettings configs)
        {
            IPsdzProgressListener listener = progressListener;
            string text = null;
            try
            {
                IncrementTalExecutions();
                Log.Debug(Log.CurrentMethod(), "Call 'Programming.ExecuteHddUpdate(... )' ...");
                text = _programmingService.ExecuteHDDUpdate(connection, tal, fa, vin, configs);
                Log.Debug(Log.CurrentMethod(), "ExecutionID: '" + text + "'");
                long maxSecToCompletion = 0L;
                long startTime = JavaCurrentTimeMillis();
                IPsdzTal psdzTal;
                do
                {
                    psdzTal = _programmingService.RequestHddUpdateStatus(text);
                    NotifyListenerAboutBeginTask(listener, psdzTal.TalExecutionState.ToString());
                    NotifyEventListenersAboutHddDuration(psdzTal, listener, startTime, ref maxSecToCompletion);
                    Thread.Sleep(2000);
                }
                while (!IsTalExecutionFinished(psdzTal.TalExecutionState));
                return psdzTal;
            }
            finally
            {
                if (text != null && !_ignoreTalRelease)
                {
                    _programmingService.Release(text);
                }
                DecrementTalExecutions();
                NotifyListenerAboutFinished(listener);
            }
        }

        private IPsdzTal ExecuteTalInternal(IPsdzConnection connection, IPsdzTal tal, IPsdzSvt svtTarget, IPsdzVin vin, IPsdzFa fa, TalExecutionSettings talExecutionConfig, string backupDataPath, CancellationToken ct, [CallerMemberName] string caller = null)
        {
            Log.Info(caller, "Enter.");
            IPsdzProgressListener listener = progressListener;
            IConnectionLossEventListener connectionLossEventListener = null;
            string text = null;
            try
            {
                IncrementTalExecutions();
                long startTime = JavaCurrentTimeMillis();
                Log.Debug(Log.CurrentMethod(), "Call 'Programming.executeAsync(connection, tal, svtTarget, fa, null, vin, talExecutionConfig)' ...");
                connectionLossEventListener = eventManagerService.AddPsdzEventListenerForConnectionLoss();
                text = _programmingService.ExecuteAsync(connection, tal, svtTarget, fa, null, vin, talExecutionConfig);
                Log.Debug(Log.CurrentMethod(), "ExecutionID: '" + text + "'");
                IPsdzTal psdzTal;
                do
                {
                    psdzTal = _programmingService.RequestExecutionStatus(text);
                    NotifyListenerAboutBeginTask(listener, psdzTal.TalExecutionState.ToString());
                    SetExecutionTime(listener, startTime, psdzTal.PsdzExecutionTime);
                    Thread.Sleep(500);
                }
                while (!IsTalExecutionFinished(psdzTal.TalExecutionState) && !ct.IsCancellationRequested);
                if (ct.IsCancellationRequested)
                {
                    Log.Info(Log.CurrentMethod(), "TAL execution cancellation requested.");
                    psdzTal = _programmingService.Cancel(text);
                }
                if (!string.IsNullOrEmpty(backupDataPath) && Directory.Exists(backupDataPath) && ContainsProcessedBackupTas(psdzTal))
                {
                    try
                    {
                        Log.Info(Log.CurrentMethod(), string.Format(CultureInfo.InvariantCulture, "Store backup data that are read during TAL execution to the target directory '{0}'.", backupDataPath));
                        _programmingService.RequestBackupdata(text, backupDataPath);
                    }
                    catch (Exception exception)
                    {
                        Log.WarningException(Log.CurrentMethod(), exception);
                    }
                }
                return psdzTal;
            }
            finally
            {
                if (text != null && !_ignoreTalRelease)
                {
                    _programmingService.Release(text);
                }
                DecrementTalExecutions();
                NotifyListenerAboutFinished(listener);
                eventManagerService.RemovePsdzEventListenerForConnectionLoss();
                connectionLossEventListener?.LogConnectionLossEventMessages();
                Log.Info(caller, "Exit.");
            }
        }

        private static void NotifyListenerAboutBeginTask(IPsdzProgressListener listener, string task)
        {
            try
            {
                listener.BeginTask(task);
            }
            catch (Exception exception)
            {
                Log.WarningException(Log.CurrentMethod(), exception);
            }
        }

        private static void NotifyListenerAboutFinished(IPsdzProgressListener listener)
        {
            try
            {
                listener.SetFinished();
            }
            catch (Exception exception)
            {
                Log.WarningException(Log.CurrentMethod(), exception);
            }
        }

        private void NotifyEventListenersAboutHddDuration(IPsdzTal statusTal, IPsdzProgressListener listener, long startTime, ref long maxSecToCompletion)
        {
            try
            {
                if (statusTal == null)
                {
                    Log.Info(Log.CurrentMethod(), "Status TAL is null.");
                    return;
                }
                IPsdzTalLine hddTalLine = GetHddTalLine(statusTal);
                if (hddTalLine == null)
                {
                    Log.Info(Log.CurrentMethod(), "Hdd TALLine is null.");
                    return;
                }
                PsdzHddUpdate hddUpdate = hddTalLine.HddUpdate;
                long secondsToCompletion = GetSecondsToCompletion(hddUpdate);
                if (secondsToCompletion > maxSecToCompletion)
                {
                    maxSecToCompletion = secondsToCompletion;
                    Log.Info(Log.CurrentMethod(), "New maxSecToCompletion is " + maxSecToCompletion + ".");
                }
                if (secondsToCompletion > 0)
                {
                    long num = JavaCurrentTimeMillis();
                    long end = num + 1000 * (secondsToCompletion + 60);
                    try
                    {
                        end = NeverEnd(end, num);
                        int duration = GetDuration(startTime, end);
                        listener.SetDuration(duration);
                        listener.SetElapsedTime(GetDuration(startTime, num));
                    }
                    catch (Exception exception)
                    {
                        Log.WarningException(Log.CurrentMethod(), exception);
                    }
                }
                int progress = 0;
                if (maxSecToCompletion > 0)
                {
                    progress = (int)((maxSecToCompletion - secondsToCompletion) * 100 / maxSecToCompletion);
                }
                IPsdzEvent psdzEvent = new PsdzTransactionProgressEvent
                {
                    EcuId = hddTalLine.EcuIdentifier,
                    Progress = progress,
                    Timestamp = JavaCurrentTimeMillis(),
                    TransactionInfo = PsdzTransactionInfo.ProgressInfo
                };
                eventManagerService.SendInternalEvent(psdzEvent);
            }
            catch (Exception exception2)
            {
                Log.WarningException(Log.CurrentMethod(), exception2);
            }
        }

        private IPsdzTalLine GetHddTalLine(IPsdzTal tal)
        {
            return tal.TalLines?.FirstOrDefault((IPsdzTalLine talLine) => talLine.HddUpdate?.Tas?.Any() == true);
        }

        private static bool ContainsProcessedBackupTas(IPsdzTal tal)
        {
            if (tal == null)
            {
                return false;
            }
            foreach (IPsdzTalLine talLine in tal.TalLines)
            {
                PsdzTaCategory psdzTaCategory = (PsdzTaCategory)(((object)talLine.IdBackup) ?? ((object)talLine.FscBackup));
                if (psdzTaCategory?.Tas?.Any() == true && (psdzTaCategory.ExecutionState == PsdzTaExecutionState.Finished || psdzTaCategory.ExecutionState == PsdzTaExecutionState.FinishedWithWarnings))
                {
                    return true;
                }
            }
            return false;
        }

        private long GetSecondsToCompletion(PsdzHddUpdate hddUpdate)
        {
            using (IEnumerator<PsdzHddUpdateTA> enumerator = (hddUpdate?.Tas?.Cast<PsdzHddUpdateTA>() ?? Array.Empty<PsdzHddUpdateTA>()).GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    return enumerator.Current.SecondsToCompletion;
                }
            }
            return -1L;
        }

        private static void SetExecutionTime(IPsdzProgressListener listener, long startTime, IPsdzExecutionTime execTime)
        {
            try
            {
                long num = JavaCurrentTimeMillis();
                long end = execTime.PlannedEndTime + 60000;
                end = NeverEnd(end, num);
                int duration = GetDuration(startTime, end);
                listener.SetDuration(duration);
                listener.SetElapsedTime(GetDuration(startTime, num));
            }
            catch (Exception exception)
            {
                Log.WarningException(Log.CurrentMethod(), exception);
            }
        }

        private static long NeverEnd(long end, long current)
        {
            long num;
            for (num = end; num - current < 10000; num += 10000)
            {
            }
            return num;
        }

        private static int GetDuration(long start, long end)
        {
            if ((double)end * 1.0 - (double)start * 1.0 > 2147483647.0)
            {
                return int.MaxValue;
            }
            return (int)(end - start);
        }

        public static long JavaCurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }

        private static bool IsTalExecutionFinished(PsdzTalExecutionState talExecutionStatus)
        {
            if (talExecutionStatus != PsdzTalExecutionState.Executable)
            {
                return talExecutionStatus != PsdzTalExecutionState.Running;
            }
            return false;
        }

        private void DecrementTalExecutions()
        {
            int dependencyCount = Interlocked.Decrement(ref _activeTalExecutions);
            OnActiveDependencyCountChanged(dependencyCount);
        }

        private void IncrementTalExecutions()
        {
            int dependencyCount = Interlocked.Increment(ref _activeTalExecutions);
            OnActiveDependencyCountChanged(dependencyCount);
        }

        private void OnActiveDependencyCountChanged(int dependencyCount)
        {
            this.ActiveDependencyCountChanged?.Invoke(this, new DependencyCountChangedEventArgs(dependencyCount));
        }
    }
}
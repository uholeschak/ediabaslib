using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Tal;
using BMW.Rheingold.Psdz.Model.Tal.TalFilter;
using PsdzClient.Contracts;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using RestSharp;

namespace BMW.Rheingold.Psdz
{
    internal class IndividualDataRestoreService : IIndividualDataRestoreService
    {
        private int _activeBackupTalExecutions;
        private TalModel statusTal;
        private readonly IWebCallHandler _webCallHandler;
        private readonly IPsdzProgressListener progressListener;
        private readonly string _endpoint = "idr";
        private readonly MacrosService _macrosService;
        private readonly IProgrammingService _programmingService;
        private static readonly TaExecutionStateMapper _taExecutionStateMapper = new TaExecutionStateMapper();
        private static readonly TalExecutionStateMapper _talExecutionStateMapper = new TalExecutionStateMapper();
        private bool _ignoreTalRelease = ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Psdz.IgnoreTalRelease", defaultValue: true);
        public event EventHandler<DependencyCountChangedEventArgs> ActiveDependencyCountChanged;
        public IndividualDataRestoreService(IWebCallHandler webCallHandler, IPsdzProgressListener progressListener, IMacrosService macrosService, IProgrammingService programmingService)
        {
            _webCallHandler = webCallHandler;
            this.progressListener = progressListener;
            _macrosService = (MacrosService)macrosService;
            _programmingService = programmingService;
        }

        public IPsdzTal GenerateBackupTal(IPsdzConnection connection, string backupDataPath, IPsdzTal standardTal, IPsdzTalFilter talFilter)
        {
            try
            {
                if (!Directory.Exists(backupDataPath))
                {
                    Log.Warning(Log.CurrentMethod(), "No file existing for the provided backupDataPath: " + backupDataPath + ".");
                    return null;
                }

                GenerateBackupTalRequestModel requestBodyObject = new GenerateBackupTalRequestModel
                {
                    BackupPath = backupDataPath,
                    StandardTal = TalMapper.Map(standardTal),
                    TalFilter = TalFilterMapper.Map(talFilter)
                };
                return TalMapper.Map(_webCallHandler.ExecuteRequest<TalModel>(_endpoint, $"generatebackuptal/{connection.Id}", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzTal GenerateRestorePrognosisTal(IPsdzConnection connection, string backupDataPath, IPsdzTal standardTal, IPsdzTal backupTal, IPsdzTalFilter talFilter)
        {
            try
            {
                if (!Directory.Exists(backupDataPath))
                {
                    Log.Warning(Log.CurrentMethod(), "No file existing for the provided backupDataPath: " + backupDataPath + ".");
                    return null;
                }

                GenerateRestorePrognosisTalRequestModel requestBodyObject = new GenerateRestorePrognosisTalRequestModel
                {
                    BackupPath = backupDataPath,
                    BackupTal = TalMapper.Map(backupTal),
                    StandardTal = TalMapper.Map(standardTal),
                    TalFilter = TalFilterMapper.Map(talFilter)
                };
                return TalMapper.Map(_webCallHandler.ExecuteRequest<TalModel>(_endpoint, $"generaterestoreprognosistal/{connection.Id}", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzTal GenerateRestoreTal(IPsdzConnection connection, string backupDataFilePath, IPsdzTal standardTal, IPsdzTalFilter talFilter)
        {
            try
            {
                if (!Directory.Exists(backupDataFilePath))
                {
                    Log.Warning(Log.CurrentMethod(), "No file existing for the provided backupDataPath: " + backupDataFilePath + ".");
                    return null;
                }

                GenerateRestoreTalRequestModel requestBodyObject = new GenerateRestoreTalRequestModel
                {
                    BackupPath = backupDataFilePath,
                    StandardTal = TalMapper.Map(standardTal),
                    TalFilter = TalFilterMapper.Map(talFilter)
                };
                return TalMapper.Map(_webCallHandler.ExecuteRequest<TalModel>(_endpoint, $"generaterestoretal/{connection.Id}", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzTal ExecuteBackupTal(IPsdzConnection connection, IPsdzTal tal, IPsdzSvt svtTarget, IPsdzFa faTarget, IPsdzVin vin, TalExecutionSettings talExecutionSettings, string backupDataPath)
        {
            return ExecuteTal(connection, Log.CurrentMethod(), tal, svtTarget, faTarget, vin, talExecutionSettings, backupDataPath, generateBackupFile: true);
        }

        public IPsdzTal ExecuteRestoreTal(IPsdzConnection connection, IPsdzTal tal, IPsdzSvt svtTarget, IPsdzFa faTarget, IPsdzVin vin, TalExecutionSettings talExecutionSettings)
        {
            return ExecuteTal(connection, Log.CurrentMethod(), tal, svtTarget, faTarget, vin, talExecutionSettings);
        }

        public IPsdzTal ExecuteAsyncBackupTal(IPsdzConnection connection, IPsdzTal tal, IPsdzSvt svtTarget, IPsdzFa faTarget, IPsdzVin vin, TalExecutionSettings talExecutionSettings, string backupDataPath)
        {
            return ExecuteAsyncTal(connection, Log.CurrentMethod(), tal, svtTarget, faTarget, vin, talExecutionSettings, backupDataPath, generateBackupFile: true);
        }

        public IPsdzTal ExecuteAsyncRestoreTal(IPsdzConnection connection, IPsdzTal tal, IPsdzSvt svtTarget, IPsdzFa faTarget, IPsdzVin vin, TalExecutionSettings talExecutionSettings)
        {
            return ExecuteAsyncTal(connection, Log.CurrentMethod(), tal, svtTarget, faTarget, vin, talExecutionSettings);
        }

        private IPsdzTal ExecuteAsyncTal(IPsdzConnection connection, string callingMethod, IPsdzTal tal, IPsdzSvt svtTarget, IPsdzFa faTarget, IPsdzVin vin, TalExecutionSettings talExecutionSettings, string backupDataPath = "", bool generateBackupFile = false)
        {
            string text = string.Empty;
            IPsdzProgressListener listener = progressListener;
            try
            {
                int itemCount = Interlocked.Increment(ref _activeBackupTalExecutions);
                this.ActiveDependencyCountChanged?.Invoke(this, new DependencyCountChangedEventArgs(itemCount));
                long startTime = DateTime.Now.Ticks / 10000;
                Log.Debug(callingMethod, "Call 'Programming.executeAsync(connection, tal, svtTarget, fa, null, vin, talExecutionConfig)' ...");
                text = _programmingService.ExecuteAsync(connection, tal, svtTarget, faTarget, null, vin, talExecutionSettings);
                Log.Debug(callingMethod, "ExecutionID: '" + text + "'");
                TalModel talModel;
                do
                {
                    talModel = TalMapper.Map(_programmingService.RequestExecutionStatus(text));
                    NotifyListenerAboutBeginTask(listener, talModel.TalExecutionState.ToString());
                    ExecutionTimeTypeModel executionTimeType = talModel.ExecutionTimeType;
                    SetExecutionTime(listener, startTime, executionTimeType);
                    Thread.Sleep(500);
                }
                while (!IsTalExecutionFinished(talModel.TalExecutionState));
                if (generateBackupFile)
                {
                    RequestBackupdata(text, backupDataPath, callingMethod, talModel);
                }

                Log.Info(callingMethod, "Finished to execute TAL " + NormalizeXmlText(tal.AsXml));
                return TalMapper.Map(talModel);
            }
            catch (Exception exception)
            {
                Log.ErrorException(callingMethod, exception);
                throw;
            }
            finally
            {
                if (!string.IsNullOrEmpty(text) && !_ignoreTalRelease)
                {
                    _programmingService.Release(text);
                }

                int itemCount2 = Interlocked.Decrement(ref _activeBackupTalExecutions);
                this.ActiveDependencyCountChanged?.Invoke(this, new DependencyCountChangedEventArgs(itemCount2));
                NotifyListenerAboutFinished(listener);
            }
        }

        private IPsdzTal ExecuteTal(IPsdzConnection connection, string callingMethod, IPsdzTal tal, IPsdzSvt svtTarget, IPsdzFa faTarget, IPsdzVin vin, TalExecutionSettings talExecutionSettings, string backupFilePath = "", bool generateBackupFile = false)
        {
            string text = string.Empty;
            IPsdzProgressListener listener = progressListener;
            try
            {
                Log.Info(callingMethod, "Starting to execute TAL " + NormalizeXmlText(tal.AsXml));
                int itemCount = Interlocked.Increment(ref _activeBackupTalExecutions);
                this.ActiveDependencyCountChanged?.Invoke(this, new DependencyCountChangedEventArgs(itemCount));
                long startTime = DateTime.Now.Ticks / 10000;
                Log.Debug(callingMethod, "Call 'Macros.executeTAL(connection, tal, svtTarget, fa, null, vin, talExecutionConfig)'");
                text = _macrosService.ExecuteTal(connection, tal, svtTarget, faTarget, null, vin, talExecutionSettings);
                Log.Debug(callingMethod, $"ExecutionID: '{text}'");
                do
                {
                    statusTal = TalMapper.Map(_programmingService.RequestExecutionStatus(text));
                    NotifyListenerAboutBeginTask(listener, statusTal.TalExecutionState.ToString());
                    ExecutionTimeTypeModel executionTimeType = statusTal.ExecutionTimeType;
                    SetExecutionTime(listener, startTime, executionTimeType);
                    Thread.Sleep(500);
                }
                while (!IsTalExecutionFinished(statusTal.TalExecutionState));
                if (generateBackupFile)
                {
                    RequestBackupdata(text, backupFilePath, callingMethod, statusTal);
                }

                Log.Info(callingMethod, "Finished to execute TAL " + NormalizeXmlText(tal.AsXml));
                return TalMapper.Map(statusTal);
            }
            catch (Exception exception)
            {
                Log.ErrorException(callingMethod, exception);
                throw;
            }
            finally
            {
                if (!string.IsNullOrEmpty(text) && !_ignoreTalRelease)
                {
                    _programmingService.Release(text);
                }

                int itemCount2 = Interlocked.Decrement(ref _activeBackupTalExecutions);
                this.ActiveDependencyCountChanged?.Invoke(this, new DependencyCountChangedEventArgs(itemCount2));
                NotifyListenerAboutFinished(listener);
            }
        }

        private void NotifyListenerAboutBeginTask(IPsdzProgressListener listener, string task)
        {
            try
            {
                if (listener == null)
                {
                    Log.Warning(Log.CurrentMethod(), "Progress notification cannot be sent because the listener is null.");
                }
                else
                {
                    listener.BeginTask(task);
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
            }
        }

        private void NotifyListenerAboutFinished(IPsdzProgressListener listener)
        {
            try
            {
                if (listener == null)
                {
                    Log.Warning(Log.CurrentMethod(), "Progress notification cannot be sent because the listener is null.");
                }
                else
                {
                    listener.SetFinished();
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
            }
        }

        private void SetExecutionTime(IPsdzProgressListener listener, long startTime, ExecutionTimeTypeModel execTime)
        {
            try
            {
                if (listener == null)
                {
                    Log.Warning(Log.CurrentMethod(), "Progress notification cannot be sent because the listener is null.");
                    return;
                }

                long num = DateTime.Now.Ticks / 10000;
                long end = execTime.PlannedEndTime + 60000;
                end = NeverEnd(end, num);
                int duration = GetDuration(startTime, end);
                listener.SetDuration(duration);
                listener.SetElapsedTime(GetDuration(startTime, num));
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
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

        private static bool IsTalExecutionFinished(TalExecutionStateModel talExecutionStatus)
        {
            PsdzTalExecutionState value = _talExecutionStateMapper.GetValue(talExecutionStatus);
            if (!value.Equals(PsdzTalExecutionState.Executable))
            {
                return !value.Equals(PsdzTalExecutionState.Running);
            }

            return false;
        }

        private void RequestBackupdata(string executionId, string backupDataPath, string callingMethod, TalModel statusTal)
        {
            if (!string.IsNullOrEmpty(backupDataPath) && Directory.Exists(backupDataPath) && ContainsProcessedBackupTas(statusTal))
            {
                try
                {
                    Log.Info(callingMethod, string.Format(CultureInfo.InvariantCulture, "Store backup data that are read during TAL execution to the target directory '{0}'.", backupDataPath));
                    _programmingService.RequestBackupdata(executionId, backupDataPath);
                    LogDirectoryContent(backupDataPath, callingMethod);
                }
                catch (Exception exception)
                {
                    Log.ErrorException(callingMethod, exception);
                }
            }
        }

        private static bool ContainsProcessedBackupTas(TalModel tal)
        {
            if (tal == null)
            {
                return false;
            }

            ICollection<TalLineModel> talLines = tal.TalLines;
            int i = 0;
            for (int count = talLines.Count; i < count; i++)
            {
                TalLineModel talLineModel = talLines.ElementAt(i);
                IdBackupModel idBackup = talLineModel.IdBackup;
                PsdzTaExecutionState? value = _taExecutionStateMapper.GetValue(idBackup.ExecutionStatus);
                if (value.HasValue)
                {
                    ICollection<TaModel> tas = idBackup.Tas;
                    if (tas != null && tas.Any() && (value == PsdzTaExecutionState.Finished || value == PsdzTaExecutionState.FinishedWithWarnings))
                    {
                        return true;
                    }
                }

                FscBackupModel fscBackup = talLineModel.FscBackup;
                PsdzTaExecutionState? value2 = _taExecutionStateMapper.GetValue(fscBackup.ExecutionStatus);
                if (value2.HasValue)
                {
                    ICollection<TaModel> tas2 = idBackup.Tas;
                    if (tas2 != null && tas2.Any() && (value2 == PsdzTaExecutionState.Finished || value2 == PsdzTaExecutionState.FinishedWithWarnings))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void LogDirectoryContent(string path, string method)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            Log.Info(method, string.Format(CultureInfo.InvariantCulture, "Directory " + path + " contains the following files: " + string.Join(" | ", (
                from file in directoryInfo.GetFiles()select file.Name).ToList())));
        }

        private static string NormalizeXmlText(string xmlText)
        {
            if (string.IsNullOrEmpty(xmlText))
            {
                return xmlText;
            }

            return Regex.Replace(xmlText.Trim(), ">\\s+<", "><");
        }
    }
}
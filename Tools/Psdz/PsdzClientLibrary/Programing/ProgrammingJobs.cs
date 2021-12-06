using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using PsdzClient.Programming;

namespace PsdzClient.Programing
{
    public class ProgrammingJobs : IDisposable
    {
        public delegate void UpdateStatusDelegate(string message = null);
        public delegate void ProgressDelegate(int progresPercent, bool marquee, string message = null);
        public event UpdateStatusDelegate UpdateStatusEvent;
        public event ProgressDelegate ProgressEvent;

        private bool _disposed;
        public PsdzContext PsdzContext { get; set; }
        public ProgrammingService ProgrammingService { get; set; }

        private static readonly ILog log = LogManager.GetLogger(typeof(ProgrammingJobs));

        public ProgrammingJobs()
        {
            ProgrammingService = null;
        }

        public bool StartProgrammingService(CancellationTokenSource cts, string istaFolder, string dealerId)
        {
            StringBuilder sbResult = new StringBuilder();
            try
            {
                sbResult.AppendLine("Starting programming service");
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "DealerId={0}", dealerId));
                UpdateStatus(sbResult.ToString());

                if (ProgrammingService != null && ProgrammingService.IsPsdzPsdzServiceHostInitialized())
                {
                    if (!StopProgrammingService(cts))
                    {
                        sbResult.AppendLine("Stop host failed");
                        UpdateStatus(sbResult.ToString());
                        return false;
                    }
                }

                ProgrammingService = new ProgrammingService(istaFolder, dealerId);
                SetupLog4Net();
                ProgrammingService.EventManager.ProgrammingEventRaised += (sender, args) =>
                {
                    if (args is ProgrammingTaskEventArgs programmingEventArgs)
                    {
                        if (programmingEventArgs.IsTaskFinished)
                        {
                            ProgressEvent?.Invoke(100, false);
                        }
                        else
                        {
                            int progress = (int)(programmingEventArgs.Progress * 100.0);
                            string message = string.Format(CultureInfo.InvariantCulture, "{0}%, {1}s", progress, programmingEventArgs.TimeLeftSec);
                            ProgressEvent?.Invoke(progress, false, message);
                        }
                    }
                };

                sbResult.AppendLine("Generating test module data ...");
                UpdateStatus(sbResult.ToString());
                bool result = ProgrammingService.PdszDatabase.GenerateTestModuleData(progress =>
                {
                    string message = string.Format(CultureInfo.InvariantCulture, "{0}%", progress);
                    ProgressEvent?.Invoke(progress, false, message);

                    if (cts != null)
                    {
                        return cts.Token.IsCancellationRequested;
                    }
                    return false;
                });

                ProgressEvent?.Invoke(0, true);

                if (!result)
                {
                    sbResult.AppendLine("Generating test module data failed");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                sbResult.AppendLine("Starting host ...");
                UpdateStatus(sbResult.ToString());
                if (!ProgrammingService.StartPsdzServiceHost())
                {
                    sbResult.AppendLine("Start host failed");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                ProgrammingService.SetLogLevelToMax();
                sbResult.AppendLine("Host started");
                UpdateStatus(sbResult.ToString());

                ProgrammingService.PdszDatabase.ResetXepRules();
                return true;
            }
            catch (Exception ex)
            {
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Exception: {0}", ex.Message));
                UpdateStatus(sbResult.ToString());
                return false;
            }
        }

        public bool StopProgrammingService(CancellationTokenSource cts)
        {
            StringBuilder sbResult = new StringBuilder();
            try
            {
                sbResult.AppendLine("Stopping host ...");
                UpdateStatus(sbResult.ToString());

                if (ProgrammingService != null)
                {
                    ProgrammingService.Psdz.Shutdown();
                    ProgrammingService.CloseConnectionsToPsdzHost();
                    ProgrammingService.Dispose();
                    ProgrammingService = null;
                    ClearProgrammingObjects();
                }

                sbResult.AppendLine("Host stopped");
                UpdateStatus(sbResult.ToString());
            }
            catch (Exception ex)
            {
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Exception: {0}", ex.Message));
                UpdateStatus(sbResult.ToString());
                return false;
            }

            return true;
        }

        public bool InitProgrammingObjects(string istaFolder)
        {
            try
            {
                PsdzContext = new PsdzContext(istaFolder);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public void ClearProgrammingObjects()
        {
            if (PsdzContext != null)
            {
                PsdzContext.Dispose();
                PsdzContext = null;
            }
        }

        public void UpdateStatus(string message = null)
        {
            UpdateStatusEvent?.Invoke(message);
        }

        public void SetupLog4Net()
        {
            string appDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(appDir))
            {
                string log4NetConfig = Path.Combine(appDir, "log4net.xml");
                if (File.Exists(log4NetConfig))
                {
                    string logFile = Path.Combine(ProgrammingService.GetPsdzServiceHostLogDir(), "PsdzClient.log");
                    log4net.GlobalContext.Properties["LogFileName"] = logFile;
                    XmlConfigurator.Configure(new FileInfo(log4NetConfig));
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                if (ProgrammingService != null)
                {
                    ProgrammingService.Dispose();
                    ProgrammingService = null;
                }
                ClearProgrammingObjects();
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }
    }
}

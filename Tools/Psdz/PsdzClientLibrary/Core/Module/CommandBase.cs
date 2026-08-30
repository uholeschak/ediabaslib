using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Interaction.Models;
using BMW.Rheingold.CoreFramework.InteropHelper;
using BMW.Rheingold.Psdz;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using PsdzClient;

#pragma warning disable SYSLIB0006, CS0168, CS4014
namespace BMW.Rheingold.PresentationFramework
{
    public abstract class CommandBase : ICommand
    {
        public delegate void DelegateFunction();

        private static readonly List<string> trace = new List<string>();

        protected INavigationService navigationService;

        protected Window owner;

        protected PropertyChangedEventHandler progressMonitorPropChangedEventHandler;

        private readonly string id;

        private InteractionProgressModel interactionProgressModel;

        private bool finishedExecution;

        private bool parallel;

        private ProgressMonitor progressMonitor;

        private Task doExecuteTask;

        private bool traceEnabled;

        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        [PreserveSource(Hint = "IMultisessionLogic", Placeholder = true)]
        private PlaceholderType logic;

        [PreserveSource(Hint = "IMultisessionLogic", Placeholder = true)]
        public PlaceholderType Logic
        {
            get
            {
                return logic;
            }
            set
            {
                logic = value;
            }
        }

        public Dispatcher Dispatcher { get; private set; }

        public string ID => id ?? GetType().Name;

        public bool IsParallelExecutionUsed
        {
            get
            {
                return parallel;
            }
            set
            {
                parallel = value;
            }
        }

        public string Trace
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (string item in trace)
                {
                    stringBuilder.Append(item);
                }
                return stringBuilder.ToString();
            }
        }

        protected InteractionProgressModel InteractionProgressModel
        {
            get
            {
                return interactionProgressModel;
            }
            set
            {
                interactionProgressModel = value;
            }
        }

        protected bool FinishedExecution
        {
            get
            {
                return finishedExecution;
            }
            set
            {
                finishedExecution = value;
            }
        }

        protected Task DoExecuteTask
        {
            get
            {
                return doExecuteTask;
            }
            set
            {
                doExecuteTask = value;
            }
        }

        protected bool TraceEnabled
        {
            get
            {
                return traceEnabled;
            }
            set
            {
                traceEnabled = value;
            }
        }

        protected ProgressMonitor ProgressMonitor
        {
            get
            {
                return progressMonitor;
            }
            set
            {
                progressMonitor = value;
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        protected CommandBase()
            : this(null)
        {
            //[-] Logic = ServiceLocator.Current.GetService<IAppSessionContext>()?.Logic as IMultisessionLogic;
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        protected CommandBase(string id)
        {
            if (PresentationFramework.DebugLevel > 0)
            {
                Log.Info("CommandBase.CommandBase(string)", "Command of type \"{0}\" instantiated with id \"{1}\".", GetType().Name, id);
            }
            Dispatcher = Dispatcher.CurrentDispatcher;
            this.id = id;
            VerifyAssemblyHelper.VerifyStrongName(typeof(CommandBase), force: true);
            navigationService = ServiceLocator.Current.GetService<INavigationService>();
            //[-] Logic = ServiceLocator.Current.GetService<IAppSessionContext>()?.Logic as IMultisessionLogic;
        }

        public static string BuildTrace(string cmd, string method, bool start)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("[").Append(cmd).Append("-")
                .Append(method)
                .Append("]");
            if (start)
            {
                stringBuilder.Append("->");
            }
            else
            {
                stringBuilder.Append("<-");
            }
            return stringBuilder.ToString();
        }

        public virtual void Abort(object parameter)
        {
            Log.Info("CommandBase.Abort()", "called");
            try
            {
                if (!IsAbortable(parameter))
                {
                    Log.Warning("CommandBase.Abort()", "called for non abortable command: {0}", ID);
                    return;
                }
                Log.Info("CommandBase.Abort()", "called for abortable command: {0}", ID);
                if (doExecuteTask == null || FinishedExecution)
                {
                    Log.Warning("CommandBase.Abort()", "Command thread execution wasn't started or has finished already", ID);
                }
                else
                {
                    cts.Cancel();
                    doExecuteTask = null;
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("CommandBase.Abort()", exception);
            }
        }

        public virtual bool CanExecute(object parameter)
        {
            return true;
        }

        public void ClearTrace()
        {
            trace.Clear();
        }

        public virtual void DoExecute(ProgressMonitor monitor, object parameter)
        {
        }

        public async Task ExecuteAsync(object parameter)
        {
            _ = 1;
            try
            {
                Dispatcher = Dispatcher.CurrentDispatcher;
                await CheckForAbort(parameter);
                FinishedExecution = false;
                LogStart("Pre");
                bool num = PreExecute(parameter);
                LogEnd("Pre");
                if (num)
                {
                    FinishedExecution = true;
                    return;
                }
                InteractionProgressModel = new InteractionProgressModel
                {
                    IsIndeterminate = true
                };
                long num2 = DateTime.Now.Ticks + 15000000;
                doExecuteTask = Task.Factory.StartNew(StartExecution, parameter, cts.Token);
                if (!IsParallelExecutionUsed)
                {
                    while (num2 > DateTime.Now.Ticks)
                    {
                        if (FinishedExecution)
                        {
                            DoPostExecute(parameter);
                            return;
                        }
                        Thread.Sleep(100);
                    }
                }
                if (FinishedExecution)
                {
                    DoPostExecute(parameter);
                }
                else if (InteractionProgressModel != null)
                {
                    Log.Info("CommandBase.Execute()", "Show progress dialog [{0}].", GetType());
                    if (IsParallelExecutionUsed)
                    {
                        ScheduleParallel();
                    }
                    //[-] Logic.Services.InteractionService.Register(InteractionProgressModel);
                    await doExecuteTask;
                    LogEnd("Prog");
                    DoPostExecute(parameter);
                }
            }
            catch (Exception ex)
            {
                //[-] MessageDialog.Show(ExceptionHandler.Instance.Handle(ex));
                //[-] if (ex is AppEndException)
                //[-] {
                //[-] Environment.Exit(0);
                //[-] }
            }
            finally
            {
                HandleFinallyExecutionCleanup(parameter);
                //[-] Logic?.Services.InteractionService.Deregister(InteractionProgressModel);
                FinishCommand(null);
            }
        }

        public virtual void Execute(object parameter)
        {
            ExecuteAsync(parameter);
        }

        public void ExecuteInCommand(object parameter, IProgressMonitor progressMonitor)
        {
            ProgressMonitor = progressMonitor as ProgressMonitor;
            ExecuteInCommand(parameter);
        }

        public void ExecuteInCommand(object parameter, CommandBase cmd)
        {
            ProgressMonitor = cmd.progressMonitor;
            ExecuteInCommand(parameter);
        }

        public virtual void HandleDoExecuteException(object parameter, Exception ex)
        {
        }

        public virtual bool IsAbortable(object parameter)
        {
            return false;
        }

        public void LogEnd(string method)
        {
            if (TraceEnabled)
            {
                trace.Add(BuildTrace(ID, method, start: false));
                Log.Info("CommandBase.LogEnd()", trace[trace.Count - 1]);
            }
        }

        public void LogStart(string method)
        {
            if (TraceEnabled)
            {
                trace.Add(BuildTrace(ID, method, start: true));
                Log.Info("CommandBase.LogStart()", trace[trace.Count - 1]);
            }
        }

        public void NotifyCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        public virtual void ParallelExecute()
        {
        }

        public virtual void PostExecute(object parameter)
        {
        }

        public virtual bool PreExecute(object parameter)
        {
            return false;
        }

        protected void FinishCommand(object obj)
        {
            if (doExecuteTask != null)
            {
                int millisecondsTimeout = 1000;
                if (obj != null)
                {
                    millisecondsTimeout = 100000;
                }
                doExecuteTask.Wait(millisecondsTimeout);
                doExecuteTask = null;
            }
            if (ProgressMonitor != null && progressMonitorPropChangedEventHandler != null)
            {
                ProgressMonitor.PropertyChanged -= progressMonitorPropChangedEventHandler;
            }
            if (obj != null)
            {
                if (obj is Thread thread)
                {
                    Log.ThreadStopped("CommandBase.FinishCommand()", thread);
                    return;
                }
                Log.Error("CommandBase.FinishCommand()", "FinishCommand called with parameter of wrong type {0}", obj.GetType().FullName);
            }
        }

        protected virtual void HandleFinallyExecutionCleanup(object parameter)
        {
        }

        private async Task CheckForAbort(object parameter)
        {
            if (doExecuteTask != null && !FinishedExecution)
            {
                if (!IsAbortable(parameter))
                {
                    string text = "threadManager is null";
                    Log.Info("CommandBase.CheckForAbort()", "Active threads: " + text);
                    //[-] throw new AppException(new FormatedData("#024"), new FormatedData("#025", ID, text));
                }
                if (await CanAbordParameters())
                {
                    Abort(parameter);
                }
            }
        }

        private Task<bool> CanAbordParameters()
        {
            return Task.Run(delegate
            {
                InteractionQuestionModel model = new InteractionQuestionModel(new FormatedData("#Warning").Localize(), new FormatedData("#026").Localize());
                //[-] InteractionButtonResponse obj = Logic?.Services.InteractionService.RegisterSync(model);
                //[-] return obj != null && obj.Action == InteractionButton.Yes;
                //[+] return true;
                return true;
            });
        }

        private void DoPostExecute(object parameter)
        {
            LogStart("Post");
            PostExecute(parameter);
            LogEnd("Post");
        }

        private void ExecuteInCommand(object parameter)
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
            bool finished = false;
            Exception error = null;
            Dispatcher.Invoke((DelegateFunction)async delegate
            {
                try
                {
                    await CheckForAbort(parameter);
                    LogStart("Pre");
                    finished = PreExecute(parameter);
                    LogEnd("Pre");
                }
                catch (Exception ex)
                {
                    error = ex;
                    finished = true;
                }
                finally
                {
                    HandleFinallyExecutionCleanup(parameter);
                    if (finished)
                    {
                        FinishedExecution = true;
                    }
                }
            });
            if (!finished)
            {
                throw new Exception("PreExecute() must return with true.");
            }
            if (error != null)
            {
                throw error;
            }
        }

        private void FinishCommandInSeparateThread()
        {
            Thread thread = new Thread(FinishCommand);
            thread.Name = ID + "FinishCommand";
            thread.Priority = ThreadPriority.Normal;
            thread.Start(thread);
            Log.ThreadStarted("CommandBase.FinishCommandInSeparateThread()", thread);
        }

        private void ScheduleParallel()
        {
            Thread thread = new Thread(StartParallelExecution);
            thread.Name = ID + "Para";
            thread.Priority = ThreadPriority.Normal;
            thread.Start();
            Log.ThreadStarted("CommandBase.ScheduleParallel()", thread);
        }

        private void StartExecution(object parameter)
        {
            try
            {
                cts.Token.Register(Thread.CurrentThread.Abort);
                LogStart("Do");
                DoExecute(ProgressMonitor, parameter);
                LogEnd("Do");
                ProgressMonitor obj = ProgressMonitor;
                if (obj != null && obj.IsRunningInBackground)
                {
                    Dispatcher.Invoke((DelegateFunction)delegate
                    {
                        PostExecute(parameter);
                    });
                    FinishCommandInSeparateThread();
                }
            }
            catch (Exception ex)
            {
                Exception ex2 = ex;
                Exception ex3 = ex2;
                //[-] Logic.Services.InteractionService.Register(new InteractionMessageModel(ExceptionHandler.Instance.Handle(ex3).Text));
                //[-] if (ex3 is AppEndException)
                //[-] {
                //[-] Environment.Exit(0);
                //[-] }
                Dispatcher.Invoke((DelegateFunction)delegate
                {
                    try
                    {
                        HandleDoExecuteException(parameter, ex3);
                    }
                    catch (Exception ex4)
                    {
                        //[-] Logic.Services.InteractionService.Register(new InteractionMessageModel(ExceptionHandler.Instance.Handle(ex4).Text));
                    }
                });
            }
            finally
            {
                FinishedExecution = true;
            }
        }

        private void StartParallelExecution()
        {
            Dispatcher.Invoke((DelegateFunction)delegate
            {
                LogStart("Para");
                ParallelExecute();
                LogEnd("Para");
            });
        }
    }
}

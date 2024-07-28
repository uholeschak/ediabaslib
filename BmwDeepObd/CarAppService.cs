using Android.Content;
using Android.OS;
using AndroidX.Car.App;
using AndroidX.Car.App.Constraints;
using AndroidX.Car.App.Model;
using AndroidX.Car.App.Validation;
using AndroidX.Lifecycle;
using EdiabasLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

namespace BmwDeepObd
{
    [Android.App.Service(
        Label = "@string/app_name",
        Icon = "@drawable/icon",
        Exported = true,
        Name = ActivityCommon.AppNameSpace + "." + nameof(CarService)
    )]
    [Android.App.IntentFilter(new[]
        {
            "androidx.car.app.CarAppService"
        },
        Categories = new[]
        {
            "androidx.car.app.category.IOT"
        })
    ]

    // Testing with Desktop Head Unit (DHU)
    // https://developer.android.com/training/cars/testing/dhu
    // Start Android Auto Emulation Server on the smartphone
    // Select once: Connect vehicle
    // adb forward tcp:5277 tcp:5277
    // cd C:\Users\Ulrich\AppData\Local\Android\android-sdk\extras\google\auto
    // desktop-head-unit.exe

    public class CarService : CarAppService
    {
        public class ErrorMessageEntry
        {
            public ErrorMessageEntry(EdiabasThread.EdiabasErrorReport errorReport, string message)
            {
                ErrorReport = errorReport;
                Message = message;
            }

            public EdiabasThread.EdiabasErrorReport ErrorReport { get; }
            public string Message { get; }
        }

        public delegate void ErrorMessageResultDelegate(List<ErrorMessageEntry> errorList);

#if DEBUG
        private static readonly string Tag = typeof(CarService).FullName;
#endif
        public const int UpdateInterval = 1000;
        private ActivityCommon _activityCommon;
        private Thread _errorEvalThread;

        public ActivityCommon ActivityCommon => _activityCommon;

        public override void OnCreate()
        {
            base.OnCreate();

            _activityCommon = new ActivityCommon(this);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (IsErrorEvalJobRunning())
            {
                _errorEvalThread.Join();
            }

            if (_activityCommon != null)
            {
                _activityCommon.Dispose();
                _activityCommon = null;
            }
        }

        public override HostValidator CreateHostValidator()
        {
            return HostValidator.AllowAllHostsValidator;
        }

        public override Session OnCreateSession()
        {
            return new CarSession(this);
        }

        public bool IsErrorEvalJobRunning()
        {
            if (_errorEvalThread == null)
            {
                return false;
            }
            if (_errorEvalThread.IsAlive)
            {
                return true;
            }
            _errorEvalThread = null;
            return false;
        }

        public bool EvaluateErrorMessages(JobReader.PageInfo pageInfo, List<EdiabasThread.EdiabasErrorReport> errorReportList, MethodInfo formatErrorResult, ErrorMessageResultDelegate resultHandler)
        {
            if (IsErrorEvalJobRunning())
            {
#if DEBUG
                Android.Util.Log.Info(Tag, "EvaluateErrorMessages: Thread still active");
#endif
                return false;
            }

            _errorEvalThread = new Thread(() =>
            {
#if DEBUG
                Android.Util.Log.Info(Tag, "EvaluateErrorMessages: Thread started");
#endif
                List<ErrorMessageEntry> errorList = new List<ErrorMessageEntry>();
                List<ActivityCommon.VagDtcEntry> dtcList = null;
                int errorIndex = 0;
                foreach (EdiabasThread.EdiabasErrorReport errorReport in errorReportList)
                {
                    if (errorReport is EdiabasThread.EdiabasErrorReportReset)
                    {
                        continue;
                    }

                    string message = GenerateErrorMessage(pageInfo, errorReport, errorIndex, formatErrorResult, ref dtcList);
                    errorList.Add(new ErrorMessageEntry(errorReport, message));
                    errorIndex++;
                }

                if (resultHandler != null)
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, string.Format("EvaluateErrorMessages: Thread finished items: {0}", errorList.Count));
#endif
                    resultHandler.Invoke(errorList);
                }
            });

            _errorEvalThread.Start();

            return true;
        }

        private string GenerateErrorMessage(JobReader.PageInfo pageInfo, EdiabasThread.EdiabasErrorReport errorReport, int errorIndex, MethodInfo formatErrorResult, ref List<ActivityCommon.VagDtcEntry> dtcList)
        {

            EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
            if (ediabasThread == null)
            {
                return string.Empty;
            }

            List<string> translationList = new List<string>();
            return ediabasThread.GenerateErrorMessage(this, _activityCommon, pageInfo, errorReport, errorIndex, formatErrorResult, ref translationList,
                null, ref dtcList);
        }


        public class CarSession(CarService carService) : Session
        {
            public override Screen OnCreateScreen(Intent intent)
            {
                return new MainScreen(CarContext, carService);
            }

            public static int GetContentLimit(CarContext carContext, int contentLimitType)
            {
                try
                {
                    if (carContext.GetCarService(Java.Lang.Class.FromType(typeof(ConstraintManager))) is ConstraintManager constraintManager)
                    {
                        return constraintManager.GetContentLimit(ConstraintManager.ContentLimitTypeList);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                return 0;
            }
        }

        public class BaseScreen : Screen, ILifecycleEventObserver
        {
            private CarService _carServiceInst;
            private readonly Handler _updateHandler;
            private readonly UpdateScreenRunnable _updateScreenRunnable;

            public CarService CarServiceInst => _carServiceInst;

            public BaseScreen(CarContext carContext, CarService carService) : base(carContext)
            {
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("BaseScreen: Class={0}", GetType().FullName));
#endif
                _carServiceInst = carService;
                _updateHandler = new Handler(Looper.MainLooper);
                _updateScreenRunnable = new UpdateScreenRunnable(this);
                Lifecycle.AddObserver(this);
            }

            public void OnStateChanged(ILifecycleOwner source, Lifecycle.Event e)
            {
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("BaseScreen: OnStateChanged Class={0}, State={1}", GetType().FullName, e));
#endif
                if (e == Lifecycle.Event.OnStart)
                {
                    RequestUpdate();
                }
                else if (e == Lifecycle.Event.OnStop)
                {
                    RequestUpdate(true);
                }
            }

            public override ITemplate OnGetTemplate()
            {
                return null!;
            }

            public virtual void RequestUpdate(bool stop = false)
            {
                Lifecycle.State currentState = Lifecycle.CurrentState;
                bool isValid = currentState == Lifecycle.State.Started || currentState == Lifecycle.State.Resumed;
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("RequestUpdate: State={0}, Valid={1}, Stop={2}, Class={3}", currentState, isValid, stop, GetType().FullName));
#endif
                _updateHandler.RemoveCallbacks(_updateScreenRunnable);
                if (isValid && !stop)
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, string.Format("RequestUpdate: PostDelayed Class={0}", GetType().FullName));
#endif
                    _updateHandler.PostDelayed(_updateScreenRunnable, UpdateInterval);
                }
                else
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, string.Format("RequestUpdate: Stopped Class={0}", GetType().FullName));
#endif
                }
            }

            public virtual bool ContentChanged()
            {
                return false;
            }
        }

        public class MainScreen(CarContext carContext, CarService carService) : BaseScreen(carContext, carService)
        {
            private string _lastContent = string.Empty;

            public override ITemplate OnGetTemplate()
            {
#if DEBUG
                Android.Util.Log.Info(Tag, "MainScreen: OnGetTemplate");
#endif
                int listLimit = CarSession.GetContentLimit(CarContext, ConstraintManager.ContentLimitTypeList);

                ItemList.Builder itemBuilder = new ItemList.Builder();
                EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
                JobReader.PageInfo pageInfoActive = ediabasThread?.JobPageInfo;
                if (!ActivityCommon.CommActive || pageInfoActive == null)
                {
                    itemBuilder.AddItem(new Row.Builder()
                        .SetTitle(CarContext.GetString(Resource.String.car_service_disconnected))
                        .AddText(CarContext.GetString(Resource.String.car_service_disconnected_hint))
                        .SetOnClickListener(new ActionListener((page) =>
                        {
                            if (ShowApp())
                            {
                                CarToast.MakeText(CarContext, Resource.String.car_service_app_displayed, CarToast.LengthLong).Show();
                            }
                        }))
                        .Build());
                }
                else
                {
                    int pageIndex = 0;
                    foreach (JobReader.PageInfo pageInfo in ActivityCommon.JobReader.PageList)
                    {
                        if (pageIndex >= listLimit)
                        {
                            break;
                        }

                        string pageName = ActivityMain.GetPageString(pageInfo, pageInfo.Name);
                        Row.Builder row = new Row.Builder()
                            .SetTitle(pageName)
                            .SetBrowsable(true)
                            .SetOnClickListener(new ActionListener((page) =>
                            {
                                if (!(page is int index))
                                {
                                    return;
                                }

                                if (index < 0 || index >= ActivityCommon.JobReader.PageList.Count)
                                {
                                    return;
                                }

                                JobReader.PageInfo newPageInfo = ActivityCommon.JobReader.PageList[index];
                                EdiabasThread ediabasThreadLocal = ActivityCommon.EdiabasThread;
                                if (ediabasThreadLocal != null)
                                {
                                    ediabasThreadLocal.JobPageInfo = newPageInfo;
                                }

                                try
                                {
                                    ScreenManager.Push(new PageScreen(CarContext, CarServiceInst));
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }, pageIndex));

                        bool activePage = pageInfo == pageInfoActive;
                        if (activePage)
                        {
                            row.AddText(CarContext.GetString(Resource.String.car_service_active_page));
                        }

                        itemBuilder.AddItem(row.Build());
                        pageIndex++;
                    }
                }

                ListTemplate listTemplate = new ListTemplate.Builder()
                    .SetHeaderAction(AndroidX.Car.App.Model.Action.AppIcon)
                    .SetTitle(CarContext.GetString(Resource.String.app_name))
                    .SetSingleList(itemBuilder.Build())
                    .Build();

                _lastContent = GetContentString(out _);
                RequestUpdate();

                return listTemplate;
            }

            public override bool ContentChanged()
            {
                string newContent = GetContentString(out _);
                if (string.Compare(_lastContent, newContent, System.StringComparison.Ordinal) == 0)
                {
                    return false;
                }

                return true;
            }

            private string GetContentString(out bool disconnected)
            {
                disconnected = false;
                try
                {
                    StringBuilder sbContent = new StringBuilder();
                    EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
                    JobReader.PageInfo pageInfoActive = ediabasThread?.JobPageInfo;
                    if (!ActivityCommon.CommActive || pageInfoActive == null)
                    {
                        disconnected = true;
                        sbContent.AppendLine(CarContext.GetString(Resource.String.car_service_disconnected));
                    }
                    else
                    {
                        foreach (JobReader.PageInfo pageInfo in ActivityCommon.JobReader.PageList)
                        {
                            string pageName = ActivityMain.GetPageString(pageInfo, pageInfo.Name);
                            sbContent.AppendLine(pageName);
                            bool activePage = pageInfo == pageInfoActive;
                            if (activePage)
                            {
                                sbContent.AppendLine(CarContext.GetString(Resource.String.car_service_active_page));
                            }
                        }
                    }

                    return sbContent.ToString();
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }

            public bool ShowApp()
            {
                try
                {
                    string packageName = CarContext.PackageName;
                    if (string.IsNullOrEmpty(packageName))
                    {
                        return false;
                    }

                    Intent intent = CarContext.PackageManager?.GetLaunchIntentForPackage(packageName);
                    if (intent == null)
                    {
                        return false;
                    }

                    intent.AddCategory(Intent.CategoryLauncher);
                    intent.SetFlags(ActivityFlags.NewTask);
                    CarContext.StartActivity(intent);
                    return true;
                }
                catch (Exception)
                {
                    // ignored
                }
                return false;
            }

        }

        public class PageScreen(CarContext carContext, CarService carService) : BaseScreen(carContext, carService)
        {
            private string _lastContent = string.Empty;
            private object _errorLockObject = new object();
            private string _errorState = string.Empty;
            private List<ErrorMessageEntry> _errorList;

            public override ITemplate OnGetTemplate()
            {
#if DEBUG
                Android.Util.Log.Info(Tag, "PageScreen: OnGetTemplate");
#endif

                int listLimit = CarSession.GetContentLimit(CarContext, ConstraintManager.ContentLimitTypeList);

                ItemList.Builder itemBuilder = new ItemList.Builder();
                EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
                JobReader.PageInfo pageInfoActive = ediabasThread?.JobPageInfo;
                string pageTitle = CarContext.GetString(Resource.String.app_name);

                if (!ActivityCommon.CommActive || pageInfoActive == null)
                {
                    itemBuilder.AddItem(new Row.Builder()
                        .SetTitle(CarContext.GetString(Resource.String.car_service_disconnected))
                        .Build());
                }
                else
                {
                    pageTitle = ActivityMain.GetPageString(pageInfoActive, pageInfoActive.Name);

                    if (pageInfoActive.ErrorsInfo != null)
                    {
                        List<ErrorMessageEntry> _errorListCopy;
                        string _errorStateCopy;
                        lock (_errorLockObject)
                        {
                            _errorStateCopy = _errorState;
                            _errorListCopy = _errorList;
                        }

                        int lineIndex = 0;
                        if (_errorListCopy != null)
                        {
                            foreach (ErrorMessageEntry errorEntry in _errorListCopy)
                            {
                                if (lineIndex >= listLimit)
                                {
                                    break;
                                }

                                string message = errorEntry.Message;
                                if (!string.IsNullOrEmpty(message))
                                {
                                    string[] messageLines = message.Split(new[] { '\r', '\n' });

                                    string title = string.Empty;
                                    StringBuilder sbText = new StringBuilder();
                                    int lineCount = 0;

                                    foreach (string messageLine in messageLines)
                                    {
                                        if (string.IsNullOrEmpty(messageLine))
                                        {
                                            continue;
                                        }

                                        if (lineCount == 0)
                                        {
                                            title = messageLine;
                                        }
                                        else
                                        {
                                            if (sbText.Length > 0)
                                            {
                                                sbText.AppendLine();
                                            }

                                            sbText.Append(messageLine);
                                        }
                                        lineCount++;
                                    }

                                    if (!string.IsNullOrEmpty(title) && sbText.Length > 0)
                                    {
                                        Row.Builder row = new Row.Builder()
                                            .SetTitle(title)
                                            .AddText(sbText.ToString());

                                        if (CarContext.CarAppApiLevel >= 2)
                                        {
                                            row.SetOnClickListener(new ActionListener((page) =>
                                            {
                                                try
                                                {
                                                    ScreenManager.Push(new PageDetailScreen(CarContext, CarServiceInst, title, sbText.ToString()));
                                                }
                                                catch (Exception)
                                                {
                                                    // ignored
                                                }
                                            }));
                                        }

                                        itemBuilder.AddItem(row.Build());
                                        lineIndex++;
                                    }
                                }
                            }
                        }
                        else if (!string.IsNullOrEmpty(_errorStateCopy))
                        {
                            Row.Builder row = new Row.Builder()
                                .SetTitle(_errorStateCopy);
                            itemBuilder.AddItem(row.Build());
                            lineIndex++;
                        }

                        if (lineIndex == 0)
                        {
                            Row.Builder row = new Row.Builder()
                                .SetTitle(CarContext.GetString(Resource.String.error_no_error));

                            itemBuilder.AddItem(row.Build());
                        }
                    }
                    else
                    {
                        MultiMap<string, EdiabasNet.ResultData> resultDict = null;
                        lock (EdiabasThread.DataLock)
                        {
                            if (ActivityCommon.EdiabasThread.ResultPageInfo == pageInfoActive)
                            {
                                resultDict = ActivityCommon.EdiabasThread.EdiabasResultDict;
                            }
                        }

                        int lineIndex = 0;
                        foreach (JobReader.DisplayInfo displayInfo in pageInfoActive.DisplayList)
                        {
                            if (lineIndex >= listLimit)
                            {
                                break;
                            }

                            string rowTitle = ActivityMain.GetPageString(pageInfoActive, displayInfo.Name);
                            Row.Builder row = new Row.Builder()
                                .SetTitle(rowTitle);

                            if (ediabasThread != null && resultDict != null)
                            {
                                string result = ediabasThread.FormatResult(pageInfoActive, displayInfo, resultDict);
                                if (!string.IsNullOrEmpty(result))
                                {
                                    row.AddText(result);

                                    if (CarContext.CarAppApiLevel >= 2)
                                    {
                                        row.SetOnClickListener(new ActionListener((page) =>
                                        {
                                            try
                                            {
                                                ScreenManager.Push(new PageDetailScreen(CarContext, CarServiceInst, rowTitle, result));
                                            }
                                            catch (Exception)
                                            {
                                                // ignored
                                            }
                                        }));
                                    }
                                }
                            }

                            itemBuilder.AddItem(row.Build());
                            lineIndex++;
                        }
                    }
                }

                ListTemplate listTemplate = new ListTemplate.Builder()
                    .SetHeaderAction(AndroidX.Car.App.Model.Action.Back)
                    .SetTitle(pageTitle)
                    .SetSingleList(itemBuilder.Build())
                    .Build();

                _lastContent = GetContentString(out _);
                RequestUpdate();

                return listTemplate;
            }

            public override bool ContentChanged()
            {
                string newContent = GetContentString(out bool disconnected);
                if (disconnected)
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, "PageScreen: ContentChanged disconnected");
#endif
                    try
                    {
                        ScreenManager.PopToRoot();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    return false;
                }

                if (string.Compare(_lastContent, newContent, StringComparison.Ordinal) == 0)
                {
                    return false;
                }

                return true;
            }

            private string GetContentString(out bool disconnected)
            {
                disconnected = false;
                try
                {
                    StringBuilder sbContent = new StringBuilder();
                    EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
                    JobReader.PageInfo pageInfoActive = ediabasThread?.JobPageInfo;
                    string pageTitle = CarContext.GetString(Resource.String.app_name);

                    if (!ActivityCommon.CommActive || pageInfoActive == null)
                    {
                        disconnected = true;
                        sbContent.AppendLine(CarContext.GetString(Resource.String.car_service_disconnected));
                    }
                    else
                    {
                        pageTitle = ActivityMain.GetPageString(pageInfoActive, pageInfoActive.Name);

                        if (pageInfoActive.ErrorsInfo != null)
                        {
                            List<EdiabasThread.EdiabasErrorReport> errorReportList = null;
                            EdiabasThread.UpdateState updateState;
                            int updateProgress;

                            lock (EdiabasThread.DataLock)
                            {
                                if (ActivityCommon.EdiabasThread.ResultPageInfo == pageInfoActive)
                                {
                                    errorReportList = ActivityCommon.EdiabasThread.EdiabasErrorReportList;
                                }

                                updateState = ActivityCommon.EdiabasThread.UpdateProgressState;
                                updateProgress = ActivityCommon.EdiabasThread.UpdateProgress;
                            }

                            if (errorReportList == null)
                            {
                                string state = string.Empty;
                                switch (updateState)
                                {
                                    case EdiabasThread.UpdateState.Init:
                                        state = CarContext.GetString(Resource.String.error_reading_state_init);
                                        break;

                                    case EdiabasThread.UpdateState.Error:
                                        state = CarContext.GetString(Resource.String.error_reading_state_error);
                                        break;

                                    case EdiabasThread.UpdateState.DetectVehicle:
                                        state = string.Format(CarContext.GetString(Resource.String.error_reading_state_detect), updateProgress);
                                        break;

                                    case EdiabasThread.UpdateState.ReadErrors:
                                        state = string.Format(CarContext.GetString(Resource.String.error_reading_state_read), updateProgress);
                                        break;
                                }

                                lock (_errorLockObject)
                                {
                                    _errorState = state;
                                    _errorList = null;
                                }
                            }
                            else
                            {
                                CarServiceInst.EvaluateErrorMessages(pageInfoActive, errorReportList, null,
                                    list =>
                                    {
                                        lock (_errorLockObject)
                                        {
                                            _errorState = string.Empty;
                                            _errorList = list;
                                        }
                                    });
                            }

                            List<ErrorMessageEntry> _errorListCopy;
                            string _errorStateCopy;
                            lock (_errorLockObject)
                            {
                                _errorStateCopy = _errorState;
                                _errorListCopy = _errorList;
                            }

                            int lineIndex = 0;
                            if (_errorListCopy != null)
                            {
                                foreach (ErrorMessageEntry errorEntry in _errorListCopy)
                                {
                                    string message = errorEntry.Message;
                                    if (!string.IsNullOrEmpty(message))
                                    {
                                        sbContent.AppendLine(message);
                                        lineIndex++;
                                    }
                                }
                            }
                            else if (!string.IsNullOrEmpty(_errorStateCopy))
                            {
                                sbContent.AppendLine(_errorStateCopy);
                                lineIndex++;
                            }

                            if (lineIndex == 0)
                            {
                                sbContent.AppendLine(CarContext.GetString(Resource.String.error_no_error));
                            }
                        }
                        else
                        {
                            MultiMap<string, EdiabasNet.ResultData> resultDict = null;
                            lock (EdiabasThread.DataLock)
                            {
                                if (ActivityCommon.EdiabasThread.ResultPageInfo == pageInfoActive)
                                {
                                    resultDict = ActivityCommon.EdiabasThread.EdiabasResultDict;
                                }
                            }

                            foreach (JobReader.DisplayInfo displayInfo in pageInfoActive.DisplayList)
                            {
                                string rowTitle = ActivityMain.GetPageString(pageInfoActive, displayInfo.Name);
                                sbContent.AppendLine(rowTitle);
                                if (ediabasThread != null && resultDict != null)
                                {
                                    string result = ediabasThread.FormatResult(pageInfoActive, displayInfo, resultDict);
                                    if (!string.IsNullOrEmpty(result))
                                    {
                                        sbContent.AppendLine(result);
                                    }
                                }
                            }
                        }
                    }

                    sbContent.AppendLine(pageTitle);
                    return sbContent.ToString();
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
        }

        public class PageDetailScreen(CarContext carContext, CarService carService, string title, string message) : BaseScreen(carContext, carService)
        {
            public override ITemplate OnGetTemplate()
            {
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("PageDetailScreen: OnGetTemplate, Title='{0}', Message='{1}'", title, message));
#endif
                LongMessageTemplate messageTemplate = new LongMessageTemplate.Builder(message)
                    .SetHeaderAction(AndroidX.Car.App.Model.Action.Back)
                    .SetTitle(title)
                    .Build();

                return messageTemplate;
            }
        }

        public class ActionListener(ActionListener.ClickDelegate handler, object parameter = null) : Java.Lang.Object, IOnClickListener
        {
            public delegate void ClickDelegate(object parameter);

            public void OnClick()
            {
                handler?.Invoke(parameter);
            }
        }

        public class UpdateScreenRunnable(BaseScreen screen) : Java.Lang.Object, Java.Lang.IRunnable
        {
            public void Run()
            {
                try
                {
                    if (screen != null)
                    {
                        bool invalidate = screen.ContentChanged();
#if DEBUG
                        Android.Util.Log.Info(Tag, string.Format("UpdateScreenRunnable: Invalidate={0}, Class={1}", invalidate, screen.GetType().FullName));
#endif
                        if (invalidate)
                        {
                            screen.Invalidate();
                        }

                        screen.RequestUpdate();
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
    }
}

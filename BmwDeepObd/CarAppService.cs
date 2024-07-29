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
            private readonly object _lockObject = new object();
            private bool _disconnected = true;
            private List<PageInfoEntry> _pageList;

            private class PageInfoEntry
            {
                public PageInfoEntry(string name, bool activePage)
                {
                    Name = name;
                    ActivePage = activePage;
                }

                public string Name { get; }

                public bool ActivePage { get; }
            }

            public override ITemplate OnGetTemplate()
            {
#if DEBUG
                Android.Util.Log.Info(Tag, "MainScreen: OnGetTemplate");
#endif
                if (string.IsNullOrEmpty(_lastContent))
                {
                    _lastContent = GetContentString();
                }

                bool disconnectedCopy;
                List<PageInfoEntry> pageListCopy;

                lock (_lockObject)
                {
                    disconnectedCopy = _disconnected;
                    pageListCopy = _pageList;
                }

                int listLimit = CarSession.GetContentLimit(CarContext, ConstraintManager.ContentLimitTypeList);
                ItemList.Builder itemBuilder = new ItemList.Builder();

                if (disconnectedCopy)
                {
                    itemBuilder.AddItem(new Row.Builder()
                        .SetTitle(CarContext.GetString(Resource.String.car_service_disconnected))
                        .AddText(CarContext.GetString(Resource.String.car_service_disconnected_hint))
                        .SetOnClickListener(ParkedOnlyOnClickListener.Create(new ActionListener((page) =>
                        {
                            _lastContent = string.Empty;
                            if (ShowApp())
                            {
                                CarToast.MakeText(CarContext, Resource.String.car_service_app_displayed, CarToast.LengthLong).Show();
                            }
                        })))
                        .Build());
                }
                else
                {
                    int pageIndex = 0;
                    if (pageListCopy != null)
                    {
                        foreach (PageInfoEntry pageInfo in pageListCopy)
                        {
                            if (pageIndex >= listLimit)
                            {
                                break;
                            }

                            string pageName = pageInfo.Name;
                            bool activePage = pageInfo.ActivePage;

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
                                        _lastContent = string.Empty;
                                        ScreenManager.Push(new PageScreen(CarContext, CarServiceInst));
                                    }
                                    catch (Exception)
                                    {
                                        // ignored
                                    }
                                }, pageIndex));

                            if (activePage)
                            {
                                row.AddText(CarContext.GetString(Resource.String.car_service_active_page));
                            }

                            itemBuilder.AddItem(row.Build());
                            pageIndex++;
                        }

                        if (pageIndex == 0)
                        {
                            itemBuilder.AddItem(new Row.Builder()
                                .SetTitle(CarContext.GetString(Resource.String.car_service_no_pages))
                                .Build());
                        }
                    }
                }

                ListTemplate listTemplate = new ListTemplate.Builder()
                    .SetHeaderAction(AndroidX.Car.App.Model.Action.AppIcon)
                    .SetTitle(CarContext.GetString(Resource.String.app_name))
                    .SetSingleList(itemBuilder.Build())
                    .Build();

                RequestUpdate();

                return listTemplate;
            }

            public override bool ContentChanged()
            {
                string newContent = GetContentString();
                if (string.Compare(_lastContent, newContent, System.StringComparison.Ordinal) == 0)
                {
                    return false;
                }

                return true;
            }

            private string GetContentString()
            {
                try
                {
                    StringBuilder sbContent = new StringBuilder();
                    EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
                    JobReader.PageInfo pageInfoActive = ediabasThread?.JobPageInfo;
                    bool disconnected = false;
                    List<PageInfoEntry> pageList = null;

                    if (!ActivityCommon.CommActive || pageInfoActive == null)
                    {
                        disconnected = true;
                        sbContent.AppendLine(CarContext.GetString(Resource.String.car_service_disconnected));
                    }
                    else
                    {
                        pageList = new List<PageInfoEntry>();
                        int pageIndex = 0;
                        foreach (JobReader.PageInfo pageInfo in ActivityCommon.JobReader.PageList)
                        {
                            string pageName = ActivityMain.GetPageString(pageInfo, pageInfo.Name);
                            sbContent.AppendLine(pageName);
                            bool activePage = pageInfo == pageInfoActive;
                            if (activePage)
                            {
                                sbContent.AppendLine(CarContext.GetString(Resource.String.car_service_active_page));
                            }

                            pageList.Add(new PageInfoEntry(pageName, activePage));
                            pageIndex++;
                        }

                        if (pageIndex == 0)
                        {
                            sbContent.AppendLine(CarContext.GetString(Resource.String.car_service_no_pages));
                        }
                    }

                    lock (_lockObject)
                    {
                        _disconnected = disconnected;
                        _pageList = pageList;
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
            private readonly object _lockObject = new object();
            private bool _disconnected = true;
            private string _pageTitle = string.Empty;
            private bool _errorPage = false;
            private string _errorState = string.Empty;
            private List<ErrorMessageEntry> _errorList;
            private List<DataInfoEntry> _dataList;

            private class DataInfoEntry
            {
                public DataInfoEntry(string title, string result)
                {
                    Title = title;
                    Result = result;
                }

                public string Title { get; }
                public string Result { get; }
            }

            public override ITemplate OnGetTemplate()
            {
#if DEBUG
                Android.Util.Log.Info(Tag, "PageScreen: OnGetTemplate");
#endif

                if (string.IsNullOrEmpty(_lastContent))
                {
                    _lastContent = GetContentString();
                }

                int listLimit = CarSession.GetContentLimit(CarContext, ConstraintManager.ContentLimitTypeList);
                ItemList.Builder itemBuilder = new ItemList.Builder();
                string pageTitle = CarContext.GetString(Resource.String.app_name);

                bool disconnectedCopy;
                string pageTitleCopy;
                bool errorPageCopy;
                string errorStateCopy;
                List<ErrorMessageEntry> errorListCopy;
                List<DataInfoEntry> dataListCopy;

                lock (_lockObject)
                {
                    disconnectedCopy = _disconnected;
                    pageTitleCopy = _pageTitle;
                    errorPageCopy = _errorPage;
                    errorStateCopy = _errorState;
                    errorListCopy = _errorList;
                    dataListCopy = _dataList;
                }

                if (!string.IsNullOrEmpty(pageTitleCopy))
                {
                    pageTitle = pageTitleCopy;
                }

                if (disconnectedCopy)
                {
                    itemBuilder.AddItem(new Row.Builder()
                        .SetTitle(CarContext.GetString(Resource.String.car_service_disconnected))
                        .Build());
                }
                else
                {
                    if (errorPageCopy)
                    {
                        int lineIndex = 0;
                        if (errorListCopy != null)
                        {
                            foreach (ErrorMessageEntry errorEntry in errorListCopy)
                            {
                                if (lineIndex >= listLimit)
                                {
                                    break;
                                }

                                string message = errorEntry.Message;
                                string ecuName = errorEntry.ErrorReport.EcuName;
                                bool validResponse = !string.IsNullOrEmpty(ecuName) && errorEntry.ErrorReport.ErrorDict != null;
                                bool shadow = errorEntry.ErrorReport is EdiabasThread.EdiabasErrorShadowReport;
                                if (!string.IsNullOrEmpty(message))
                                {
                                    string[] messageLines = message.Split(new[] { '\r', '\n' });

                                    string rowTitle = string.Empty;
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
                                            rowTitle = messageLine;
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

                                    if (!string.IsNullOrEmpty(rowTitle) && sbText.Length > 0)
                                    {
                                        Row.Builder row = new Row.Builder()
                                            .SetTitle(rowTitle)
                                            .AddText(sbText.ToString())
                                            .SetOnClickListener(ParkedOnlyOnClickListener.Create(new ActionListener((page) =>
                                            {
                                                try
                                                {
                                                    _lastContent = string.Empty;
                                                    string actionText = validResponse && !shadow ? CarContext.GetString(Resource.String.button_error_reset) : null;
                                                    ScreenManager.Push(new PageDetailScreen(CarContext, CarServiceInst, rowTitle, sbText.ToString(),
                                                        actionText, () =>
                                                        {
                                                            if (ActivityCommon.ErrorResetActive)
                                                            {
                                                                CarToast.MakeText(CarContext, Resource.String.car_service_error_reset_active, CarToast.LengthLong).Show();
                                                                return;
                                                            }

                                                            List<string> errorResetList = new List<string>() { ecuName };
                                                            EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
                                                            if (ediabasThread != null)
                                                            {
                                                                lock (EdiabasThread.DataLock)
                                                                {
                                                                    ediabasThread.ErrorResetList = errorResetList;
                                                                }

                                                                CarToast.MakeText(CarContext, Resource.String.car_service_error_reset_started, CarToast.LengthLong).Show();
                                                            }
                                                        }));
                                                }
                                                catch (Exception)
                                                {
                                                    // ignored
                                                }
                                            })));

                                        itemBuilder.AddItem(row.Build());
                                        lineIndex++;
                                    }
                                }
                            }
                        }
                        else if (!string.IsNullOrEmpty(errorStateCopy))
                        {
                            Row.Builder row = new Row.Builder()
                                .SetTitle(errorStateCopy);
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
                    else if (dataListCopy != null)
                    {
                        int lineIndex = 0;
                        foreach (DataInfoEntry dataEntry in dataListCopy)
                        {
                            if (lineIndex >= listLimit)
                            {
                                break;
                            }

                            string rowTitle = dataEntry.Title;
                            string result = dataEntry.Result;

                            Row.Builder row = new Row.Builder()
                                .SetTitle(rowTitle);

                            if (!string.IsNullOrEmpty(result))
                            {
                                row.AddText(result);
                            }

                            row.SetOnClickListener(ParkedOnlyOnClickListener.Create(new ActionListener((page) =>
                            {
                                try
                                {
                                    _lastContent = string.Empty;
                                    ScreenManager.Push(new PageDetailScreen(CarContext, CarServiceInst, rowTitle, result));
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            })));

                            itemBuilder.AddItem(row.Build());
                            lineIndex++;
                        }

                        if (lineIndex == 0)
                        {
                            itemBuilder.AddItem(new Row.Builder()
                                .SetTitle(CarContext.GetString(Resource.String.car_service_no_data))
                                .Build());
                        }
                    }
                }

                ListTemplate listTemplate = new ListTemplate.Builder()
                    .SetHeaderAction(AndroidX.Car.App.Model.Action.Back)
                    .SetTitle(pageTitle)
                    .SetSingleList(itemBuilder.Build())
                    .Build();

                RequestUpdate();

                return listTemplate;
            }

            public override bool ContentChanged()
            {
                string newContent = GetContentString();

                bool disconnectedCopy;
                lock (_lockObject)
                {
                    disconnectedCopy = _disconnected;
                }

                if (disconnectedCopy)
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

            private string GetContentString()
            {
                try
                {
                    StringBuilder sbContent = new StringBuilder();
                    EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
                    JobReader.PageInfo pageInfoActive = ediabasThread?.JobPageInfo;
                    string pageTitle = CarContext.GetString(Resource.String.app_name);
                    bool disconnected = false;
                    bool errorPage = false;

                    if (!ActivityCommon.CommActive || pageInfoActive == null)
                    {
                        disconnected = true;
                        sbContent.AppendLine(CarContext.GetString(Resource.String.car_service_disconnected));
                        lock (_lockObject)
                        {
                            _errorState = string.Empty;
                            _errorList = null;
                            _dataList = null;
                        }
                    }
                    else
                    {
                        pageTitle = ActivityMain.GetPageString(pageInfoActive, pageInfoActive.Name);

                        if (pageInfoActive.ErrorsInfo != null)
                        {
                            errorPage = true;
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

                                lock (_lockObject)
                                {
                                    _errorState = state;
                                    _errorList = null;
                                    _dataList = null;
                                }
                            }
                            else
                            {
                                CarServiceInst.EvaluateErrorMessages(pageInfoActive, errorReportList, null,
                                    list =>
                                    {
                                        lock (_lockObject)
                                        {
                                            _errorState = string.Empty;
                                            _errorList = list;
                                            _dataList = null;
                                        }
                                    });
                            }

                            List<ErrorMessageEntry> errorListCopy;
                            string errorStateCopy;
                            lock (_lockObject)
                            {
                                errorStateCopy = _errorState;
                                errorListCopy = _errorList;
                            }

                            int lineIndex = 0;
                            if (errorListCopy != null)
                            {
                                foreach (ErrorMessageEntry errorEntry in errorListCopy)
                                {
                                    string message = errorEntry.Message;
                                    if (!string.IsNullOrEmpty(message))
                                    {
                                        sbContent.AppendLine(message);
                                        lineIndex++;
                                    }
                                }
                            }
                            else if (!string.IsNullOrEmpty(errorStateCopy))
                            {
                                sbContent.AppendLine(errorStateCopy);
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

                            int lineIndex = 0;
                            List<DataInfoEntry> dataList = new List<DataInfoEntry>();
                            foreach (JobReader.DisplayInfo displayInfo in pageInfoActive.DisplayList)
                            {
                                string rowTitle = ActivityMain.GetPageString(pageInfoActive, displayInfo.Name);
                                string result = string.Empty;

                                if (ediabasThread != null && resultDict != null)
                                {
                                    result = ediabasThread.FormatResult(pageInfoActive, displayInfo, resultDict);
                                }

                                sbContent.AppendLine(rowTitle);
                                if (!string.IsNullOrEmpty(result))
                                {
                                    sbContent.AppendLine(result);
                                }

                                dataList.Add(new DataInfoEntry(rowTitle, result));
                                lineIndex++;
                            }

                            if (lineIndex == 0)
                            {
                                sbContent.AppendLine(CarContext.GetString(Resource.String.car_service_no_data));
                            }

                            lock (_lockObject)
                            {
                                _errorState = string.Empty;
                                _errorList = null;
                                _dataList = dataList;
                            }
                        }
                    }

                    lock (_lockObject)
                    {
                        _disconnected = disconnected;
                        _pageTitle = pageTitle;
                        _errorPage = errorPage;
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

        public class PageDetailScreen(CarContext carContext, CarService carService, string title, string message,
            string actionText = null, PageDetailScreen.ActionDelegate actionDelegate = null) : BaseScreen(carContext, carService)
        {
            public delegate void ActionDelegate();

            public override ITemplate OnGetTemplate()
            {
                string itemMessage = message;
                if (string.IsNullOrEmpty(itemMessage))
                {
                    itemMessage = CarContext.GetString(Resource.String.car_service_no_data);
                }
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("PageDetailScreen: OnGetTemplate, Title='{0}', Message='{1}', Action='{2}'",
                    title, itemMessage, actionText ?? string.Empty));
#endif
                if (CarContext.CarAppApiLevel >= 2)
                {
                    AndroidX.Car.App.Model.Action.Builder actionButton = null;
                    if (!string.IsNullOrEmpty(actionText) && actionDelegate != null)
                    {
                        actionButton = new AndroidX.Car.App.Model.Action.Builder()
                            .SetTitle(actionText)
                            .SetOnClickListener(ParkedOnlyOnClickListener.Create(new ActionListener((page) =>
                            {
                                actionDelegate.Invoke();
                                try
                                {
                                    ScreenManager.Pop();
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            })));
                    }

                    LongMessageTemplate.Builder longMessageTemplate = new LongMessageTemplate.Builder(itemMessage)
                        .SetHeaderAction(AndroidX.Car.App.Model.Action.Back)
                        .SetTitle(title);

                    if (actionButton != null)
                    {
                        longMessageTemplate.AddAction(actionButton.Build());
                    }

                    return longMessageTemplate.Build();
                }

                MessageTemplate.Builder messageTemplate = new MessageTemplate.Builder(itemMessage)
                    .SetHeaderAction(AndroidX.Car.App.Model.Action.Back)
                    .SetTitle(title);

                return messageTemplate.Build();
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

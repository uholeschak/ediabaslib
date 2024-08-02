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
        public const int DefaultListItems = 6;
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
            List<string> translationList = new List<string>();
            string errorMessage = ActivityCommon.EdiabasThread?.GenerateErrorMessage(this, _activityCommon, pageInfo, errorReport, errorIndex, formatErrorResult, ref translationList,
                null, ref dtcList);

            if (errorMessage == null)
            {
                return string.Empty;
            }

            return errorMessage;
        }


        public class CarSession(CarService carService) : Session
        {
            public override Screen OnCreateScreen(Intent intent)
            {
                return new MainScreen(CarContext, carService);
            }

            public static int GetContentLimit(CarContext carContext, int contentLimitType, int defaultValue)
            {
                try
                {
                    if (carContext.CarAppApiLevel >= 2)
                    {
                        if (carContext.GetCarService(Java.Lang.Class.FromType(typeof(ConstraintManager))) is ConstraintManager constraintManager)
                        {
                            return constraintManager.GetContentLimit(ConstraintManager.ContentLimitTypeList);
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                return defaultValue;
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
            private Tuple<string, string> _lastContent = null;
            private readonly object _lockObject = new object();
            private bool _disconnected = true;

            public override ITemplate OnGetTemplate()
            {
#if DEBUG
                Android.Util.Log.Info(Tag, "MainScreen: OnGetTemplate");
#endif
                _lastContent = GetContentString();

                string lastStructureContent = _lastContent?.Item1;
                string lastValueContent = _lastContent?.Item2;
                bool loading = lastStructureContent == null || lastValueContent == null;

                bool disconnectedCopy;

                lock (_lockObject)
                {
                    disconnectedCopy = _disconnected;
                }

                ItemList.Builder itemBuilder = new ItemList.Builder();

                Row.Builder rowState = new Row.Builder()
                    .SetTitle(CarContext.GetString(Resource.String.car_service_connection_state))
                    .SetOnClickListener(ParkedOnlyOnClickListener.Create(new ActionListener((page) =>
                    {
                        if (ShowApp())
                        {
                            CarToast.MakeText(CarContext, Resource.String.car_service_app_displayed,
                                CarToast.LengthLong).Show();
                        }
                    })));

                if (disconnectedCopy)
                {
                    rowState.AddText(CarContext.GetString(Resource.String.car_service_disconnected));
                }
                else
                {
                    rowState.AddText(CarContext.GetString(Resource.String.car_service_connected));
                }
                itemBuilder.AddItem(rowState.Build());

                Row.Builder rowPageList = new Row.Builder()
                    .SetTitle(CarContext.GetString(Resource.String.car_service_page_list));
                if (disconnectedCopy)
                {
                    rowPageList.AddText(CarContext.GetString(Resource.String.car_service_page_list_empty));
                }
                else
                {
                    rowPageList.AddText(CarContext.GetString(Resource.String.car_service_page_list_show));
                    rowPageList.SetBrowsable(true);
                    rowPageList.SetOnClickListener(new ActionListener((page) =>
                    {
                        try
                        {
                            ScreenManager.Push(new PageListScreen(CarContext, CarServiceInst));
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }));
                }
                itemBuilder.AddItem(rowPageList.Build());

                ListTemplate.Builder listTemplate = new ListTemplate.Builder()
                    .SetHeaderAction(AndroidX.Car.App.Model.Action.AppIcon)
                    .SetTitle(CarContext.GetString(Resource.String.app_name));
                if (loading)
                {
                    listTemplate.SetLoading(true);
                }
                else
                {
                    listTemplate.SetSingleList(itemBuilder.Build());
                }

                RequestUpdate();

                return listTemplate.Build();
            }

            public override bool ContentChanged()
            {
                Tuple<string, string> newContent = GetContentString();

                string lastStructureContent = _lastContent?.Item1;
                string lastValueContent = _lastContent?.Item2;
                string newStructureContent = newContent?.Item1;
                string newValueContent = newContent?.Item2;

                if (newStructureContent == null || newValueContent == null)
                {   // loading
#if DEBUG
                    Android.Util.Log.Info(Tag, "MainScreen: ContentChanged loading");
#endif
                    return true;
                }

                if (_lastContent != null && string.Compare(lastStructureContent ?? string.Empty, newStructureContent, StringComparison.Ordinal) != 0)
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, "MainScreen: ContentChanged structure has changed");
#endif
                    return false;
                }

                if (string.Compare(lastValueContent ?? string.Empty, newValueContent, StringComparison.Ordinal) != 0)
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, "MainScreen: ContentChanged value has changed");
#endif
                    return true;
                }

                return false;
            }

            private Tuple<string, string> GetContentString()
            {
                try
                {
                    StringBuilder sbStructureContent = new StringBuilder();
                    StringBuilder sbValueContent = new StringBuilder();
                    JobReader.PageInfo pageInfoActive = ActivityCommon.EdiabasThread?.JobPageInfo;

                    sbStructureContent.AppendLine(CarContext.GetString(Resource.String.car_service_connection_state));
                    bool disconnected = !ActivityCommon.CommActive || pageInfoActive == null;

                    sbValueContent.AppendLine();
                    if (disconnected)
                    {
                        sbValueContent.AppendLine(CarContext.GetString(Resource.String.car_service_disconnected));
                    }
                    else
                    {
                        sbValueContent.AppendLine(CarContext.GetString(Resource.String.car_service_connected));
                    }

                    sbStructureContent.AppendLine(CarContext.GetString(Resource.String.car_service_page_list));

                    sbValueContent.AppendLine();
                    if (disconnected)
                    {
                        sbValueContent.AppendLine(CarContext.GetString(Resource.String.car_service_page_list_empty));
                    }
                    else
                    {
                        sbValueContent.AppendLine(CarContext.GetString(Resource.String.car_service_page_list_show));
                    }

                    lock (_lockObject)
                    {
                        _disconnected = disconnected;
                    }

                    return new Tuple<string, string>(sbStructureContent.ToString(), sbValueContent.ToString());
                }
                catch (Exception)
                {
                    return null;
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

        public class PageListScreen(CarContext carContext, CarService carService) : BaseScreen(carContext, carService)
        {
            private Tuple<string, string> _lastContent = null;
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
                Android.Util.Log.Info(Tag, "PageListScreen: OnGetTemplate");
#endif
                _lastContent = GetContentString();

                string lastStructureContent = _lastContent?.Item1;
                string lastValueContent = _lastContent?.Item2;
                bool loading = lastStructureContent == null || lastValueContent == null;

                bool disconnectedCopy;
                List<PageInfoEntry> pageListCopy;

                lock (_lockObject)
                {
                    disconnectedCopy = _disconnected;
                    pageListCopy = _pageList;
                }

                int listLimit = CarSession.GetContentLimit(CarContext, ConstraintManager.ContentLimitTypeList, DefaultListItems);
                ItemList.Builder itemBuilder = new ItemList.Builder();

                if (disconnectedCopy)
                {
                    itemBuilder.AddItem(new Row.Builder()
                        .SetTitle(CarContext.GetString(Resource.String.car_service_connection_state))
                        .AddText(CarContext.GetString(Resource.String.car_service_disconnected))
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
                                    lock (EdiabasThread.DataLock)
                                    {
                                        EdiabasThread ediabasThreadLocal = ActivityCommon.EdiabasThread;
                                        if (ediabasThreadLocal != null)
                                        {
                                            ediabasThreadLocal.JobPageInfo = newPageInfo;
                                        }
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

                ListTemplate.Builder listTemplate = new ListTemplate.Builder()
                    .SetHeaderAction(AndroidX.Car.App.Model.Action.Back)
                    .SetTitle(CarContext.GetString(Resource.String.car_service_page_list));
                if (loading)
                {
                    listTemplate.SetLoading(true);
                }
                else
                {
                    listTemplate.SetSingleList(itemBuilder.Build());
                }

                RequestUpdate();

                return listTemplate.Build();
            }

            public override bool ContentChanged()
            {
                Tuple<string, string> newContent = GetContentString();

                bool disconnectedCopy;
                lock (_lockObject)
                {
                    disconnectedCopy = _disconnected;
                }

                if (disconnectedCopy)
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, "PageListScreen: ContentChanged disconnected");
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

                string lastStructureContent = _lastContent?.Item1;
                string lastValueContent = _lastContent?.Item2;
                string newStructureContent = newContent?.Item1;
                string newValueContent = newContent?.Item2;

                if (newStructureContent == null || newValueContent == null)
                {   // loading
#if DEBUG
                    Android.Util.Log.Info(Tag, "PageListScreen: ContentChanged loading");
#endif
                    return true;
                }

                if (_lastContent != null && string.Compare(lastStructureContent ?? string.Empty, newStructureContent, StringComparison.Ordinal) != 0)
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, "PageListScreen: ContentChanged structure has changed");
#endif
                    try
                    {
                        ScreenManager.Pop();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    return false;
                }

                if (string.Compare(lastValueContent ?? string.Empty, newValueContent, StringComparison.Ordinal) != 0)
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, "PageListScreen: ContentChanged value has changed");
#endif
                    return true;
                }

                return false;
            }

            private Tuple<string, string> GetContentString()
            {
                try
                {
                    StringBuilder sbStructureContent = new StringBuilder();
                    StringBuilder sbValueContent = new StringBuilder();
                    JobReader.PageInfo pageInfoActive = ActivityCommon.EdiabasThread?.JobPageInfo;

                    bool disconnected = false;
                    List<PageInfoEntry> pageList = null;

                    if (!ActivityCommon.CommActive || pageInfoActive == null)
                    {
                        disconnected = true;
                        sbStructureContent.AppendLine(CarContext.GetString(Resource.String.car_service_connection_state));

                        sbValueContent.AppendLine();
                        sbValueContent.AppendLine(CarContext.GetString(Resource.String.car_service_disconnected));
                    }
                    else
                    {
                        pageList = new List<PageInfoEntry>();
                        int pageIndex = 0;
                        foreach (JobReader.PageInfo pageInfo in ActivityCommon.JobReader.PageList)
                        {
                            string pageName = ActivityMain.GetPageString(pageInfo, pageInfo.Name);
                            sbStructureContent.AppendLine(pageName);
                            bool activePage = pageInfo == pageInfoActive;

                            sbValueContent.AppendLine();
                            if (activePage)
                            {
                                sbValueContent.AppendLine(CarContext.GetString(Resource.String.car_service_active_page));
                            }

                            pageList.Add(new PageInfoEntry(pageName, activePage));
                            pageIndex++;
                        }

                        if (pageIndex == 0)
                        {
                            sbStructureContent.AppendLine(CarContext.GetString(Resource.String.car_service_no_pages));
                        }
                    }

                    lock (_lockObject)
                    {
                        _disconnected = disconnected;
                        _pageList = pageList;
                    }

                    return new Tuple<string, string>(sbStructureContent.ToString(), sbValueContent.ToString());
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public class PageScreen(CarContext carContext, CarService carService) : BaseScreen(carContext, carService), IOnScreenResultListener
        {
            private Tuple<string, string, JobReader.PageInfo> _lastContent = null;
            private readonly object _lockObject = new object();
            private bool _disconnected = true;
            private string _pageTitle = string.Empty;
            private bool _errorPage = false;
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

                _lastContent = GetContentString();

                string lastStructureContent = _lastContent?.Item1;
                string lastValueContent = _lastContent?.Item2;
                bool loading = lastStructureContent == null || lastValueContent == null;

                int listLimit = CarSession.GetContentLimit(CarContext, ConstraintManager.ContentLimitTypeList, DefaultListItems);
                ItemList.Builder itemBuilder = new ItemList.Builder();
                string pageTitle = CarContext.GetString(Resource.String.app_name);

                bool disconnectedCopy;
                string pageTitleCopy;
                bool errorPageCopy;
                List<ErrorMessageEntry> errorListCopy;
                List<DataInfoEntry> dataListCopy;

                lock (_lockObject)
                {
                    disconnectedCopy = _disconnected;
                    pageTitleCopy = _pageTitle;
                    errorPageCopy = _errorPage;
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
                        .SetTitle(CarContext.GetString(Resource.String.car_service_connection_state))
                        .AddText(CarContext.GetString(Resource.String.car_service_disconnected))
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
                                                    string actionText = null;
                                                    Java.Lang.String actionResult = null;

                                                    if (validResponse && !shadow)
                                                    {
                                                        actionText = CarContext.GetString(Resource.String.car_service_error_reset);
                                                        actionResult = new Java.Lang.String(ecuName);
                                                    }

                                                    ScreenManager.PushForResult(new PageDetailScreen(CarContext, CarServiceInst, rowTitle, sbText.ToString(),
                                                        actionText, actionResult), this);
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
                        else
                        {
                            loading = true;
                        }

                        if (lineIndex == 0 && !loading)
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

                ListTemplate.Builder listTemplate = new ListTemplate.Builder()
                    .SetHeaderAction(AndroidX.Car.App.Model.Action.Back)
                    .SetTitle(pageTitle);
                if (loading)
                {
                    listTemplate.SetLoading(true);
                }
                else
                {
                    listTemplate.SetSingleList(itemBuilder.Build());
                }

                RequestUpdate();

                return listTemplate.Build();
            }

            public void OnScreenResult(Java.Lang.Object p0)
            {
                Java.Lang.String ecuNameString = p0 as Java.Lang.String;
                if (ecuNameString == null)
                {
                    return;
                }

                if (ActivityCommon.ErrorResetActive)
                {
                    CarToast.MakeText(CarContext, Resource.String.car_service_error_reset_active, CarToast.LengthLong).Show();
                    return;
                }

                string ecuName = ecuNameString.ToString();
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("PageScreen: OnScreenResult Ecu={0}", ecuName));
#endif
                List<string> errorResetList = new List<string>() { ecuName };
                lock (EdiabasThread.DataLock)
                {
                    EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
                    if (ediabasThread != null)
                    {
                        ediabasThread.ErrorResetList = errorResetList;
                    }
                }

                // force app screen update
                ActivityCommon.EdiabasThread?.DataUpdatedEvent(true);
                CarToast.MakeText(CarContext, Resource.String.car_service_error_reset_started, CarToast.LengthLong).Show();
                try
                {
                    ScreenManager.Pop();
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            public override bool ContentChanged()
            {
                Tuple<string, string, JobReader.PageInfo> newContent = GetContentString();

                bool disconnectedCopy;
                bool errorPageCopy;
                lock (_lockObject)
                {
                    disconnectedCopy = _disconnected;
                    errorPageCopy = _errorPage;
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

                string lastStructureContent = _lastContent?.Item1;
                string lastValueContent = _lastContent?.Item2;
                JobReader.PageInfo lastPageInfo = _lastContent?.Item3;
                string newStructureContent = newContent?.Item1;
                string newValueContent = newContent?.Item2;
                JobReader.PageInfo newPageInfo = newContent?.Item3;

                if (lastPageInfo != null && lastPageInfo != newPageInfo)
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, "PageScreen: ContentChanged page has changed");
#endif
                    try
                    {
                        ScreenManager.Pop();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    return false;
                }

                if (newStructureContent == null || newValueContent == null)
                {   // loading
#if DEBUG
                    Android.Util.Log.Info(Tag, "PageScreen: ContentChanged loading");
#endif
                    return true;
                }

                if (errorPageCopy)
                {   // no error page update
                    return false;
                }

                if (_lastContent != null && string.Compare(lastStructureContent ?? string.Empty, newStructureContent, StringComparison.Ordinal) != 0)
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, "PageScreen: ContentChanged structure has changed");
#endif
                    try
                    {
                        ScreenManager.Pop();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    return false;
                }

                if (string.Compare(lastValueContent ?? string.Empty, newValueContent, StringComparison.Ordinal) != 0)
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, "PageScreen: ContentChanged value has changed");
#endif
                    return true;
                }

                return false;
            }

            private Tuple<string, string, JobReader.PageInfo> GetContentString()
            {
                try
                {
                    StringBuilder sbStructureContent = new StringBuilder();
                    StringBuilder sbValueContent = new StringBuilder();
                    JobReader.PageInfo pageInfoActive = ActivityCommon.EdiabasThread?.JobPageInfo;

                    string pageTitle = CarContext.GetString(Resource.String.app_name);
                    bool disconnected = false;
                    bool errorPage = false;
                    bool loading = false;

                    if (!ActivityCommon.CommActive || pageInfoActive == null)
                    {
                        disconnected = true;
                        sbStructureContent.AppendLine(CarContext.GetString(Resource.String.car_service_connection_state));
                        sbValueContent.AppendLine(CarContext.GetString(Resource.String.car_service_disconnected));
                        lock (_lockObject)
                        {
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
                            lock (EdiabasThread.DataLock)
                            {
                                EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
                                if (ediabasThread != null)
                                {
                                    if (ediabasThread.ResultPageInfo == pageInfoActive)
                                    {
                                        errorReportList = ediabasThread.EdiabasErrorReportList;
                                    }
                                }
                            }

                            if (errorReportList == null)
                            {
                                lock (_lockObject)
                                {
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
                                            _errorList = list;
                                            _dataList = null;
                                        }
                                    });
                            }

                            List<ErrorMessageEntry> errorListCopy;
                            lock (_lockObject)
                            {
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
                                        sbStructureContent.AppendLine(message);

                                        sbValueContent.AppendLine();
                                        sbValueContent.AppendLine(message);
                                        lineIndex++;
                                    }
                                }
                            }
                            else
                            {
                                loading = true;
                            }

                            if (lineIndex == 0 && !loading)
                            {
                                sbStructureContent.AppendLine(CarContext.GetString(Resource.String.error_no_error));
                            }
                        }
                        else
                        {
                            MultiMap<string, EdiabasNet.ResultData> resultDict = null;
                            lock (EdiabasThread.DataLock)
                            {
                                EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
                                if (ediabasThread != null)
                                {
                                    if (ediabasThread.ResultPageInfo == pageInfoActive)
                                    {
                                        resultDict = ActivityCommon.EdiabasThread.EdiabasResultDict;
                                    }
                                }
                            }

                            int lineIndex = 0;
                            List<DataInfoEntry> dataList = new List<DataInfoEntry>();
                            foreach (JobReader.DisplayInfo displayInfo in pageInfoActive.DisplayList)
                            {
                                string rowTitle = ActivityMain.GetPageString(pageInfoActive, displayInfo.Name);
                                string result = string.Empty;

                                if (resultDict != null)
                                {
                                    result = ActivityCommon.EdiabasThread?.FormatResult(pageInfoActive, displayInfo, resultDict);
                                }

                                sbStructureContent.AppendLine(rowTitle);
                                sbValueContent.AppendLine();
                                if (!string.IsNullOrEmpty(result))
                                {
                                    sbValueContent.AppendLine(result);
                                }

                                dataList.Add(new DataInfoEntry(rowTitle, result));
                                lineIndex++;
                            }

                            if (lineIndex == 0)
                            {
                                sbStructureContent.AppendLine(CarContext.GetString(Resource.String.car_service_no_data));
                            }

                            lock (_lockObject)
                            {
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

                    sbStructureContent.AppendLine(pageTitle);
                    if (loading)
                    {
                        return new Tuple<string, string, JobReader.PageInfo>(null, null, pageInfoActive);
                    }

                    return new Tuple<string, string, JobReader.PageInfo>(sbStructureContent.ToString(), sbValueContent.ToString(), pageInfoActive);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public class PageDetailScreen(CarContext carContext, CarService carService, string title, string message,
            string actionText = null, Java.Lang.Object actionResult = null) : BaseScreen(carContext, carService)
        {
            public override ITemplate OnGetTemplate()
            {
                string itemMessage = message;
                if (string.IsNullOrEmpty(itemMessage))
                {
                    itemMessage = CarContext.GetString(Resource.String.car_service_no_data);
                }
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("PageDetailScreen: OnGetTemplate, Title='{0}', Message='{1}', Action='{2}'",
                    title ?? string.Empty, itemMessage, actionText ?? string.Empty));
#endif
                if (CarContext.CarAppApiLevel >= 2)
                {
                    ActionStrip.Builder actionStripBuilder = null;
                    if (!string.IsNullOrEmpty(actionText) && actionResult != null)
                    {
                        AndroidX.Car.App.Model.Action.Builder actionButton = new AndroidX.Car.App.Model.Action.Builder()
                            .SetTitle(actionText)
                            .SetOnClickListener(ParkedOnlyOnClickListener.Create(new ActionListener((page) =>
                            {
                                SetResult(actionResult);

                                try
                                {
                                    ScreenManager.Pop();
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            })));

                        actionStripBuilder = new ActionStrip.Builder();
                        actionStripBuilder.AddAction(actionButton.Build());
                    }

                    LongMessageTemplate.Builder longMessageTemplate = new LongMessageTemplate.Builder(itemMessage)
                        .SetHeaderAction(AndroidX.Car.App.Model.Action.Back);
                    if (!string.IsNullOrEmpty(title))
                    {
                        longMessageTemplate.SetTitle(title);
                    }

                    if (actionStripBuilder != null)
                    {
                        longMessageTemplate.SetActionStrip(actionStripBuilder.Build());
                    }

                    return longMessageTemplate.Build();
                }

                MessageTemplate.Builder messageTemplate = new MessageTemplate.Builder(itemMessage)
                    .SetHeaderAction(AndroidX.Car.App.Model.Action.Back);

                if (!string.IsNullOrEmpty(title))
                {
                    messageTemplate.SetTitle(title);
                }

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

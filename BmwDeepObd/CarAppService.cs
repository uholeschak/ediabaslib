using Android.Content;
using Android.OS;
using Android.Service.Controls.Templates;
using AndroidX.Car.App;
using AndroidX.Car.App.Constraints;
using AndroidX.Car.App.Model;
using AndroidX.Car.App.Validation;
using AndroidX.Lifecycle;
using System.Text;

[assembly: Android.App.UsesPermission("androidx.car.app.MAP_TEMPLATES")]
[assembly: Android.App.UsesPermission("androidx.car.app.NAVIGATION_TEMPLATES")]
[assembly: Android.App.UsesPermission("androidx.car.app.ACCESS_SURFACE")]

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
#if DEBUG
        private static readonly string Tag = typeof(CarService).FullName;
#endif
        public const int UpdateInterval = 1000;

        public override void OnCreate()
        {
            base.OnCreate();

            lock (ActivityCommon.GlobalLockObject)
            {
                ConnectEdiabasEvents();
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            lock (ActivityCommon.GlobalLockObject)
            {
                DisconnectEdiabasEvents();
            }
        }

        public override HostValidator CreateHostValidator()
        {
            return HostValidator.AllowAllHostsValidator;
        }

        public override Session OnCreateSession()
        {
            return new CarSession();
        }

        private void ConnectEdiabasEvents()
        {
            if (ActivityCommon.EdiabasThread != null)
            {
                ActivityCommon.EdiabasThread.DataUpdated += DataUpdated;
                ActivityCommon.EdiabasThread.PageChanged += PageChanged;
                ActivityCommon.EdiabasThread.ThreadTerminated += ThreadTerminated;
            }
        }

        private void DisconnectEdiabasEvents()
        {
            if (ActivityCommon.EdiabasThread != null)
            {
                ActivityCommon.EdiabasThread.DataUpdated -= DataUpdated;
                ActivityCommon.EdiabasThread.PageChanged -= PageChanged;
                ActivityCommon.EdiabasThread.ThreadTerminated -= ThreadTerminated;
            }
        }

        private void DataUpdated(object sender, System.EventArgs e)
        {
        }

        private void PageChanged(object sender, System.EventArgs e)
        {
        }

        private void ThreadTerminated(object sender, System.EventArgs e)
        {
        }

        public class CarSession : Session
        {
            public override Screen OnCreateScreen(Intent intent)
            {
                return new MainScreen(CarContext);
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
                catch (System.Exception)
                {
                    // ignored
                }

                return 0;
            }
        }

        public class BaseScreen : Screen, ILifecycleEventObserver
        {
            private readonly Handler _updateHandler;
            private readonly UpdateScreenRunnable _updateScreenRunnable;

            public BaseScreen(CarContext carContext) : base(carContext)
            {
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("BaseScreen: Class={0}", GetType().FullName));
#endif
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

        public class MainScreen(CarContext carContext) : BaseScreen(carContext)
        {
            private string _lastContent = string.Empty;

            public override ITemplate OnGetTemplate()
            {
#if DEBUG
                Android.Util.Log.Info(Tag, "MainScreen: OnGetTemplate");
#endif
                int listLimit = CarSession.GetContentLimit(CarContext, ConstraintManager.ContentLimitTypeList);

                ItemList.Builder itemBuilder = new ItemList.Builder();
                if (!ActivityCommon.CommActive)
                {
                    string title = "Vehicle is disconnected";
                    string text = "Connect the vehicle in the app first";

                    itemBuilder.AddItem(new Row.Builder()
                        .SetTitle(title)
                        .AddText(text)
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
                        itemBuilder.AddItem(new Row.Builder()
                            .SetTitle(pageName)
                            .SetBrowsable(true)
                            .SetOnClickListener(new ActionListener((page) =>
                            {
                                int index = -1;
                                if (page is int pageValue)
                                {
                                    index = pageValue;
                                }
                                ScreenManager.Push(new PageScreen(CarContext, index));
                            }, pageIndex))
                            .Build());
                        pageIndex++;
                    }
                }

                ListTemplate listTemplate = new ListTemplate.Builder()
                    .SetHeaderAction(Action.AppIcon)
                    .SetTitle("Main page")
                    .SetSingleList(itemBuilder.Build())
                    .Build();

                _lastContent = GetContentString();

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
                    if (!ActivityCommon.CommActive)
                    {
                        sbContent.Append("NoComm");
                    }
                    else
                    {
                        foreach (JobReader.PageInfo pageInfo in ActivityCommon.JobReader.PageList)
                        {
                            string pageName = ActivityMain.GetPageString(pageInfo, pageInfo.Name);
                            sbContent.Append(pageName);
                        }
                    }

                    return sbContent.ToString();
                }
                catch (System.Exception)
                {
                    return string.Empty;
                }
            }
        }

        public class PageScreen(CarContext carContext, int pageIndex) : BaseScreen(carContext)
        {
            public override ITemplate OnGetTemplate()
            {
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("PageScreen: OnGetTemplate Index={0}", pageIndex));
#endif

                int listLimit = CarSession.GetContentLimit(CarContext, ConstraintManager.ContentLimitTypeList);
                int listSize = 10;
                if (listLimit < listSize)
                {
                    listSize = listLimit;
                }

                ItemList.Builder itemBuilder = new ItemList.Builder();
                for (int i = 0; i < listSize; i++)
                {
                    itemBuilder.AddItem(
                        new Row.Builder()
                        .SetTitle($"Row title {i + 1}")
                        .AddText($"Row text {i + 1}")
                        .Build());
                }

                ListTemplate listTemplate = new ListTemplate.Builder()
                    .SetHeaderAction(Action.Back)
                    .SetTitle($"Page {pageIndex + 1}")
                    .SetSingleList(itemBuilder.Build())
                    .Build();

                RequestUpdate();
                return listTemplate;
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
                catch (System.Exception)
                {
                    // ignored
                }
            }
        }
    }
}

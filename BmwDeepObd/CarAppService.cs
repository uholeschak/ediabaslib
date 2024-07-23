using Android.Content;
using AndroidX.Car.App;
using AndroidX.Car.App.Model;
using AndroidX.Car.App.Validation;

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
        public CarService()
        {
            // Exported services must have an empty public constructor.
        }

        public override HostValidator CreateHostValidator()
        {
            return HostValidator.AllowAllHostsValidator;
        }

        public override Session OnCreateSession()
        {
            return new CarSession();
        }

        public class CarSession : Session
        {
            public override Screen OnCreateScreen(Intent intent)
            {
                return new MainScreen(CarContext);
            }
        }

        public class MainScreen(CarContext carContext) : Screen(carContext)
        {
            public override ITemplate OnGetTemplate()
            {
                Pane.Builder paneBuilder = new Pane.Builder();
                for (int i = 0; i < 10; i++)
                {
                    paneBuilder.AddRow(new Row.Builder()
                        .SetTitle($"Page {i + 1}")
                        .AddText($"Show page {i + 1}")
                        .AddAction(new Action.Builder().SetTitle($"Show page {i + 1}")
                            .SetIcon(CarIcon.ComposeMessage)
                            .SetOnClickListener(new ActionListener((page) =>
                            {
                                int pageIndex = -1;
                                if (page is int pageValue)
                                {
                                    pageIndex = pageValue;
                                }
                                ScreenManager.Push(new PageScreen(CarContext, pageIndex));
                            }, i)).Build())
                        .Build());
                }

                return new PaneTemplate.Builder(paneBuilder.Build())
                    .SetHeaderAction(Action.AppIcon)
                    .SetTitle("Main page")
                    .Build();
            }
        }

        public class PageScreen(CarContext carContext, int pageIndex) : Screen(carContext)
        {
            public override ITemplate OnGetTemplate()
            {
                Pane.Builder paneBuilder = new Pane.Builder();
                for (int i = 0; i < 10; i++)
                {
                    paneBuilder.AddRow(new Row.Builder().SetTitle($"Row {i + 1}").Build());
                }

                return new PaneTemplate.Builder(paneBuilder.Build())
                    .SetHeaderAction(Action.Back)
                    .SetTitle($"Page {pageIndex + 1}")
                    .Build();
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
    }
}

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
    // cd SDK_LOCATION/extras/google/auto
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
#if false
                ItemList.Builder itemList = new ItemList.Builder();

                itemList.AddItem(new GridItem.Builder()
                    .SetTitle("Item1")
                    .SetLoading(true)
                    .Build());

                itemList.AddItem(new GridItem.Builder()
                    .SetTitle("Item2")
                    .SetLoading(true)
                    .Build());

                return new GridTemplate.Builder().SetTitle("Items")
                    .SetLoading(false)
                    .SetSingleList(itemList.Build())
                    .Build();
#else
                Pane.Builder paneBuilder = new Pane.Builder();
                paneBuilder.AddRow(new Row.Builder()
                    .SetTitle("Page 1")
                    .AddAction(new Action.Builder().SetTitle("Action1")
                        .SetIcon(CarIcon.AppIcon)
                        .SetOnClickListener(new ActionListener((sender,
                        args) =>
                    {
                        ScreenManager.Push(new MainScreen(CarContext));
                    })).Build())
                    .Build());
                paneBuilder.AddRow(new Row.Builder().SetTitle("Row2").Build());
                return new PaneTemplate.Builder(paneBuilder.Build())
                    .SetHeaderAction(Action.Back)
                    .Build();
#endif
            }
        }

        public class ActionListener : Java.Lang.Object, IOnClickListener
        {
            private System.EventHandler _handler;

            public ActionListener(System.EventHandler handler)
            {
                _handler = handler;
            }

            public void OnClick()
            {
            }
        }
    }
}

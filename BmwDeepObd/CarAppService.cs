using Android.Content;
using AndroidX.Car.App;
using AndroidX.Car.App.Model;
using AndroidX.Car.App.Validation;

[assembly: Android.App.UsesFeature("android.hardware.type.automotive", Required = true)]
[assembly: Android.App.UsesFeature("android.software.car.templates_host", Required = true)]
[assembly: Android.App.UsesFeature("android.hardware.wifi", Required = false)]
[assembly: Android.App.UsesFeature("android.hardware.screen.portrait", Required = false)]
[assembly: Android.App.UsesFeature("android.hardware.screen.landscape", Required = false)]

namespace BmwDeepObd
{
    [Android.App.Service(
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
                ItemList.Builder listBuilder = new ItemList.Builder();

                listBuilder.AddItem(new GridItem.Builder()
                    .SetTitle("Item1")
                    .Build());

                listBuilder.AddItem(new GridItem.Builder()
                    .SetTitle("Item2")
                    .Build());

                return new GridTemplate.Builder().SetTitle("Items")
                    .SetHeaderAction(Action.AppIcon)
                    .SetSingleList(listBuilder.Build())
                    .Build();
            }
        }
    }
}

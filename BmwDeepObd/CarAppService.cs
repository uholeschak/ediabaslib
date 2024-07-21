using Android.Content;
using AndroidX.Car.App;
using AndroidX.Car.App.Model;
using AndroidX.Car.App.Validation;

[assembly: Android.App.UsesFeature("android.hardware.type.automotive", Required = true)]

namespace BmwDeepObd
{
    [Android.App.MetaData("androidx.car.app.minCarApiLevel", Value = "1")]

    [Android.App.Service(Label = "@string/app_name",
        Exported = true,
        Enabled = true,
        Name = ActivityCommon.AppNameSpace + "." + nameof(CarService)
    )]
    [Android.App.IntentFilter(new[]
        {
            IntentCarAppService,
        },
        Categories = new[]
        {
            CategoryCarAppIot
        })
    ]

    // Testing with Desktop Head Unit (DHU)
    // https://developer.android.com/training/cars/testing/dhu
    // Start Android Auto Emulation Server on the smartphone
    // Select: Connect vehicle
    // adb forward tcp:5277 tcp:5277
    // cd SDK_LOCATION/extras/google/auto
    // desktop-head-unit.exe

    public class CarService : CarAppService
    {
        public const string IntentCarAppService = "androidx.car.app.CarAppService";
        public const string CategoryCarAppIot = "androidx.car.app.category.IOT";

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

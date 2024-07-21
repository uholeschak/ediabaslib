using Android.Content;
using AndroidX.Car.App;
using AndroidX.Car.App.Validation;

namespace BmwDeepObd;

[Android.App.Service(
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
            //return MainScreen(CarContext);
            return null;
        }
    }
}

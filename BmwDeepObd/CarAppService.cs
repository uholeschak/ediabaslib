using Android.Content;
using AndroidX.Car.App;
using AndroidX.Car.App.Model;
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

using Android.Content;
using AndroidX.Car.App;
using AndroidX.Car.App.Constraints;
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

        public class MainScreen(CarContext carContext) : Screen(carContext)
        {
            public override ITemplate OnGetTemplate()
            {
                int listLimit = CarSession.GetContentLimit(CarContext, ConstraintManager.ContentLimitTypeList);
                int listSize = 10;
                if (listLimit < listSize)
                {
                    listSize = listLimit;
                }

                ItemList.Builder itemBuilder = new ItemList.Builder();
                for (int i = 0; i < listSize; i++)
                {
                    itemBuilder.AddItem(new Row.Builder()
                        .SetTitle($"Page {i + 1}")
                        .AddText($"Show page {i + 1}")
                        .SetBrowsable(true)
                        .SetOnClickListener(new ActionListener((page) =>
                        {
                            int pageIndex = -1;
                            if (page is int pageValue)
                            {
                                pageIndex = pageValue;
                            }
                            ScreenManager.Push(new PageScreen(CarContext, pageIndex));
                        }))
                        .Build());
                }

                return new ListTemplate.Builder()
                    .SetHeaderAction(Action.AppIcon)
                    .SetTitle("Main page")
                    .SetSingleList(itemBuilder.Build())
                    .Build();
            }
        }

        public class PageScreen(CarContext carContext, int pageIndex) : Screen(carContext)
        {
            public override ITemplate OnGetTemplate()
            {
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

                return new ListTemplate.Builder()
                    .SetHeaderAction(Action.Back)
                    .SetTitle($"Page {pageIndex + 1}")
                    .SetSingleList(itemBuilder.Build())
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

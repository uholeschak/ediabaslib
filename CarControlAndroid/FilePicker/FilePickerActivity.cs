using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Views;

namespace CarControlAndroid.FilePicker
{
    [Android.App.Activity(Label = "@string/select_file",
            ConfigurationChanges = Android.Content.PM.ConfigChanges.KeyboardHidden |
                Android.Content.PM.ConfigChanges.Orientation |
                Android.Content.PM.ConfigChanges.ScreenSize)]
    public class FilePickerActivity : AppCompatActivity
    {
        // Intent extra
        public const string ExtraTitle = "title";
        public const string ExtraInitDir = "init_dir";
        public const string ExtraFileName = "file_name";
        public const string ExtraFileExtensions = "file_extensions";

        public delegate void FilterEventHandler(string fileNamefilter);
        public event FilterEventHandler FilterEvent;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            string title = Intent.GetStringExtra(ExtraTitle);
            if (!string.IsNullOrEmpty(title))
            {
                SupportActionBar.Title = title;
            }
            SetContentView(Resource.Layout.file_picker);

            // Set result CANCELED incase the user backs out
            SetResult(Android.App.Result.Canceled);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.file_picker_menu, menu);
            IMenuItem menuSearch = menu.FindItem(Resource.Id.action_search);
            if (menuSearch != null)
            {
                menuSearch.SetActionView(new Android.Support.V7.Widget.SearchView(this));

                View searchView = MenuItemCompat.GetActionView(menuSearch);
                var searchViewV7 = searchView as Android.Support.V7.Widget.SearchView;
                if (searchViewV7 != null)
                {
                    searchViewV7.QueryTextChange += (sender, e) =>
                    {
                        e.Handled = OnQueryTextChange(e.NewText);
                    };

                    searchViewV7.QueryTextSubmit += (sender, e) =>
                    {
                        e.Handled = OnQueryTextChange(e.Query);
                    };
                }
            }

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private bool OnQueryTextChange(string text)
        {
            OnFilterEvent(text);
            return true;
        }

        protected virtual void OnFilterEvent(string filter)
        {
            var handler = FilterEvent;
            if (handler != null) handler(filter);
        }
    }
}

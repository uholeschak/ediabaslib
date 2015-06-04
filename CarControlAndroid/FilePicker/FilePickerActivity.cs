using Android.OS;
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
    }
}

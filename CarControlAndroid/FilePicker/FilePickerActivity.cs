namespace com.xamarin.recipes.filepicker
{
    using Android.OS;
    using Android.Support.V7.App;
    using Android.Views;
    using CarControlAndroid;

    [Android.App.Activity(Label = "@string/select_file",
            ConfigurationChanges = Android.Content.PM.ConfigChanges.KeyboardHidden |
                Android.Content.PM.ConfigChanges.Orientation |
                Android.Content.PM.ConfigChanges.ScreenSize)]
    public class FilePickerActivity : AppCompatActivity
    {
        // Intent extra
        public const string EXTRA_TITLE = "title";
        public const string EXTRA_INIT_DIR = "init_dir";
        public const string EXTRA_FILE_NAME = "file_name";
        public const string EXTRA_FILE_EXTENSIONS = "file_extensions";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            string title = Intent.GetStringExtra(FilePickerActivity.EXTRA_TITLE);
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

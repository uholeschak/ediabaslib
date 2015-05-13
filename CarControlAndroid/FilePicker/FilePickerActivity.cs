namespace com.xamarin.recipes.filepicker
{
    using Android.OS;
    using Android.Support.V4.App;
    using Android.Support.V7.App;
    using CarControlAndroid;

    [Android.App.Activity(Label = "@string/select_file", Theme = "@style/Theme.AppCompat")]
#pragma warning disable 618
    public class FilePickerActivity : ActionBarActivity
#pragma warning restore 618
    {
        // Return Intent extra
        public const string EXTRA_INIT_DIR = "init_dir";
        public const string EXTRA_FILE_NAME = "file_name";
        public string initDir;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.file_picker);

            // Set result CANCELED incase the user backs out
            SetResult(Android.App.Result.Canceled);
        }
    }
}

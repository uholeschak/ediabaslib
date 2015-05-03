namespace com.xamarin.recipes.filepicker
{
    using Android.App;
    using Android.OS;
    using Android.Support.V4.App;
    using CarControlAndroid;

    [Activity(Label = "@string/select_file")]
    public class FilePickerActivity : FragmentActivity
    {
        // Return Intent extra
        public const string EXTRA_FILE_NAME = "file_name";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.file_picker);

            // Set result CANCELED incase the user backs out
            SetResult(Result.Canceled);
        }
    }
}

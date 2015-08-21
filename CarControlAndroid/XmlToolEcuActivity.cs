using System;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CarControlAndroid
{
    [Android.App.Activity(
        ConfigurationChanges = Android.Content.PM.ConfigChanges.KeyboardHidden |
                               Android.Content.PM.ConfigChanges.Orientation |
                               Android.Content.PM.ConfigChanges.ScreenSize)]
    public class XmlToolEcuActivity : AppCompatActivity
    {
        public class JobInfo
        {
            public JobInfo(string name)
            {
                _name = name;
                _comments = new List<string>();
                Selected = false;
            }

            private readonly string _name;
            private readonly List<string> _comments;

            public string Name
            {
                get { return _name; }
            }

            public List<string> Comments
            {
                get
                {
                    return _comments;
                }
            }

            public uint ArgCount { get; set; }

            public bool Selected { get; set; }
        }

        // Intent extra
        public const string ExtraEcuName = "ecu_name";

        private JobListAdapter _jobListAdapter;
        private TextView _textViewJobInfo;
        private List<JobInfo> _jobList;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            //SupportActionBar.SetDisplayShowCustomEnabled(true);
            SupportActionBar.Title = string.Format(GetString(Resource.String.xml_tool_ecu_title), Intent.GetStringExtra(ExtraEcuName) ?? string.Empty);
            SetContentView(Resource.Layout.xml_tool_ecu);

            SetResult(Android.App.Result.Canceled);

            _jobList = XmlToolActivity.JobList;

            ListView listViewJobs = FindViewById<ListView>(Resource.Id.listJobs);
            _jobListAdapter = new JobListAdapter(this);
            listViewJobs.Adapter = _jobListAdapter;
            listViewJobs.ItemClick += (sender, args) =>
            {
                int pos = args.Position;
                if (pos >= 0)
                {
                    _textViewJobInfo.Text =_jobListAdapter.Items[pos].Name;
                }
            };
            _textViewJobInfo = FindViewById<TextView>(Resource.Id.textViewJobInfo);

            _textViewJobInfo.Text = string.Empty;
            UpdateDisplay();
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

        private void UpdateDisplay()
        {
            _jobListAdapter.Items.Clear();
            foreach (JobInfo job in _jobList.OrderBy(x => x.Name))
            {
                if (job.Name.StartsWith("STATUS_", StringComparison.OrdinalIgnoreCase) && job.ArgCount == 0)
                {
                    _jobListAdapter.Items.Add(job);
                }
            }
            _jobListAdapter.NotifyDataSetChanged();
        }

        private class JobListAdapter : BaseAdapter<JobInfo>
        {
            private readonly List<JobInfo> _items;

            public List<JobInfo> Items
            {
                get { return _items; }
            }

            private readonly Android.App.Activity _context;
            private bool _ignoreCheckEvent;

            public JobListAdapter(Android.App.Activity context)
            {
                _context = context;
                _items = new List<JobInfo>();
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override JobInfo this[int position]
            {
                get { return _items[position]; }
            }

            public override int Count
            {
                get { return _items.Count; }
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var item = _items[position];

                View view = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.job_select_list, null);
                CheckBox checkBoxSelect = view.FindViewById<CheckBox>(Resource.Id.checkBoxJobSelect);
                _ignoreCheckEvent = true;
                checkBoxSelect.Checked = item.Selected;
                _ignoreCheckEvent = false;

                checkBoxSelect.Tag = new TagInfo(item);
                checkBoxSelect.CheckedChange -= OnCheckChanged;
                checkBoxSelect.CheckedChange += OnCheckChanged;

                TextView textJobName = view.FindViewById<TextView>(Resource.Id.textJobName);
                TextView textJobDesc = view.FindViewById<TextView>(Resource.Id.textJobDesc);
                textJobName.Text = item.Name;

                StringBuilder stringBuilderComments = new StringBuilder();
                foreach (string comment in item.Comments)
                {
                    if (stringBuilderComments.Length > 0)
                    {
                        stringBuilderComments.Append("; ");
                    }
                    stringBuilderComments.Append(comment);
                }
                textJobDesc.Text = stringBuilderComments.ToString();

                return view;
            }

            private void OnCheckChanged(object sender, CompoundButton.CheckedChangeEventArgs args)
            {
                if (!_ignoreCheckEvent)
                {
                    CheckBox checkBox = (CheckBox) sender;
                    TagInfo tagInfo = (TagInfo) checkBox.Tag;
                    if (tagInfo.Info.Selected != args.IsChecked)
                    {
                        NotifyDataSetChanged();
                    }
                    tagInfo.Info.Selected = args.IsChecked;
                }
            }

            private class TagInfo : Java.Lang.Object
            {
                public TagInfo(JobInfo info)
                {
                    _info = info;
                }

                private readonly JobInfo _info;

                public JobInfo Info
                {
                    get { return _info; }
                }
            }
        }
    }
}

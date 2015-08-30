using System;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Views.InputMethods;

namespace CarControlAndroid
{
    [Android.App.Activity(
        ConfigurationChanges = Android.Content.PM.ConfigChanges.KeyboardHidden |
                               Android.Content.PM.ConfigChanges.Orientation |
                               Android.Content.PM.ConfigChanges.ScreenSize)]
    public class XmlToolEcuActivity : AppCompatActivity, View.IOnTouchListener
    {
        public class JobInfo
        {
            public JobInfo(string name)
            {
                _name = name;
                _comments = new List<string>();
                Selected = false;
                Format = string.Empty;
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

            public string Format { get; set; }
        }

        // Intent extra
        public const string ExtraEcuName = "ecu_name";

        public static List<JobInfo> IntentJobList { get; set; }
        private InputMethodManager _imm;
        private ListView _listViewJobs;
        private JobListAdapter _jobListAdapter;
        private LinearLayout _layoutJobConfig;
        private TextView _textViewJobConfig;
        private TextView _textViewFormatDot;
        private EditText _editTextFormat;
        private Spinner _spinnerFormatPos;
        private ArrayAdapter<string> _spinnerFormatPosAdapter;
        private Spinner _spinnerFormatLength1;
        private ArrayAdapter<string> _spinnerFormatLength1Adapter;
        private Spinner _spinnerFormatLength2;
        private ArrayAdapter<string> _spinnerFormatLength2Adapter;
        private Spinner _spinnerFormatType;
        private ArrayAdapter<string> _spinnerFormatTypeAdapter;
        private List<JobInfo> _jobList;
        private JobInfo _selectedJob;
        private bool _ignoreFormatSelection;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            //SupportActionBar.SetDisplayShowCustomEnabled(true);
            SupportActionBar.Title = string.Format(GetString(Resource.String.xml_tool_ecu_title), Intent.GetStringExtra(ExtraEcuName) ?? string.Empty);
            SetContentView(Resource.Layout.xml_tool_ecu);

            _imm = (InputMethodManager)GetSystemService(InputMethodService);

            SetResult(Android.App.Result.Canceled);

            _jobList = IntentJobList;

            _listViewJobs = FindViewById<ListView>(Resource.Id.listJobs);
            _jobListAdapter = new JobListAdapter(this);
            _listViewJobs.Adapter = _jobListAdapter;
            _listViewJobs.SetOnTouchListener(this);
            _listViewJobs.ItemClick += (sender, args) =>
            {
                int pos = args.Position;
                if (pos >= 0)
                {
                    _selectedJob = _jobListAdapter.Items[pos];
                    _layoutJobConfig.Visibility = ViewStates.Visible;
                    _textViewJobConfig.Text = string.Format(GetString(Resource.String.xml_tool_ecu_job_config), _selectedJob.Name);
                    UpdateFormatFields(_selectedJob, false, true);
                }
                else
                {
                    _selectedJob = null;
                    _layoutJobConfig.Visibility = ViewStates.Gone;
                }
            };

            _layoutJobConfig = FindViewById<LinearLayout>(Resource.Id.layoutJobConfig);
            _layoutJobConfig.SetOnTouchListener(this);
            _textViewJobConfig = FindViewById<TextView>(Resource.Id.textViewJobConfig);
            _textViewFormatDot = FindViewById<TextView>(Resource.Id.textViewFormatDot);
            _editTextFormat = FindViewById<EditText>(Resource.Id.editTextFormat);

            _spinnerFormatPos = FindViewById<Spinner>(Resource.Id.spinnerFormatPos);
            _spinnerFormatPosAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem);
            _spinnerFormatPos.Adapter = _spinnerFormatPosAdapter;
            _spinnerFormatPosAdapter.Add(GetString(Resource.String.xml_tool_ecu_format_right));
            _spinnerFormatPosAdapter.Add(GetString(Resource.String.xml_tool_ecu_format_left));
            _spinnerFormatPos.ItemSelected += FormatItemSelected;

            _spinnerFormatLength1 = FindViewById<Spinner>(Resource.Id.spinnerFormatLength1);
            _spinnerFormatLength1Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem);
            _spinnerFormatLength1.Adapter = _spinnerFormatLength1Adapter;
            _spinnerFormatLength1Adapter.Add("--");
            for (int i = 0; i <= 10; i++)
            {
                _spinnerFormatLength1Adapter.Add(i.ToString());
            }
            _spinnerFormatLength1.ItemSelected += FormatItemSelected;

            _spinnerFormatLength2 = FindViewById<Spinner>(Resource.Id.spinnerFormatLength2);
            _spinnerFormatLength2Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem);
            _spinnerFormatLength2.Adapter = _spinnerFormatLength2Adapter;
            _spinnerFormatLength2Adapter.Add("--");
            for (int i = 0; i <= 10; i++)
            {
                _spinnerFormatLength2Adapter.Add(i.ToString());
            }
            _spinnerFormatLength2.ItemSelected += FormatItemSelected;

            _spinnerFormatType = FindViewById<Spinner>(Resource.Id.spinnerFormatType);
            _spinnerFormatTypeAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem);
            _spinnerFormatType.Adapter = _spinnerFormatTypeAdapter;
            _spinnerFormatTypeAdapter.Add("--");
            _spinnerFormatTypeAdapter.Add(GetString(Resource.String.xml_tool_ecu_user_format));
            _spinnerFormatTypeAdapter.Add("(R)eal");
            _spinnerFormatTypeAdapter.Add("(L)ong");
            _spinnerFormatTypeAdapter.Add("(D)ouble");
            _spinnerFormatTypeAdapter.Add("(T)ext");
            _spinnerFormatType.ItemSelected += FormatItemSelected;

            _layoutJobConfig.Visibility = ViewStates.Gone;
            _textViewJobConfig.Text = string.Empty;
            UpdateDisplay();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            HideKeyboard();
            UpdateFormatString(_selectedJob);
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    UpdateFormatString(_selectedJob);
                    HideKeyboard();
                    break;
            }
            return false;
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

        private void UpdateFormatFields(JobInfo jobInfo, bool userFormat, bool initialCall = false)
        {
            string format = jobInfo.Format;
            string parseString = format;
            Int32 length1 = -1;
            Int32 length2 = -1;
            char convertType = '\0';
            bool leftAlign = false;
            if (!string.IsNullOrEmpty(parseString))
            {
                if (parseString[0] == '-')
                {
                    leftAlign = true;
                    parseString = parseString.Substring(1);
                }
            }
            if (!string.IsNullOrEmpty(parseString))
            {
                convertType = parseString[parseString.Length - 1];
                parseString = parseString.Remove(parseString.Length - 1, 1);
            }
            if (!string.IsNullOrEmpty(parseString))
            {
                string[] words = parseString.Split('.');
                try
                {
                    if (words.Length > 0)
                    {
                        if (words[0].Length > 0)
                        {
                            length1 = Convert.ToInt32(words[0], 10);
                        }
                    }
                    if (words.Length > 1)
                    {
                        if (words[1].Length > 0)
                        {
                            length2 = Convert.ToInt32(words[1], 10);
                        }
                    }
                }
                catch (Exception)
                {
                    length1 = -1;
                    length2 = -1;
                }
            }

            _ignoreFormatSelection = true;
            int selection = 1;
            switch (convertType)
            {
                case '\0':
                    selection = 0;
                    break;

                case 'R':
                    selection = 2;
                    break;

                case 'L':
                    selection = 3;
                    break;

                case 'D':
                    selection = 4;
                    break;

                case 'T':
                    selection = 5;
                    break;
            }
            if (userFormat)
            {
                selection = 1;
            }

            _spinnerFormatType.SetSelection(selection);

            if (selection > 0)
            {
                _spinnerFormatPos.Enabled = true;
                _spinnerFormatPos.SetSelection(leftAlign ? 1 : 0);

                int index1 = length1 + 1;
                if (length1 < 0)
                {
                    index1 = 0;
                }
                if (index1 > _spinnerFormatLength1Adapter.Count)
                {
                    index1 = 0;
                }
                _spinnerFormatLength1.Enabled = true;
                _spinnerFormatLength1.SetSelection(index1);

                int index2 = length2 + 1;
                if (length2 < 0)
                {
                    index2 = 0;
                }
                if (index2 > _spinnerFormatLength2Adapter.Count)
                {
                    index2 = 0;
                }
                _spinnerFormatLength2.Enabled = true;
                _spinnerFormatLength2.SetSelection(index2);
            }
            else
            {
                _spinnerFormatPos.Enabled = false;
                _spinnerFormatPos.SetSelection(0);

                _spinnerFormatLength1.Enabled = false;
                _spinnerFormatLength1.SetSelection(0);

                _spinnerFormatLength2.Enabled = false;
                _spinnerFormatLength2.SetSelection(0);
            }

            if (initialCall)
            {
                if (GetFormatString() != format)
                {
                    selection = 1;
                    _spinnerFormatType.SetSelection(selection);
                }
            }
            _editTextFormat.Text = format;
            _ignoreFormatSelection = false;

            ViewStates viewState;
            if (selection == 1)
            {
                _editTextFormat.Visibility = ViewStates.Visible;
                viewState = ViewStates.Gone;
            }
            else
            {
                _editTextFormat.Visibility = ViewStates.Gone;
                viewState = ViewStates.Visible;
            }
            _spinnerFormatPos.Visibility = viewState;
            _spinnerFormatLength1.Visibility = viewState;
            _textViewFormatDot.Visibility = viewState;
            _spinnerFormatLength2.Visibility = viewState;
        }

        private string GetFormatString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            string convertType = string.Empty;
            switch (_spinnerFormatType.SelectedItemPosition)
            {
                case 1:
                    stringBuilder.Append(_editTextFormat.Text);
                    break;

                case 2:
                    convertType = "R";
                    break;

                case 3:
                    convertType = "L";
                    break;

                case 4:
                    convertType = "D";
                    break;

                case 5:
                    convertType = "T";
                    break;
            }
            if (!string.IsNullOrEmpty(convertType))
            {
                if (_spinnerFormatPos.SelectedItemPosition > 0)
                {
                    stringBuilder.Append("-");
                }
                if (_spinnerFormatLength1.SelectedItemPosition > 0)
                {
                    stringBuilder.Append((_spinnerFormatLength1.SelectedItemPosition - 1).ToString());
                }
                if (_spinnerFormatLength2.SelectedItemPosition > 0)
                {
                    stringBuilder.Append(".");
                    stringBuilder.Append((_spinnerFormatLength2.SelectedItemPosition - 1).ToString());
                }
                stringBuilder.Append(convertType);
            }

            return stringBuilder.ToString();
        }

        private void UpdateFormatString(JobInfo jobInfo)
        {
            if ((jobInfo == null) || _ignoreFormatSelection)
            {
                return;
            }
            jobInfo.Format = GetFormatString();
            UpdateFormatFields(jobInfo, _spinnerFormatType.SelectedItemPosition == 1);
        }

        private void FormatItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            HideKeyboard();
            UpdateFormatString(_selectedJob);
        }

        private void HideKeyboard()
        {
            if (_imm != null)
            {
                _imm.HideSoftInputFromWindow(_editTextFormat.WindowToken, HideSoftInputFlags.None);
            }
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

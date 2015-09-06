using System;
using System.Threading.Tasks;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using EdiabasLib;
using System.Threading;

namespace CarControlAndroid
{
    [Android.App.Activity(Label = "@string/can_adapter_title",
            ConfigurationChanges = Android.Content.PM.ConfigChanges.KeyboardHidden |
                        Android.Content.PM.ConfigChanges.Orientation |
                        Android.Content.PM.ConfigChanges.ScreenSize)]
    public class CanAdapterActivity : AppCompatActivity
    {
        // Intent extra
        public const string ExtraDeviceAddress = "device_address";

        private View _barView;
        private Button _buttonRead;
        private Button _buttonWrite;
        private Spinner _spinnerCanAdapterMode;
        private StringAdapter _spinnerCanAdapterModeAdapter;
        private Spinner _spinnerCanAdapterSepTime;
        private StringAdapter _spinnerCanAdapterSepTimeAdapter;
        private Spinner _spinnerCanAdapterBlockSize;
        private StringAdapter _spinnerCanAdapterBlockSizeAdapter;
        private TextView _textViewIgnitionState;
        private string _deviceAddress = string.Empty;
        private int _blockSize = -1;
        private int _separationTime = -1;
        private int _canMode = -1;
        private int _ignitionState = -1;
        private ActivityCommon _activityCommon;
        private EdiabasNet _ediabas;
        private Task _adapterTask;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowCustomEnabled(true);
            SetContentView(Resource.Layout.can_adapter_config);

            _barView = LayoutInflater.Inflate(Resource.Layout.bar_can_adapter, null);
            ActionBar.LayoutParams barLayoutParams = new ActionBar.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent);
            barLayoutParams.Gravity = barLayoutParams.Gravity &
                (int)(~(GravityFlags.HorizontalGravityMask | GravityFlags.VerticalGravityMask)) |
                (int)(GravityFlags.Left | GravityFlags.CenterVertical);
            SupportActionBar.SetCustomView(_barView, barLayoutParams);

            SetResult(Android.App.Result.Canceled);

            _buttonRead = _barView.FindViewById<Button>(Resource.Id.buttonAdapterRead);
            _buttonRead.Click += (sender, args) =>
            {
                PerformRead();
            };

            _buttonWrite = _barView.FindViewById<Button>(Resource.Id.buttonAdapterWrite);
            _buttonWrite.Click += (sender, args) =>
            {
                PerformWrite();
            };

            _spinnerCanAdapterMode = FindViewById<Spinner>(Resource.Id.spinnerCanAdapterMode);
            _spinnerCanAdapterModeAdapter = new StringAdapter(this);
            _spinnerCanAdapterMode.Adapter = _spinnerCanAdapterModeAdapter;
            _spinnerCanAdapterModeAdapter.Items.Add(GetString(Resource.String.button_can_adapter_can_500));
            _spinnerCanAdapterModeAdapter.Items.Add(GetString(Resource.String.button_can_adapter_can_100));
            _spinnerCanAdapterModeAdapter.Items.Add(GetString(Resource.String.button_can_adapter_can_off));
            _spinnerCanAdapterModeAdapter.NotifyDataSetChanged();

            _spinnerCanAdapterSepTime = FindViewById<Spinner>(Resource.Id.spinnerCanAdapterSepTime);
            _spinnerCanAdapterSepTimeAdapter = new StringAdapter(this);
            _spinnerCanAdapterSepTime.Adapter = _spinnerCanAdapterSepTimeAdapter;
            _spinnerCanAdapterSepTimeAdapter.Items.Add(GetString(Resource.String.can_adapter_text_off));
            for (int i = 1; i <= 2; i++)
            {
                _spinnerCanAdapterSepTimeAdapter.Items.Add(i.ToString());
            }
            _spinnerCanAdapterSepTimeAdapter.NotifyDataSetChanged();

            _spinnerCanAdapterBlockSize = FindViewById<Spinner>(Resource.Id.spinnerCanAdapterBlockSize);
            _spinnerCanAdapterBlockSizeAdapter = new StringAdapter(this);
            _spinnerCanAdapterBlockSize.Adapter = _spinnerCanAdapterBlockSizeAdapter;
            _spinnerCanAdapterBlockSizeAdapter.Items.Add(GetString(Resource.String.can_adapter_text_off));
            for (int i = 0; i <= 15; i++)
            {
                _spinnerCanAdapterBlockSizeAdapter.Items.Add(i.ToString());
            }
            _spinnerCanAdapterBlockSizeAdapter.NotifyDataSetChanged();

            _textViewIgnitionState = FindViewById<TextView>(Resource.Id.textViewCanAdapterIgnitionState);

            _activityCommon = new ActivityCommon(this)
            {
                SelectedInterface = ActivityCommon.InterfaceType.Bluetooth
            };

            _deviceAddress = Intent.GetStringExtra(ExtraDeviceAddress);

            UpdateDisplay();
            PerformRead(true);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (IsJobRunning())
            {
                _adapterTask.Wait();
            }
            EdiabasClose();
        }

        public override void OnBackPressed()
        {
            if (!IsJobRunning())
            {
                base.OnBackPressed();
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void EdiabasInit()
        {
            if (_ediabas == null)
            {
                _ediabas = new EdiabasNet
                {
                    EdInterfaceClass = new EdInterfaceObd()
                };
                _activityCommon.SetEdiabasInterface(_ediabas, _deviceAddress);
            }
        }

        private bool InterfacePrepare()
        {
            if (!_ediabas.EdInterfaceClass.Connected)
            {
                if (!_ediabas.EdInterfaceClass.InterfaceConnect())
                {
                    return false;
                }
                _ediabas.EdInterfaceClass.CommParameter =
                    new UInt32[] {0x0000010F, 0x0001C200, 0x000004B0, 0x00000014, 0x0000000A, 0x00000002, 0x00001388};
                _ediabas.EdInterfaceClass.CommAnswerLen =
                    new Int16[] {0x0000, 0x0000};
            }
            return true;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool EdiabasClose()
        {
            if (IsJobRunning())
            {
                return false;
            }
            if (_ediabas != null)
            {
                _ediabas.Dispose();
                _ediabas = null;
            }
            UpdateDisplay();
            return true;
        }

        private bool IsJobRunning()
        {
            if (_adapterTask == null)
            {
                return false;
            }
            if (!_adapterTask.IsCompleted)
            {
                return true;
            }
            _adapterTask.Dispose();
            _adapterTask = null;
            return false;
        }

        private void UpdateDisplay()
        {
            bool bEnabled = !IsJobRunning();
            _buttonRead.Enabled = bEnabled;
            _buttonWrite.Enabled = bEnabled;
            _spinnerCanAdapterMode.Enabled = bEnabled;
            _spinnerCanAdapterSepTime.Enabled = bEnabled;
            _spinnerCanAdapterBlockSize.Enabled = bEnabled;
            if (bEnabled)
            {
                if ((_canMode < 0) || (_canMode >= _spinnerCanAdapterModeAdapter.Items.Count))
                {
                    _spinnerCanAdapterMode.SetSelection(0);
                }
                else
                {
                    _spinnerCanAdapterMode.SetSelection(_canMode);
                }

                if ((_separationTime < 0) || (_separationTime >= _spinnerCanAdapterSepTimeAdapter.Items.Count))
                {
                    _spinnerCanAdapterSepTime.SetSelection(0);
                }
                else
                {
                    _spinnerCanAdapterSepTime.SetSelection(_separationTime);
                }

                if ((_blockSize < 0) || (_blockSize >= _spinnerCanAdapterBlockSizeAdapter.Items.Count))
                {
                    _spinnerCanAdapterBlockSize.SetSelection(0);
                }
                else
                {
                    _spinnerCanAdapterBlockSize.SetSelection(_blockSize);
                }
                string ignitionText = string.Empty;
                if (_ignitionState >= 0)
                {
                    ignitionText = (_ignitionState & 0x01) != 0x00 ? GetString(Resource.String.can_adapter_ignition_on) : GetString(Resource.String.can_adapter_ignition_off);
                    if ((_ignitionState & 0x80) != 0)
                    {
                        ignitionText = "(" + ignitionText + ")";
                    }
                }
                _textViewIgnitionState.Text = ignitionText;
            }
        }

        private void PerformRead(bool wait = false)
        {
            EdiabasInit();
            _adapterTask = Task.Factory.StartNew(() =>
            {
                bool commFailed;
                try
                {
                    if (wait)
                    {
                        Thread.Sleep(1000); // wait for interface to close
                    }
                    commFailed = !InterfacePrepare();
                    // block size
                    if (!commFailed)
                    {
                        _blockSize = AdapterCommand(0x80);
                        if (_blockSize < 0)
                        {
                            commFailed = true;
                        }
                    }
                    // separation time
                    if (!commFailed)
                    {
                        _separationTime = AdapterCommand(0x81);
                        if (_separationTime < 0)
                        {
                            commFailed = true;
                        }
                    }
                    // CAN mode
                    if (!commFailed)
                    {
                        int mode = AdapterCommand(0x82);
                        _canMode = -1;
                        if (mode < 0)
                        {
                            commFailed = true;
                        }
                        else
                        {
                            switch (mode)
                            {
                                case 0x00: // off
                                    _canMode = 2;
                                    break;

                                case 0x01: // 500
                                    _canMode = 0;
                                    break;

                                case 0x09: // 100
                                    _canMode = 1;
                                    break;
                            }
                        }
                    }
                    // ignition state
                    if (!commFailed)
                    {
                        _ignitionState = AdapterCommand(0xFE, 0xFE);
                        if (_ignitionState < 0)
                        {
                            commFailed = true;
                        }
                    }
                }
                catch (Exception)
                {
                    commFailed = true;
                }
                if (commFailed)
                {
                    _blockSize = -1;
                    _separationTime = -1;
                    _canMode = -1;
                    _ignitionState = -1;
                }

                RunOnUiThread(() =>
                {
                    if (IsJobRunning())
                    {
                        _adapterTask.Wait();
                    }
                    UpdateDisplay();
                    if (commFailed)
                    {
                        _activityCommon.ShowAlert(GetString(Resource.String.can_adapter_comm_error));
                        EdiabasClose();
                    }
                });
            });
            UpdateDisplay();
        }

        private void PerformWrite()
        {
            EdiabasInit();

            int blockSize = _spinnerCanAdapterBlockSize.SelectedItemPosition;
            if (blockSize < 0) blockSize = 0;

            int separationTime = _spinnerCanAdapterSepTime.SelectedItemPosition;
            if (separationTime < 0) separationTime = 0;

            int canMode = _spinnerCanAdapterMode.SelectedItemPosition;
            byte mode = 0x01;
            switch (canMode)
            {
                case 0: // 500
                    mode = 0x01;
                    break;

                case 1: // 100
                    mode = 0x09;
                    break;

                case 2: // off
                    mode = 0x00;
                    break;
            }

            _adapterTask = Task.Factory.StartNew(() =>
            {
                bool commFailed;
                try
                {
                    commFailed = !InterfacePrepare();
                    // block size
                    if (!commFailed)
                    {
                        if (AdapterCommand(0x00, (byte)blockSize) < 0)
                        {
                            commFailed = true;
                        }
                    }
                    // separation time
                    if (!commFailed)
                    {
                        if (AdapterCommand(0x01, (byte)separationTime) < 0)
                        {
                            commFailed = true;
                        }
                    }
                    // CAN mode
                    if (!commFailed)
                    {
                        if (AdapterCommand(0x02, mode) < 0)
                        {
                            commFailed = true;
                        }
                    }
                }
                catch (Exception)
                {
                    commFailed = true;
                }

                RunOnUiThread(() =>
                {
                    if (IsJobRunning())
                    {
                        _adapterTask.Wait();
                    }
                    if (commFailed)
                    {
                        _activityCommon.ShowAlert(GetString(Resource.String.can_adapter_comm_error));
                        EdiabasClose();
                    }
                    else
                    {
                        PerformRead();
                    }
                });
            });
            UpdateDisplay();
        }

        private int AdapterCommand(byte command, byte data = 0x00)
        {
            byte[] response;
            if (!_ediabas.EdInterfaceClass.TransmitData(new byte[] { 0x82, 0xF1, 0xF1, command, data }, out response))
            {
                return -1;
            }
            if ((response.Length != 6) || (response[3] != command))
            {
                return -1;
            }
            return response[4];
        }
    }
}

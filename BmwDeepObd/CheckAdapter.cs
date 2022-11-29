using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Android.Bluetooth;
using Android.Content;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using EdiabasLib;

namespace BmwDeepObd;

public class CheckAdapter : IDisposable
{
    public const string ObdLinkPackageName = "OCTech.Mobile.Applications.OBDLink";

    private bool _disposed;
    private ActivityCommon _activityCommon;
    private Context _context;
    private Android.App.Activity _activity;
    private ActivityCommon.InterfaceType _interfaceType;
    private string _appDataDir;
    private string _deviceAddress;
    private AdapterTypeDetect _adapterTypeDetect;
    private EdiabasNet _ediabas;

    public CheckAdapter(ActivityCommon activityCommon, string appDataDir, ActivityCommon.InterfaceType interfaceType, string deviceAddress)
    {
        _activityCommon = activityCommon;
        _context = _activityCommon.Context;
        _activity = _activityCommon.Activity;
        _appDataDir = appDataDir;
        _interfaceType = interfaceType;
        _deviceAddress = deviceAddress;
        _adapterTypeDetect = new AdapterTypeDetect(_activityCommon);
    }

    public void StartCheckAdapter()
    {
        EdiabasInit();

        CustomProgressDialog progress = new CustomProgressDialog(_context);
        progress.SetMessage(_context.GetString(Resource.String.detect_adapter));
        progress.ButtonAbort.Visibility = ViewStates.Gone;
        progress.Show();

        _adapterTypeDetect.SbLog.Clear();

        Thread detectThread = new Thread(() =>
        {
            AdapterTypeDetect.AdapterType adapterType = AdapterTypeDetect.AdapterType.Unknown;
            try
            {
                bool connectOk = InterfacePrepare();
                Stream inStream = null;
                Stream outStream = null;
                if (connectOk)
                {
                    switch (_interfaceType)
                    {
                        case ActivityCommon.InterfaceType.Bluetooth:
                        {
                            BluetoothSocket bluetoothSocket = EdBluetoothInterface.BluetoothSocket;
                            if (bluetoothSocket == null)
                            {
                                connectOk = false;
                                break;
                            }
                            inStream = bluetoothSocket.InputStream;
                            outStream = bluetoothSocket.OutputStream;
                            break;
                        }

                        case ActivityCommon.InterfaceType.DeepObdWifi:
                        {
                            NetworkStream networkStream = EdCustomWiFiInterface.NetworkStream;
                            if (networkStream == null)
                            {
                                connectOk = false;
                                break;
                            }
                            inStream = networkStream;
                            outStream = networkStream;
                            break;
                        }
                    }
                }

                if (connectOk)
                {
                    adapterType = _adapterTypeDetect.AdapterTypeDetection(inStream, outStream);
                }
                else
                {
                    adapterType = AdapterTypeDetect.AdapterType.ConnectionFailed;
                }
            }
            catch (Exception)
            {
                adapterType = AdapterTypeDetect.AdapterType.ConnectionFailed;
            }

            if (_adapterTypeDetect.SbLog.Length == 0)
            {
                LogString("Empty log");
            }

            _activity.RunOnUiThread(() =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                progress.Dismiss();
                progress.Dispose();

                switch (adapterType)
                {
                    case AdapterTypeDetect.AdapterType.ConnectionFailed:
                        {
                            if (_activityCommon.MtcBtService)
                            {
                                AlertDialog alertDialog = new AlertDialog.Builder(_context)
                                    .SetNeutralButton(Resource.String.button_ok, (sender, args) => { })
                                    .SetCancelable(true)
                                    .SetMessage(Resource.String.adapter_connection_mtc_failed)
                                    .SetTitle(Resource.String.alert_title_error)
                                    .Show();
                                if (alertDialog != null)
                                {
                                    alertDialog.DismissEvent += (sender, args) =>
                                    {
                                        if (_activityCommon == null)
                                        {
                                            return;
                                        }
                                    };
                                }
                                break;
                            }

                            new AlertDialog.Builder(_context)
                                .SetNeutralButton(Resource.String.button_ok, (sender, args) => { })
                                .SetCancelable(true)
                                .SetMessage(Resource.String.adapter_connection_failed)
                                .SetTitle(Resource.String.alert_title_error)
                                .Show();
                            break;
                        }

                    case AdapterTypeDetect.AdapterType.Unknown:
                    {
                        bool yesSelected = false;
                            AlertDialog alertDialog2 = new AlertDialog.Builder(_context)
                                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                                {
                                    yesSelected = true;
                                })
                                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                                {
                                })
                                .SetCancelable(true)
                                .SetMessage(Resource.String.unknown_adapter_type)
                                .SetTitle(Resource.String.alert_title_error)
                                .Show();
                            if (alertDialog2 != null)
                            {
                                alertDialog2.DismissEvent += (sender, args) =>
                                {
                                    if (_activityCommon == null)
                                    {
                                        return;
                                    }
                                };
                            }
                            break;
                        }

                    case AdapterTypeDetect.AdapterType.Elm327:
                    case AdapterTypeDetect.AdapterType.Elm327Limited:
                        {
                            string message;
                            switch (adapterType)
                            {
                                case AdapterTypeDetect.AdapterType.Elm327Limited:
                                    message = string.Format(CultureInfo.InvariantCulture, _context.GetString(Resource.String.limited_elm_adapter_type), _adapterTypeDetect.ElmVerH, _adapterTypeDetect.ElmVerL);
                                    message += "<br>" + _context.GetString(Resource.String.recommened_adapter_type);
                                    break;

                                default:
                                    message = _context.GetString(Resource.String.adapter_elm_replacement);
                                    break;
                            }

                            AlertDialog alertDialog = new AlertDialog.Builder(_context)
                                .SetNeutralButton(Resource.String.button_ok, (sender, args) =>
                                {
                                })
                                .SetCancelable(true)
                                .SetMessage(ActivityCommon.FromHtml(message))
                                .SetTitle(Resource.String.alert_title_info)
                                .Show();
                            if (alertDialog != null)
                            {
                                TextView messageView = alertDialog.FindViewById<TextView>(Android.Resource.Id.Message);
                                if (messageView != null)
                                {
                                    messageView.MovementMethod = new LinkMovementMethod();
                                }
                            }
                            break;
                        }

                    case AdapterTypeDetect.AdapterType.StnFwUpdate:
                        {
                            bool yesSelected = false;
                            AlertDialog alertDialog = new AlertDialog.Builder(_context)
                                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                                {
                                    _activityCommon.StartApp(ObdLinkPackageName, true);
                                    yesSelected = true;
                                })
                                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                                {
                                })
                                .SetCancelable(true)
                                .SetMessage(Resource.String.adapter_stn_firmware)
                                .SetTitle(Resource.String.alert_title_warning)
                                .Show();
                            if (alertDialog != null)
                            {
                                alertDialog.DismissEvent += (sender, args) =>
                                {
                                    if (_activityCommon == null)
                                    {
                                        return;
                                    }

                                    if (yesSelected)
                                    {
                                        return;
                                    }
                                };
                            }
                            break;
                        }

                    case AdapterTypeDetect.AdapterType.Elm327Custom:
                        {
                            new AlertDialog.Builder(_context)
                                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                                {
                                })
                                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                                {
                                })
                                .SetCancelable(true)
                                .SetMessage(Resource.String.adapter_elm_firmware)
                                .SetTitle(Resource.String.alert_title_info)
                                .Show();
                            break;
                        }

                    case AdapterTypeDetect.AdapterType.Elm327Invalid:
                    case AdapterTypeDetect.AdapterType.Elm327Fake:
                    case AdapterTypeDetect.AdapterType.Elm327FakeOpt:
                    case AdapterTypeDetect.AdapterType.Elm327NoCan:
                        {
                            AlertDialog.Builder builder = new AlertDialog.Builder(_context);

                            string message;
                            switch (adapterType)
                            {
                                case AdapterTypeDetect.AdapterType.Elm327Fake:
                                case AdapterTypeDetect.AdapterType.Elm327FakeOpt:
                                    message = string.Format(CultureInfo.InvariantCulture, _context.GetString(Resource.String.fake_elm_adapter_type), _adapterTypeDetect.ElmVerH, _adapterTypeDetect.ElmVerL);
                                    message += "<br>" + _context.GetString(Resource.String.recommened_adapter_type);
                                    break;

                                case AdapterTypeDetect.AdapterType.Elm327NoCan:
                                    message = _context.GetString(Resource.String.elm_no_can);
                                    message += "<br>" + _context.GetString(Resource.String.adapter_elm_replacement);
                                    break;

                                default:
                                    message = _context.GetString(Resource.String.invalid_adapter_type);
                                    message += "<br>" + _context.GetString(Resource.String.recommened_adapter_type);
                                    break;
                            }

                            builder.SetNeutralButton(Resource.String.button_ok, (sender, args) => { });
                            bool isError = adapterType != AdapterTypeDetect.AdapterType.Elm327FakeOpt;
                            if (isError)
                            {
                                builder.SetTitle(Resource.String.alert_title_error);
                            }
                            else
                            {
                                builder.SetTitle(Resource.String.alert_title_warning);
                            }

                            builder.SetCancelable(true);
                            builder.SetMessage(ActivityCommon.FromHtml(message));

                            AlertDialog alertDialog = builder.Show();
                            if (alertDialog != null)
                            {
                                alertDialog.DismissEvent += (sender, args) =>
                                {
                                    if (_activityCommon == null)
                                    {
                                        return;
                                    }
                                    _activityCommon.RequestSendMessage(_appDataDir, _adapterTypeDetect.SbLog.ToString(), GetType(), (o, eventArgs) =>
                                    {
                                        TerminateCheck(isError);
                                    });
                                };

                                TextView messageView = alertDialog.FindViewById<TextView>(Android.Resource.Id.Message);
                                if (messageView != null)
                                {
                                    messageView.MovementMethod = new LinkMovementMethod();
                                }
                            }
                            break;
                        }

                    case AdapterTypeDetect.AdapterType.Custom:
                    case AdapterTypeDetect.AdapterType.CustomUpdate:
                        TerminateCheck(false);
                        break;

                    case AdapterTypeDetect.AdapterType.CustomNoEscape:
                        TerminateCheck(false);
                        break;

                    case AdapterTypeDetect.AdapterType.EchoOnly:
                        TerminateCheck(false);
                        break;
                }
            });
        })
        {
            Priority = System.Threading.ThreadPriority.Highest
        };
        detectThread.Start();
    }

    private void TerminateCheck(bool error)
    {

    }

    private void LogString(string info)
    {
        _adapterTypeDetect.LogString(info);
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

    private bool EdiabasClose()
    {
        if (_ediabas != null)
        {
            _ediabas.Dispose();
            _ediabas = null;
        }
        return true;
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
                new UInt32[] { 0x0000010F, 0x0001C200, 0x000004B0, 0x00000014, 0x0000000A, 0x00000002, 0x00001388 };
            _ediabas.EdInterfaceClass.CommAnswerLen =
                new Int16[] { 0x0000, 0x0000 };
        }
        return true;
    }

    public void Dispose()
    {
        Dispose(true);
        // This object will be cleaned up by the Dispose method.
        // Therefore, you should call GC.SupressFinalize to
        // take this object off the finalization queue
        // and prevent finalization code for this object
        // from executing a second time.
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        // Check to see if Dispose has already been called.
        if (!_disposed)
        {
            // If disposing equals true, dispose all managed
            // and unmanaged resources.
            if (disposing)
            {
                // Dispose managed resources.
                EdiabasClose();
            }

            // Note disposing has been done.
            _disposed = true;
        }
    }

}

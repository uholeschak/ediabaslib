using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Google.Apis.AndroidPublisher.v2;
using Google.Apis.AndroidPublisher.v2.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace ApkUploader
{
    public partial class FormMain : Form
    {
        private const string PackageName = @"de.holeschak.bmw_deep_obd";
        private volatile Thread _serviceThread;
        private readonly string _apkPath;
        private CancellationTokenSource _cts;

        public FormMain()
        {
            InitializeComponent();
            _apkPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @".apk");
        }

        private void UpdateStatus(string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => { UpdateStatus(message); }));
                return;
            }

            textBoxStatus.Text = message;
            bool enable = _serviceThread == null;
            buttonListApks.Enabled = enable;
            buttonAbort.Enabled = !enable;
            buttonClose.Enabled = enable;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool ListApks()
        {
            if (_serviceThread != null)
            {
                return false;
            }
            _cts = new CancellationTokenSource();
            _serviceThread = new Thread(async () =>
                {
                    StringBuilder sb = new StringBuilder();
                    try
                    {
                        UpdateStatus(sb.ToString());

                        UserCredential credential;
                        using (var stream = new FileStream(Path.Combine(_apkPath, "client_secrets.json"), FileMode.Open, FileAccess.Read))
                        {
                            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                                GoogleClientSecrets.Load(stream).Secrets,
                                new[] { AndroidPublisherService.Scope.Androidpublisher },
                                "ApkUploader", _cts.Token, new FileDataStore("ApkUploader"));
                        }

                        BaseClientService.Initializer initializer =
                            new BaseClientService.Initializer
                            {
                                HttpClientInitializer = credential,
                                ApplicationName = PackageName
                            };
                        using (AndroidPublisherService service = new AndroidPublisherService(initializer))
                        {
                            EditsResource edits = service.Edits;
                            EditsResource.InsertRequest editRequest = edits.Insert(null, PackageName);
                            AppEdit appEdit = await editRequest.ExecuteAsync(_cts.Token);
                            ApksListResponse apksResponse = await edits.Apks.List(PackageName, appEdit.Id).ExecuteAsync(_cts.Token);

                            sb.AppendLine("Apks:");
                            foreach (Apk apk in apksResponse.Apks)
                            {
                                sb.AppendLine($"Version: {apk.VersionCode}, SHA1: {apk.Binary.Sha1}");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        sb.AppendLine($"Exception: {e.Message}");
                    }
                    finally
                    {
                        _serviceThread = null;
                        _cts.Dispose();
                        UpdateStatus(sb.ToString());
                    }
                });
            _serviceThread.Start();

            return true;
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!buttonClose.Enabled)
            {
                e.Cancel = true;
            }
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            UpdateStatus(string.Empty);
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonListApks_Click(object sender, EventArgs e)
        {
            ListApks();
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            _cts?.Dispose();
        }

        private void buttonAbort_Click(object sender, EventArgs e)
        {
            _cts.Cancel();
        }
    }
}

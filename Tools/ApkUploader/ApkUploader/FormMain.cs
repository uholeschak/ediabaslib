using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Apis.AndroidPublisher;
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
        private string _assemblyPath;

        public FormMain()
        {
            InitializeComponent();
            _assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
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
            buttonClose.Enabled = enable;
            buttonListApks.Enabled = enable;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool ListApks()
        {
            if (_serviceThread != null)
            {
                return false;
            }
            _serviceThread = new Thread(async () =>
                {
                    StringBuilder sb = new StringBuilder();
                    try
                    {
                        UpdateStatus(sb.ToString());

                        UserCredential credential;
                        using (var stream = new FileStream(Path.Combine(_assemblyPath, "client_secrets.json"), FileMode.Open, FileAccess.Read))
                        {
                            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                                GoogleClientSecrets.Load(stream).Secrets,
                                new[] { AndroidPublisherService.Scope.Androidpublisher },
                                "user", CancellationToken.None, new FileDataStore("ApkUploader"));
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
                            AppEdit appEdit = editRequest.Execute();
                            ApksListResponse apksResponse = edits.Apks.List(PackageName, appEdit.Id).Execute();

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
    }
}

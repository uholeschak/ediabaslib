using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Apis.AndroidPublisher.v2;
using Google.Apis.AndroidPublisher.v2.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;

namespace ApkUploader
{
    public partial class FormMain : Form
    {
        private const string PackageName = @"de.holeschak.bmw_deep_obd";
        private static readonly string[] Tracks = { "alpha", "beta", "production", "rollout" };
        private volatile Thread _serviceThread;
        private readonly string _apkPath;
        private CancellationTokenSource _cts;

        private class ExpansionInfo
        {
            public ExpansionInfo(int apkVersion, int expansionVersion, long fileSize)
            {
                ApkVersion = apkVersion;
                ExpansionVersion = expansionVersion;
                FileSize = fileSize;
            }

            public int ApkVersion { get;}
            public int ExpansionVersion { get; }
            public long FileSize { get; }
        }

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
            textBoxStatus.SelectionStart = textBoxStatus.TextLength;
            textBoxStatus.Update();
            textBoxStatus.ScrollToCaret();

            bool enable = _serviceThread == null;
            buttonListApks.Enabled = enable;
            buttonListTracks.Enabled = enable;
            buttonUploadApk.Enabled = enable;
            buttonClose.Enabled = enable;
            checkBoxAlpha.Enabled = enable;
            textBoxApkFile.Enabled = enable;
            textBoxObbFile.Enabled = enable;
            buttonSelectApk.Enabled = enable;
            buttonSelectObb.Enabled = enable;

            buttonAbort.Enabled = !enable;
        }

        private async Task<UserCredential> GetCredatials()
        {
            UserCredential credential;
            using (var stream = new FileStream(Path.Combine(_apkPath, "client_secrets.json"), FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { AndroidPublisherService.Scope.Androidpublisher },
                    "ApkUploader", _cts.Token, new FileDataStore("ApkUploader"));
            }

            return credential;
        }

        private static BaseClientService.Initializer GetInitializer(UserCredential credential)
        {
            return new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = PackageName
            };
        }

        private async Task PrintExpansion(StringBuilder sb, EditsResource edits, AppEdit appEdit, int version)
        {
            try
            {
                ExpansionFile expansionResponse = await edits.Expansionfiles.Get(PackageName, appEdit.Id, version, EditsResource.ExpansionfilesResource.GetRequest.ExpansionFileTypeEnum.Main).ExecuteAsync(_cts.Token);
                if ((expansionResponse.FileSize ?? 0) > 0)
                {
                    sb.Append($"Expansion size: {expansionResponse.FileSize}");
                    if (expansionResponse.ReferencesVersion != null)
                    {
                        sb.AppendLine($"ref ver: {expansionResponse.ReferencesVersion}");
                    }
                    sb.AppendLine();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private async Task<ExpansionInfo> GetNewestExpansionFile(EditsResource edits, AppEdit appEdit)
        {
            try
            {
                ApksListResponse apksResponse = await edits.Apks.List(PackageName, appEdit.Id).ExecuteAsync(_cts.Token);
                int apkVersion = -1;
                int expansionVersion = -1;
                long fileSize = 0;
                foreach (Apk apk in apksResponse.Apks)
                {
                    // ReSharper disable once UseNullPropagation
                    if (apk.VersionCode != null)
                    {
                        if (apk.VersionCode.Value > apkVersion)
                        {
                            try
                            {
                                ExpansionFile expansionResponse = await edits.Expansionfiles.Get(PackageName, appEdit.Id, apk.VersionCode.Value, EditsResource.ExpansionfilesResource.GetRequest.ExpansionFileTypeEnum.Main).ExecuteAsync(_cts.Token);
                                if (expansionResponse.FileSize != null)
                                {
                                    apkVersion = apk.VersionCode.Value;
                                    expansionVersion = apkVersion;
                                    fileSize = expansionResponse.FileSize.Value;
                                    if (expansionResponse.ReferencesVersion != null)
                                    {
                                        expansionVersion = expansionResponse.ReferencesVersion.Value;
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }
                }
                if (apkVersion < 0)
                {
                    return null;
                }
                return new ExpansionInfo(apkVersion, expansionVersion, fileSize);
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool ListApks()
        {
            if (_serviceThread != null)
            {
                return false;
            }
            UpdateStatus(string.Empty);
            _cts = new CancellationTokenSource();
            _serviceThread = new Thread(async () =>
                {
                    UpdateStatus(string.Empty);
                    StringBuilder sb = new StringBuilder();
                    try
                    {
                        UserCredential credential = await GetCredatials();
                        using (AndroidPublisherService service = new AndroidPublisherService(GetInitializer(credential)))
                        {
                            EditsResource edits = service.Edits;
                            EditsResource.InsertRequest editRequest = edits.Insert(null, PackageName);
                            AppEdit appEdit = await editRequest.ExecuteAsync(_cts.Token);
                            ApksListResponse apksResponse = await edits.Apks.List(PackageName, appEdit.Id).ExecuteAsync(_cts.Token);

                            sb.AppendLine("Apks:");
                            foreach (Apk apk in apksResponse.Apks)
                            {
                                if (apk.VersionCode != null)
                                {
                                    sb.AppendLine($"Version: {apk.VersionCode}, SHA1: {apk.Binary.Sha1}");
                                    await PrintExpansion(sb, edits, appEdit, apk.VersionCode.Value);
                                }
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

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool ListTracks()
        {
            if (_serviceThread != null)
            {
                return false;
            }
            UpdateStatus(string.Empty);
            _cts = new CancellationTokenSource();
            _serviceThread = new Thread(async () =>
            {
                UpdateStatus(string.Empty);
                StringBuilder sb = new StringBuilder();
                try
                {
                    UserCredential credential = await GetCredatials();
                    using (AndroidPublisherService service = new AndroidPublisherService(GetInitializer(credential)))
                    {
                        EditsResource edits = service.Edits;
                        EditsResource.InsertRequest editRequest = edits.Insert(null, PackageName);
                        AppEdit appEdit = await editRequest.ExecuteAsync(_cts.Token);
                        foreach (string track in Tracks)
                        {
                            sb.AppendLine($"Track: {track}");
                            try
                            {
                                EditsResource.TracksResource.GetRequest getRequest = edits.Tracks.Get(PackageName, appEdit.Id, track);
                                Track trackResponse = await getRequest.ExecuteAsync(_cts.Token);

                                foreach (int? version in trackResponse.VersionCodes)
                                {
                                    if (version != null)
                                    {
                                        sb.AppendLine($"Version: {version.Value}");
                                        await PrintExpansion(sb, edits, appEdit, version.Value);
                                    }
                                    else
                                    {
                                        sb.AppendLine("No version");
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                sb.AppendLine("No data");
                            }
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

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool UploadApk(string apkFileName, string expansionFileName, string track, List<Tuple<string, string>> listings)
        {
            if (_serviceThread != null)
            {
                return false;
            }

            if (!File.Exists(apkFileName))
            {
                UpdateStatus("Apk file not existing");
                return false;
            }

            if (!string.IsNullOrEmpty(expansionFileName) && !File.Exists(expansionFileName))
            {
                UpdateStatus("Expansion file not existing");
                return false;
            }

            UpdateStatus(string.Empty);
            _cts = new CancellationTokenSource();
            _serviceThread = new Thread(async () =>
            {
                UpdateStatus(string.Empty);
                StringBuilder sb = new StringBuilder();
                try
                {
                    UserCredential credential = await GetCredatials();
                    using (AndroidPublisherService service = new AndroidPublisherService(GetInitializer(credential)))
                    {
                        EditsResource edits = service.Edits;
                        EditsResource.InsertRequest editRequest = edits.Insert(null, PackageName);
                        AppEdit appEdit = await editRequest.ExecuteAsync(_cts.Token);

                        bool reuseExpansion = false;
                        ExpansionInfo expansionInfo = await GetNewestExpansionFile(edits, appEdit);
                        if (expansionInfo != null)
                        {
                            sb.AppendLine($"Latest expansion: apk version={expansionInfo.ApkVersion}, expansion version={expansionInfo.ExpansionVersion}, size={expansionInfo.FileSize}");
                            if (!string.IsNullOrEmpty(expansionFileName))
                            {
                                FileInfo fileInfo = new FileInfo(expansionFileName);
                                if (fileInfo.Exists && fileInfo.Length == expansionInfo.FileSize)
                                {
                                    sb.AppendLine("Size unchanged, reusing old expansion file");
                                    reuseExpansion = true;
                                }
                            }
                        }

                        Apk apkUploaded = null;
                        using (FileStream apkStream = new FileStream(apkFileName, FileMode.Open, FileAccess.Read))
                        {
                            long fileLength = (apkStream.Length > 0) ? apkStream.Length : 1;
                            EditsResource.ApksResource.UploadMediaUpload upload = edits.Apks.Upload(PackageName, appEdit.Id, apkStream, "application/vnd.android.package-archive");
                            upload.ChunkSize = ResumableUpload.MinimumChunkSize;
                            upload.ProgressChanged += progress =>
                            {
                                UpdateStatus(sb.ToString() + $"Apk progress: {100 * progress.BytesSent / fileLength}%");
                            };
                            upload.ResponseReceived += apk =>
                            {
                                apkUploaded = apk;
                            };
                            IUploadProgress uploadProgress = await upload.UploadAsync(_cts.Token);
                            sb.AppendLine($"Upload status: {uploadProgress.Status.ToString()}");
                            UpdateStatus(sb.ToString());
                            if (uploadProgress.Exception != null)
                            {
                                throw uploadProgress.Exception;
                            }
                        }

                        int? versionCode = apkUploaded?.VersionCode;
                        if (!versionCode.HasValue)
                        {
                            throw new Exception("No apk version code");
                        }
                        sb.AppendLine($"Version code uploaded: {versionCode.Value}");
                        UpdateStatus(sb.ToString());

                        if (!string.IsNullOrEmpty(expansionFileName) && !reuseExpansion)
                        {
                            using (FileStream expansionStream = new FileStream(expansionFileName, FileMode.Open, FileAccess.Read))
                            {
                                long fileLength = (expansionStream.Length > 0) ? expansionStream.Length : 1;
                                EditsResource.ExpansionfilesResource.UploadMediaUpload upload = edits.Expansionfiles.Upload(PackageName, appEdit.Id, versionCode.Value,
                                     EditsResource.ExpansionfilesResource.UploadMediaUpload.ExpansionFileTypeEnum.Main, expansionStream, "application/octet-stream");
                                upload.ChunkSize = ResumableUpload.MinimumChunkSize;
                                upload.ProgressChanged += progress =>
                                {
                                    UpdateStatus(sb.ToString() + $"Expansion progress: {100 * progress.BytesSent / fileLength}%");
                                };
                                IUploadProgress uploadProgress = await upload.UploadAsync(_cts.Token);
                                sb.AppendLine($"Upload status: {uploadProgress.Status.ToString()}");
                                UpdateStatus(sb.ToString());
                                if (uploadProgress.Exception != null)
                                {
                                    throw uploadProgress.Exception;
                                }
                            }
                        }
                        else
                        {
                            if (expansionInfo != null)
                            {
                                ExpansionFile expansionRef = new ExpansionFile { ReferencesVersion = expansionInfo.ExpansionVersion };
                                await edits.Expansionfiles.Update(expansionRef, PackageName, appEdit.Id, versionCode.Value,
                                    EditsResource.ExpansionfilesResource.UpdateRequest.ExpansionFileTypeEnum.Main).ExecuteAsync(_cts.Token);
                                sb.AppendLine($"Expansion version {expansionInfo.ExpansionVersion} assigned");
                            }
                            else
                            {
                                sb.AppendLine("No existing expansion found!");
                            }
                        }

                        Track updateTrack = new Track { VersionCodes = new List<int?> { versionCode.Value } };
                        EditsResource.TracksResource.UpdateRequest updateRequest = edits.Tracks.Update(updateTrack, PackageName, appEdit.Id, track);
                        Track updatedTrack = await updateRequest.ExecuteAsync(_cts.Token);
                        sb.AppendLine($"Track updated: {updatedTrack.TrackValue}");
                        UpdateStatus(sb.ToString());

                        if (listings != null)
                        {
                            foreach (Tuple<string, string> listing in listings)
                            {
                                string language = listing.Item1;
                                string changes = listing.Item1;
                                ApkListing apkListing = new ApkListing
                                {
                                    RecentChanges = changes
                                };
                                await edits.Apklistings.Update(apkListing, PackageName, appEdit.Id, versionCode.Value, language).ExecuteAsync(_cts.Token);
                                sb.AppendLine($"Changes for language {language} updated");
                                UpdateStatus(sb.ToString());
                            }
                        }

                        EditsResource.CommitRequest commitRequest = edits.Commit(PackageName, appEdit.Id);
                        AppEdit appEditCommit = await commitRequest.ExecuteAsync(_cts.Token);
                        sb.AppendLine($"App edit committed: {appEditCommit.Id}");
                        UpdateStatus(sb.ToString());
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

        private void FormMain_Load(object sender, EventArgs e)
        {
            checkBoxAlpha.Checked = Properties.Settings.Default.Alpha;
            textBoxApkFile.Text = Properties.Settings.Default.ApkFileName;
            textBoxObbFile.Text = Properties.Settings.Default.ObbFileName;
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

        private void buttonListTracks_Click(object sender, EventArgs e)
        {
            ListTracks();
        }

        private void buttonUploadApk_Click(object sender, EventArgs e)
        {
            List<Tuple<string, string>> apkChanges = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("en-US", "- ECU files updated\n- Using expansion files\n- Fixed minor problems"),
                new Tuple<string, string>("de-DE", "- ECU Dateien aktualisiert\n- Erweiterungsdateien werden verwendet\n- Kleinere Probleme behoben")
            };

            UploadApk(textBoxApkFile.Text, textBoxObbFile.Text, checkBoxAlpha.Checked ? "alpha" : "beta", apkChanges);
        }

        private void buttonSelectApk_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxApkFile.Text))
            {
                openFileDialogApk.FileName = textBoxApkFile.Text;
                openFileDialogApk.InitialDirectory = Path.GetDirectoryName(textBoxApkFile.Text);
            }
            if (openFileDialogApk.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            textBoxApkFile.Text = openFileDialogApk.FileName;
        }

        private void buttonSelectObb_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxObbFile.Text))
            {
                openFileDialogObb.FileName = textBoxObbFile.Text;
                openFileDialogObb.InitialDirectory = Path.GetDirectoryName(textBoxObbFile.Text);
            }
            if (openFileDialogObb.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            textBoxObbFile.Text = openFileDialogObb.FileName;
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.Alpha = checkBoxAlpha.Checked;
                Properties.Settings.Default.ApkFileName = textBoxApkFile.Text;
                Properties.Settings.Default.ObbFileName = textBoxObbFile.Text;
                Properties.Settings.Default.Save();
            }
            catch (Exception)
            {
                // ignored
            }

            _cts?.Dispose();
        }

        private void buttonAbort_Click(object sender, EventArgs e)
        {
            _cts.Cancel();
        }
    }
}

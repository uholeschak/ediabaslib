using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.AndroidPublisher.v3.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;

namespace ApkUploader
{
    public partial class FormMain : Form
    {
        private const string StatusCompleted = "completed";
        private const string PackageName = @"de.holeschak.bmw_deep_obd";
        private const string ExpansionKeep = @"*";
        private static readonly string[] TracksEdit = { "alpha", "beta", "production", "internal" };
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

        private class UpdateInfo
        {
            public UpdateInfo(string language, string changes)
            {
                Language = language;
                Changes = changes;
            }

            public string Language { get; }
            public string Changes { get; }
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
            buttonUpdateChanges.Enabled = enable;
            buttonUploadApk.Enabled = enable;
            buttonChangeTrack.Enabled = enable;
            buttonAssignTrack.Enabled = enable;
            buttonSetAppInfo.Enabled = enable;
            buttonClose.Enabled = enable;
            comboBoxTrackAssign.Enabled = enable;
            comboBoxTrackUnassign.Enabled = enable;
            checkBoxUpdateName.Enabled = enable;
            textBoxVersion.Enabled = enable;
            textBoxApkFile.Enabled = enable;
            textBoxObbFile.Enabled = enable;
            textBoxResourceFolder.Enabled = enable;
            buttonSelectApk.Enabled = enable;
            buttonSelectObb.Enabled = enable;
            buttonSelectResourceFolder.Enabled = enable;

            buttonAbort.Enabled = !enable;
        }

        private List<UpdateInfo> ReadUpdateInfo(string resourceDir)
        {
            try
            {
                Regex regex = new Regex(@"^values(|-[a-z]{0,2})$", RegexOptions.IgnoreCase);
                List<UpdateInfo> updateInfos = new List<UpdateInfo>();

                string[] files = Directory.GetFiles(resourceDir, "Strings.xml", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    string parentName = Directory.GetParent(file).Name;
                    MatchCollection matchesFile = regex.Matches(parentName);
                    if ((matchesFile.Count == 1) && (matchesFile[0].Groups.Count == 2))
                    {
                        string language = matchesFile[0].Groups[1].Value;
                        if (language.Length > 1)
                        {
                            language = language.Substring(1);
                            language = language.ToLowerInvariant() + "-" + language.ToUpperInvariant();

                        }
                        else
                        {
                            language = @"en-US";
                        }

                        string changes = string.Empty;
                        try
                        {
                            XDocument xmlDoc = XDocument.Load(file);
                            if (xmlDoc.Root == null)
                            {
                                continue;
                            }

                            XElement[] stringNodes = xmlDoc.Root.Elements("string").ToArray();
                            foreach (XElement stringNode in stringNodes)
                            {
                                XAttribute nameAttr = stringNode.Attribute("name");
                                if (nameAttr == null)
                                {
                                    continue;
                                }

                                if (string.Compare(nameAttr.Value, "version_last_changes", StringComparison.OrdinalIgnoreCase) != 0)
                                {
                                    continue;
                                }

                                using (XmlReader reader = stringNode.CreateReader())
                                {
                                    reader.MoveToContent();
                                    changes = reader.ReadInnerXml();
                                }
                                changes = changes.Replace("\\n", "\n");
                                break;
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(changes))
                        {
                            continue;
                        }

                        updateInfos.Add(new UpdateInfo(language, changes));
                    }
                }

                return updateInfos;
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        private string ReadAppVersion(string resourceDir, out int? versionCode)
        {
            versionCode = null;
            try
            {
                string parentDir = Directory.GetParent(resourceDir).FullName;
                string propertiesDir = Path.Combine(parentDir, "Properties");
                string manifestFile = Path.Combine(propertiesDir, "AndroidManifest.xml");
                if (!File.Exists(manifestFile))
                {
                    return string.Empty;
                }

                XNamespace android = XNamespace.Get("http://schemas.android.com/apk/res/android");
                XDocument xmlDoc = XDocument.Load(manifestFile);
                if (xmlDoc.Root == null)
                {
                    return string.Empty;
                }

                XAttribute verCodeAttr = xmlDoc.Root.Attribute(android + "versionCode");
                if (verCodeAttr != null)
                {
                    if (Int32.TryParse(verCodeAttr.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
                    {
                        versionCode = value;
                    }
                }

                XAttribute verNameAttr = xmlDoc.Root.Attribute(android + "versionName");
                if (verNameAttr == null)
                {
                    return string.Empty;
                }

                return verNameAttr.Value;
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        private bool ReadAppInfoCredentials(out string url, out string userName, out string password)
        {
            url = null;
            userName = null;
            password = null;
            try
            {
                string xmlFile = Path.Combine(_apkPath, "appinfo_credentials.xml");
                if (!File.Exists(xmlFile))
                {
                    return false;
                }

                XDocument xmlDoc = XDocument.Load(xmlFile);
                XElement credentialsNode = xmlDoc.Root?.Element("credentials");
                if (credentialsNode != null)
                {
                    XAttribute urlAttr = credentialsNode.Attribute("url");
                    if (string.IsNullOrEmpty(urlAttr?.Value))
                    {
                        return false;
                    }
                    url = urlAttr.Value;

                    XAttribute nameAttr = credentialsNode.Attribute("name");
                    if (string.IsNullOrEmpty(nameAttr?.Value))
                    {
                        return false;
                    }
                    userName = nameAttr.Value;

                    XAttribute passwordAttr = credentialsNode.Attribute("password");
                    if (string.IsNullOrEmpty(passwordAttr?.Value))
                    {
                        return false;
                    }
                    password = passwordAttr.Value;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
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
                if (expansionResponse.ReferencesVersion != null)
                {
                    sb.AppendLine($"Expansion ref ver: {expansionResponse.ReferencesVersion.Value}");
                    ExpansionFile expansionRefResponse = await edits.Expansionfiles.Get(PackageName, appEdit.Id, expansionResponse.ReferencesVersion.Value, EditsResource.ExpansionfilesResource.GetRequest.ExpansionFileTypeEnum.Main).ExecuteAsync(_cts.Token);
                    if (expansionRefResponse.FileSize != null && expansionRefResponse.FileSize.Value > 0)
                    {
                        sb.AppendLine($"Expansion size: {expansionRefResponse.FileSize.Value}");
                    }
                }
                else if ((expansionResponse.FileSize != null && expansionResponse.FileSize.Value > 0))
                {
                    sb.AppendLine($"Expansion size: {expansionResponse.FileSize.Value}");
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
                                if (expansionResponse.ReferencesVersion != null)
                                {
                                    expansionVersion = expansionResponse.ReferencesVersion.Value;
                                    try
                                    {
                                        ExpansionFile expansionRefResponse = await edits.Expansionfiles.Get(PackageName, appEdit.Id, expansionResponse.ReferencesVersion.Value, EditsResource.ExpansionfilesResource.GetRequest.ExpansionFileTypeEnum.Main).ExecuteAsync(_cts.Token);
                                        if (expansionRefResponse.FileSize != null && expansionRefResponse.FileSize.Value > 0)
                                        {
                                            apkVersion = apk.VersionCode.Value;
                                            expansionVersion = expansionResponse.ReferencesVersion.Value;
                                            fileSize = expansionRefResponse.FileSize.Value;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        apkVersion = apk.VersionCode.Value;
                                        expansionVersion = expansionResponse.ReferencesVersion.Value;
                                        fileSize = 0;
                                    }
                                }
                                else if (expansionResponse.FileSize != null && expansionResponse.FileSize.Value > 0)
                                {
                                    apkVersion = apk.VersionCode.Value;
                                    expansionVersion = apkVersion;
                                    fileSize = expansionResponse.FileSize.Value;
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
        private bool UpdateAppInfo(StringBuilder sb, long versionCode, string appVersion, string track, List<UpdateInfo> apkChanges = null)
        {
            try
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }
                sb.AppendLine("Updating app info");
                UpdateStatus(sb.ToString());

                if (!ReadAppInfoCredentials(out string url, out string userName, out string password))
                {
                    UpdateStatus("Reading app info credentials failed!");
                    return false;
                }

                sb.AppendLine($"Version code: {versionCode}");
                UpdateStatus(sb.ToString());

                if (!string.IsNullOrEmpty(appVersion))
                {
                    sb.AppendLine($"Version name: {appVersion}");
                    UpdateStatus(sb.ToString());
                }

                if (!string.IsNullOrEmpty(track))
                {
                    sb.AppendLine($"Track: {track}");
                    UpdateStatus(sb.ToString());
                }

                if (apkChanges != null)
                {
                    sb.Append("Changes info for languages present: ");
                    foreach (UpdateInfo updateInfo in apkChanges)
                    {
                        sb.Append($"{updateInfo.Language} ");
                    }
                    sb.AppendLine();
                    UpdateStatus(sb.ToString());
                }

                using (HttpClientHandler httpClientHandler = new HttpClientHandler())
                {
                    httpClientHandler.Credentials = new NetworkCredential(userName, password);
                    using (HttpClient httpClient = new HttpClient(httpClientHandler))
                    {
                        // ReSharper disable once UseObjectOrCollectionInitializer
                        MultipartFormDataContent formAppInfo = new MultipartFormDataContent();

                        formAppInfo.Add(new StringContent(PackageName), "package_name");
                        formAppInfo.Add(new StringContent(string.Format(CultureInfo.InvariantCulture, "{0}", versionCode)), "app_ver");
                        if (!string.IsNullOrEmpty(appVersion))
                        {
                            formAppInfo.Add(new StringContent(appVersion), "app_ver_name");
                        }
                        formAppInfo.Add(new StringContent(track), "track");

                        if (apkChanges != null)
                        {
                            foreach (UpdateInfo updateInfo in apkChanges)
                            {
                                if (updateInfo.Language.Length >= 2)
                                {
                                    string lang = updateInfo.Language.Substring(0, 2).ToLowerInvariant();
                                    formAppInfo.Add(new StringContent(updateInfo.Changes), "info_" + lang);
                                }
                            }
                        }

                        HttpResponseMessage responseAppInfo = httpClient.PostAsync(url, formAppInfo, _cts.Token).Result;
                        responseAppInfo.EnsureSuccessStatusCode();
                        string responseAppInfoXml = responseAppInfo.Content.ReadAsStringAsync().Result;

                        try
                        {
                            XDocument xmlDoc = XDocument.Parse(responseAppInfoXml);
                            if (xmlDoc.Root == null)
                            {
                                throw new Exception("XML invalid");
                            }

                            bool valid = false;

                            XElement statusNode = xmlDoc.Root?.Element("status");
                            if (statusNode != null)
                            {
                                XAttribute okAttr = statusNode.Attribute("ok");
                                if (string.Compare(okAttr?.Value ?? string.Empty, "true", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    valid = true;
                                }
                                else
                                {
                                    sb.AppendLine("Invalid status");
                                }
                            }

                            XElement errorNode = xmlDoc.Root?.Element("error");
                            if (errorNode != null)
                            {
                                valid = false;
                                XAttribute messageAttr = errorNode.Attribute("message");
                                if (string.IsNullOrEmpty(messageAttr?.Value))
                                {
                                    sb.AppendLine($"Error: {messageAttr?.Value}");
                                }
                            }

                            if (valid)
                            {
                                sb.AppendLine("App info updated");
                            }
                        }
                        catch (Exception)
                        {
                            sb.AppendLine("Response invalid:");
                            sb.AppendLine(responseAppInfoXml);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                sb.AppendLine($"Exception: {e.Message}");
                return false;
            }

            return true;
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
                                    sb.AppendLine($"Version: {apk.VersionCode.Value}, SHA1: {apk.Binary.Sha1}");
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

                        TracksListResponse tracksListResponse = await edits.Tracks.List(PackageName, appEdit.Id).ExecuteAsync(_cts.Token);
                        if (tracksListResponse.Tracks != null)
                        {
                            foreach (Track track in tracksListResponse.Tracks)
                            {
                                if (sb.Length > 0)
                                {
                                    sb.AppendLine();
                                }
                                sb.AppendLine($"Track: {track.TrackValue}");
                                foreach (TrackRelease trackRelease in track.Releases)
                                {
                                    if (trackRelease != null)
                                    {
                                        if (trackRelease.Name != null)
                                        {
                                            sb.AppendLine($"Name: {trackRelease.Name}");
                                        }
                                        if (trackRelease.Status != null)
                                        {
                                            sb.AppendLine($"Status: {trackRelease.Status}");
                                        }

                                        if (trackRelease.ReleaseNotes != null)
                                        {
                                            foreach (LocalizedText localizedText in trackRelease.ReleaseNotes)
                                            {
                                                if (localizedText != null)
                                                {
                                                    sb.AppendLine($"Note ({localizedText.Language}): {localizedText.Text}");
                                                }
                                            }
                                        }

                                        if (trackRelease.VersionCodes != null)
                                        {
                                            foreach (long? version in trackRelease.VersionCodes)
                                            {
                                                if (version.HasValue)
                                                {
                                                    sb.AppendLine($"Version: {version.Value}");
                                                    await PrintExpansion(sb, edits, appEdit, (int)version.Value);
                                                }
                                            }
                                        }

                                        try
                                        {
                                            Testers testers = await edits.Testers.Get(PackageName, appEdit.Id, track.TrackValue).ExecuteAsync(_cts.Token);
                                            sb.AppendLine("Test active");
                                            if (testers.GoogleGroups != null)
                                            {
                                                foreach (string group in testers.GoogleGroups)
                                                {
                                                    sb.AppendLine($"Tester group: {group}");
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
        private bool ChangeTrack(string fromTrack, string toTrack)
        {
            if (_serviceThread != null)
            {
                return false;
            }
            UpdateStatus(string.Empty);
            if (string.Compare(fromTrack, toTrack, StringComparison.OrdinalIgnoreCase) == 0)
            {
                UpdateStatus("Both tracks identical");
                return false;
            }
            if (string.IsNullOrWhiteSpace(fromTrack) || string.IsNullOrWhiteSpace(toTrack))
            {
                UpdateStatus("Empty track");
                return false;
            }
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
                        Track trackResponse = await edits.Tracks.Get(PackageName, appEdit.Id, fromTrack).ExecuteAsync(_cts.Token);
                        sb.AppendLine($"From track: {fromTrack}");
                        if (trackResponse.Releases.Count != 1)
                        {
                            sb.AppendLine($"Invalid release count: {trackResponse.Releases.Count}");
                            UpdateStatus(sb.ToString());
                            throw new Exception("Invalid release count");
                        }
                        TrackRelease trackRelease = trackResponse.Releases[0];
                        if (trackRelease.VersionCodes == null)
                        {
                            sb.AppendLine("No version codes present");
                            UpdateStatus(sb.ToString());
                            throw new Exception("Invalid versions");
                        }
                        if (trackRelease.VersionCodes.Count != 1 || !trackRelease.VersionCodes[0].HasValue)
                        {
                            sb.AppendLine($"Invalid version count: {trackRelease.VersionCodes.Count}");
                            UpdateStatus(sb.ToString());
                            throw new Exception("Invalid version count");
                        }
                        long currentVersion = trackRelease.VersionCodes[0].Value;
                        sb.AppendLine($"Version: {currentVersion}");
                        UpdateStatus(sb.ToString());

                        Track assignTrack = new Track
                        {
                            TrackValue = toTrack,
                            Releases = new List<TrackRelease>
                            {
                                new TrackRelease
                                {
                                    Name = trackRelease.Name,
                                    VersionCodes = new List<long?>
                                    {
                                        currentVersion
                                    },
                                    Status = trackRelease.Status,
                                    ReleaseNotes = trackRelease.ReleaseNotes,
                                }
                            }
                        };

                        await edits.Tracks.Update(assignTrack, PackageName, appEdit.Id, toTrack).ExecuteAsync();
                        sb.AppendLine($"Assigned to track: {toTrack}");
                        UpdateStatus(sb.ToString());

                        Track unassignTrack = new Track
                        {
                            TrackValue = fromTrack,
                            Releases = new List<TrackRelease>
                            {
                                new TrackRelease
                                {
                                    Status = StatusCompleted
                                }
                            }
                        };
                        await edits.Tracks.Update(unassignTrack, PackageName, appEdit.Id, fromTrack).ExecuteAsync();
                        sb.AppendLine($"Unassigned from track: {fromTrack}");
                        UpdateStatus(sb.ToString());

                        EditsResource.CommitRequest commitRequest = edits.Commit(PackageName, appEdit.Id);
                        AppEdit appEditCommit = await commitRequest.ExecuteAsync(_cts.Token);
                        sb.AppendLine($"App edit committed: {appEditCommit.Id}");
                        UpdateStatus(sb.ToString());

                        UpdateAppInfo(sb, currentVersion, null, toTrack);
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
        private bool AssignTrack(string track, int? versionAssign)
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
                        TrackRelease trackReleaseOld = null;
                        try
                        {
                            Track trackResponse = await edits.Tracks.Get(PackageName, appEdit.Id, track).ExecuteAsync(_cts.Token);
                            sb.AppendLine($"Track: {track}");

                            foreach (TrackRelease trackRelease in trackResponse.Releases)
                            {
                                if (trackRelease != null)
                                {
                                    if (trackReleaseOld == null)
                                    {
                                        trackReleaseOld = trackRelease;
                                    }

                                    if (trackRelease.Name != null)
                                    {
                                        sb.AppendLine($"Name: {trackRelease.Name}");
                                    }

                                    if (trackRelease.Status != null)
                                    {
                                        sb.AppendLine($"Status: {trackRelease.Status}");
                                    }

                                    foreach (long? version in trackRelease.VersionCodes)
                                    {
                                        if (version.HasValue)
                                        {
                                            sb.AppendLine($"Version: {version.Value}");
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            sb.AppendLine($"No version for track: {track}");
                        }

                        Track assignTrack = new Track
                        {
                            TrackValue = track
                        };
                        if (versionAssign.HasValue)
                        {
                            assignTrack.Releases = new List<TrackRelease>
                            {
                                new TrackRelease
                                {
                                    Name = versionAssign.ToString(),
                                    VersionCodes = new List<long?>
                                    {
                                        versionAssign
                                    },
                                    Status = StatusCompleted,
                                    ReleaseNotes = trackReleaseOld?.ReleaseNotes,
                                }
                            };
                            sb.AppendLine($"Assign version: {versionAssign}");
                        }
                        else
                        {
                            assignTrack.Releases = new List<TrackRelease>
                            {
                                new TrackRelease
                                {
                                    Status = StatusCompleted
                                }
                            };
                            sb.AppendLine("Unassign version");
                        }
                        UpdateStatus(sb.ToString());
                        await edits.Tracks.Update(assignTrack, PackageName, appEdit.Id, track).ExecuteAsync();
                        sb.AppendLine("Track updated");
                        UpdateStatus(sb.ToString());

                        EditsResource.CommitRequest commitRequest = edits.Commit(PackageName, appEdit.Id);
                        AppEdit appEditCommit = await commitRequest.ExecuteAsync(_cts.Token);
                        sb.AppendLine($"App edit committed: {appEditCommit.Id}");
                        UpdateStatus(sb.ToString());

                        if (versionAssign.HasValue)
                        {
                            UpdateAppInfo(sb, versionAssign.Value, null, track);
                        }
                        else
                        {
                            if (trackReleaseOld?.VersionCodes != null && trackReleaseOld.VersionCodes.Count > 0)
                            {
                                if (trackReleaseOld.VersionCodes[0] != null)
                                {
                                    long versionCode = trackReleaseOld.VersionCodes[0].Value;
                                    UpdateAppInfo(sb, versionCode, null, string.Empty);
                                }
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
        private bool UpdateChanges(string track, List<UpdateInfo> apkChanges, string appVersion)
        {
            if (_serviceThread != null)
            {
                return false;
            }

            if (apkChanges == null)
            {
                UpdateStatus("No language changes info");
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
                    if (!string.IsNullOrEmpty(appVersion))
                    {
                        sb.AppendLine($"App version name: {appVersion}");
                        UpdateStatus(sb.ToString());
                    }

                    UserCredential credential = await GetCredatials();
                    using (AndroidPublisherService service = new AndroidPublisherService(GetInitializer(credential)))
                    {
                        EditsResource edits = service.Edits;
                        EditsResource.InsertRequest editRequest = edits.Insert(null, PackageName);
                        AppEdit appEdit = await editRequest.ExecuteAsync(_cts.Token);
                        Track trackResponse = await edits.Tracks.Get(PackageName, appEdit.Id, track).ExecuteAsync(_cts.Token);
                        sb.AppendLine($"Track: {track}");
                        if (trackResponse.Releases.Count != 1)
                        {
                            sb.AppendLine($"Invalid release count: {trackResponse.Releases.Count}");
                            UpdateStatus(sb.ToString());
                            throw new Exception("Invalid release count");
                        }
                        TrackRelease trackRelease = trackResponse.Releases[0];
                        if (trackRelease.VersionCodes.Count != 1 || !trackRelease.VersionCodes[0].HasValue)
                        {
                            sb.AppendLine($"Invalid version count: {trackRelease.VersionCodes.Count}");
                            UpdateStatus(sb.ToString());
                            throw new Exception("Invalid version count");
                        }
                        long currentVersion = trackRelease.VersionCodes[0].Value;
                        if (trackRelease.Name != null)
                        {
                            sb.AppendLine($"Name: {trackRelease.Name}");
                        }
                        if (trackRelease.Status != null)
                        {
                            sb.AppendLine($"Status: {trackRelease.Status}");
                        }
                        sb.AppendLine($"Version: {currentVersion}");
                        UpdateStatus(sb.ToString());

                        List<LocalizedText> releaseNotes = new List<LocalizedText>();
                        foreach (UpdateInfo updateInfo in apkChanges)
                        {
                            LocalizedText localizedText = new LocalizedText
                            {
                                Language = updateInfo.Language,
                                Text = updateInfo.Changes
                            };
                            releaseNotes.Add(localizedText);
                        }

                        string trackName = appVersion;
                        if (string.IsNullOrEmpty(trackName))
                        {
                            trackName = trackRelease.Name;
                        }
                        Track trackUpdate = new Track
                        {
                            TrackValue = track,
                            Releases = new List<TrackRelease>
                            {
                                new TrackRelease
                                {
                                    Name = trackName,
                                    VersionCodes = new List<long?>
                                    {
                                        currentVersion
                                    },
                                    Status = trackRelease.Status,
                                    ReleaseNotes = releaseNotes,
                                }
                            }
                        };

                        await edits.Tracks.Update(trackUpdate, PackageName, appEdit.Id, track).ExecuteAsync(_cts.Token);
                        sb.AppendLine($"Track {track} updated");

                        EditsResource.CommitRequest commitRequest = edits.Commit(PackageName, appEdit.Id);
                        AppEdit appEditCommit = await commitRequest.ExecuteAsync(_cts.Token);
                        sb.AppendLine($"App edit committed: {appEditCommit.Id}");
                        UpdateStatus(sb.ToString());

                        UpdateAppInfo(sb, currentVersion, appVersion, track, apkChanges);
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
        private bool UploadApk(string apkFileName, string expansionFileName, string track, List<UpdateInfo> apkChanges, string appVersion)
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

            if (!string.IsNullOrEmpty(expansionFileName))
            {
                if (expansionFileName != ExpansionKeep)
                {
                    if (!File.Exists(expansionFileName))
                    {
                        UpdateStatus("Expansion file not existing");
                        return false;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(track))
            {
                UpdateStatus("Empty track");
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
                    if (apkChanges != null)
                    {
                        sb.Append("Changes info for languages present: ");
                        foreach (UpdateInfo updateInfo in apkChanges)
                        {
                            sb.Append($"{updateInfo.Language} ");
                        }
                        sb.AppendLine();
                        UpdateStatus(sb.ToString());
                    }

                    if (!string.IsNullOrEmpty(appVersion))
                    {
                        sb.AppendLine($"App version name: {appVersion}");
                        UpdateStatus(sb.ToString());
                    }

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
                                if (expansionFileName == ExpansionKeep)
                                {
                                    sb.AppendLine("Reusing old expansion file");
                                    reuseExpansion = true;
                                }
                                else
                                {
                                    FileInfo fileInfo = new FileInfo(expansionFileName);
                                    if (fileInfo.Exists && fileInfo.Length == expansionInfo.FileSize)
                                    {
                                        sb.AppendLine("Size unchanged, reusing old expansion file");
                                        reuseExpansion = true;
                                    }
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
                                UpdateStatus(sb + $"Apk progress: {100 * progress.BytesSent / fileLength}%");
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
                                    UpdateStatus(sb + $"Expansion progress: {100 * progress.BytesSent / fileLength}%");
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

                        List<LocalizedText> releaseNotes = new List<LocalizedText>();
                        if (apkChanges != null)
                        {
                            foreach (UpdateInfo updateInfo in apkChanges)
                            {
                                LocalizedText localizedText = new LocalizedText
                                {
                                    Language = updateInfo.Language,
                                    Text = updateInfo.Changes
                                };
                                releaseNotes.Add(localizedText);
                            }
                        }

                        string trackName = appVersion;
                        if (string.IsNullOrEmpty(trackName))
                        {
                            trackName = versionCode.ToString();
                        }
                        Track trackUpdate = new Track
                        {
                            TrackValue = track,
                            Releases = new List<TrackRelease>
                            {
                                new TrackRelease
                                {
                                    Name = trackName,
                                    VersionCodes = new List<long?>
                                    {
                                        versionCode.Value
                                    },
                                    Status = StatusCompleted,
                                    ReleaseNotes = releaseNotes,
                                }
                            }
                        };

                        await edits.Tracks.Update(trackUpdate, PackageName, appEdit.Id, track).ExecuteAsync(_cts.Token);
                        sb.AppendLine($"Track {track} updated");

                        EditsResource.CommitRequest commitRequest = edits.Commit(PackageName, appEdit.Id);
                        AppEdit appEditCommit = await commitRequest.ExecuteAsync(_cts.Token);
                        sb.AppendLine($"App edit committed: {appEditCommit.Id}");
                        UpdateStatus(sb.ToString());

                        UpdateAppInfo(sb, versionCode.Value, appVersion, track, apkChanges);
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
        private bool SetAppInfo(int versionCode, string appVersion, string track, List<UpdateInfo> apkChanges)
        {
            if (_serviceThread != null)
            {
                return false;
            }
            UpdateStatus(string.Empty);
            _cts = new CancellationTokenSource();
            _serviceThread = new Thread(() =>
            {
                UpdateStatus(string.Empty);
                StringBuilder sb = new StringBuilder();
                try
                {
                    UpdateAppInfo(sb, versionCode, appVersion, track, apkChanges);
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
            textBoxVersion.Text = Properties.Settings.Default.VersionAssign;
            checkBoxUpdateName.Checked = Properties.Settings.Default.UpdateName;
            textBoxApkFile.Text = Properties.Settings.Default.ApkFileName;
            textBoxObbFile.Text = Properties.Settings.Default.ObbFileName;
            textBoxResourceFolder.Text = Properties.Settings.Default.ResourceFolder;

            comboBoxTrackAssign.BeginUpdate();
            comboBoxTrackAssign.Items.Clear();
            foreach (string track in TracksEdit)
            {
                comboBoxTrackAssign.Items.Add(track);
            }
            comboBoxTrackAssign.Items.Add("");
            comboBoxTrackAssign.SelectedItem = Properties.Settings.Default.TrackAssign;
            if (comboBoxTrackAssign.SelectedIndex < 0)
            {
                comboBoxTrackAssign.SelectedIndex = 0;
            }
            comboBoxTrackAssign.EndUpdate();

            comboBoxTrackUnassign.BeginUpdate();
            comboBoxTrackUnassign.Items.Clear();
            foreach (string track in TracksEdit)
            {
                comboBoxTrackUnassign.Items.Add(track);
            }
            comboBoxTrackUnassign.SelectedItem = Properties.Settings.Default.TrackUnassign;
            if (comboBoxTrackUnassign.SelectedIndex < 0)
            {
                comboBoxTrackUnassign.SelectedIndex = 0;
            }
            comboBoxTrackUnassign.EndUpdate();
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

        private void buttonUpdateChanges_Click(object sender, EventArgs e)
        {
            List<UpdateInfo> apkChanges = null;
            string appVersion = null;
            if (!string.IsNullOrWhiteSpace(textBoxResourceFolder.Text))
            {
                apkChanges = ReadUpdateInfo(textBoxResourceFolder.Text);
                if (apkChanges == null)
                {
                    UpdateStatus("Reading resources failed!");
                    return;
                }

                if (checkBoxUpdateName.Checked)
                {
                    appVersion = ReadAppVersion(textBoxResourceFolder.Text, out int? versionCode);
                    if (appVersion == null || versionCode == null)
                    {
                        UpdateStatus("Reading app version failed!");
                        return;
                    }
                }
            }
            UpdateChanges(comboBoxTrackAssign.Text, apkChanges, appVersion);
        }

        private void buttonChangeTrack_Click(object sender, EventArgs e)
        {
            ChangeTrack(comboBoxTrackUnassign.Text, comboBoxTrackAssign.Text);
        }

        private void buttonAssignTrack_Click(object sender, EventArgs e)
        {
            int? versionAssign = null;
            if (!string.IsNullOrWhiteSpace(textBoxVersion.Text))
            {
                if (!Int32.TryParse(textBoxVersion.Text, out int value))
                {
                    UpdateStatus("Invalid version!");
                    return;
                }
                versionAssign = value;
            }
            AssignTrack(comboBoxTrackAssign.Text, versionAssign);
        }

        private void buttonUploadApk_Click(object sender, EventArgs e)
        {
            List<UpdateInfo> apkChanges = null;
            string appVersion = null;
            if (!string.IsNullOrWhiteSpace(textBoxResourceFolder.Text))
            {
                apkChanges = ReadUpdateInfo(textBoxResourceFolder.Text);
                if (apkChanges == null)
                {
                    UpdateStatus("Reading resources failed!");
                    return;
                }

                appVersion = ReadAppVersion(textBoxResourceFolder.Text, out int? versionCode);
                if (appVersion == null || versionCode == null)
                {
                    UpdateStatus("Reading app version failed!");
                    return;
                }
            }

            UploadApk(textBoxApkFile.Text, textBoxObbFile.Text, comboBoxTrackAssign.Text, apkChanges, appVersion);
        }

        private void ButtonSetAppInfo_Click(object sender, EventArgs e)
        {
            List<UpdateInfo> apkChanges = null;
            string appVersion = null;
            int? versionCode = 0;
            if (!string.IsNullOrWhiteSpace(textBoxResourceFolder.Text))
            {
                apkChanges = ReadUpdateInfo(textBoxResourceFolder.Text);
                if (apkChanges == null)
                {
                    UpdateStatus("Reading resources failed!");
                    return;
                }

                appVersion = ReadAppVersion(textBoxResourceFolder.Text, out versionCode);
                if (appVersion == null || versionCode == null)
                {
                    UpdateStatus("Reading app version failed!");
                    return;
                }
            }

            SetAppInfo(versionCode.Value, appVersion, comboBoxTrackAssign.Text, apkChanges);
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

        private void buttonSelectResourceFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialogResource.SelectedPath = textBoxResourceFolder.Text;
            if (folderBrowserDialogResource.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            textBoxResourceFolder.Text = folderBrowserDialogResource.SelectedPath;
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.VersionAssign = textBoxVersion.Text;
                Properties.Settings.Default.UpdateName = checkBoxUpdateName.Checked;
                Properties.Settings.Default.ApkFileName = textBoxApkFile.Text;
                Properties.Settings.Default.ObbFileName = textBoxObbFile.Text;
                Properties.Settings.Default.TrackAssign = comboBoxTrackAssign.Text;
                Properties.Settings.Default.TrackUnassign = comboBoxTrackUnassign.Text;
                Properties.Settings.Default.ResourceFolder = textBoxResourceFolder.Text;
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

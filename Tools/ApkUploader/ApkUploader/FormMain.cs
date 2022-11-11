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
        private static readonly string[] SerialsOem = { "DeepOBD", "DeepOBDIbus" };
        private volatile Thread _serviceThread;
        private readonly string _apkPath;
        private CancellationTokenSource _cts;

        private class ExpansionInfo
        {
            public ExpansionInfo(int version, bool fromBundle, int expansionVersion, long fileSize)
            {
                Version = version;
                FromBundle = fromBundle;
                ExpansionVersion = expansionVersion;
                FileSize = fileSize;
            }

            public int Version { get;}
            public bool FromBundle { get; }
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

        private class SerialInfo
        {
            public SerialInfo(string serial, string serialType, string oem, bool disabled = false)
            {
                Serial = serial;
                SerialType = serialType;
                Oem = oem;
                Disabled = disabled;
            }

            public string Serial { get; }
            public string SerialType { get; }
            public string Oem { get; }
            public bool Disabled { get; }
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
            buttonListBundles.Enabled = enable;
            buttonListTracks.Enabled = enable;
            buttonUpdateChanges.Enabled = enable;
            buttonUploadBundle.Enabled = enable;
            buttonChangeTrack.Enabled = enable;
            buttonAssignTrack.Enabled = enable;
            buttonSetAppInfo.Enabled = enable;
            buttonUploadSerials.Enabled = enable;
            buttonClose.Enabled = enable;
            comboBoxSerialOem.Enabled = enable;
            comboBoxTrackAssign.Enabled = enable;
            comboBoxTrackUnassign.Enabled = enable;
            checkBoxUpdateName.Enabled = enable;
            textBoxVersion.Enabled = enable;
            textBoxBundleFile.Enabled = enable;
            textBoxObbFile.Enabled = enable;
            textBoxResourceFolder.Enabled = enable;
            textBoxSerialFileName.Enabled = enable;
            buttonSelectBundleFile.Enabled = enable;
            buttonSelectObbFile.Enabled = enable;
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

        private List<SerialInfo> ReadSerialInfo(string fileName, string oem, out string message)
        {
            message = null;
            try
            {
                if (!File.Exists(fileName))
                {
                    message = "File not found";
                    return null;
                }

                Regex regex = new Regex(@"08000000([0-9a-z]{16})[0-9a-z]{2}", RegexOptions.IgnoreCase);
                List<SerialInfo> serialInfos = new List<SerialInfo>();
                using (StreamReader sr = new StreamReader(fileName))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        MatchCollection matchesSerial = regex.Matches(line);
                        if ((matchesSerial.Count == 1) && (matchesSerial[0].Groups.Count == 2))
                        {
                            string serial = matchesSerial[0].Groups[1].Value;
                            if (serialInfos.Any(x => string.Compare(x.Serial, serial, StringComparison.OrdinalIgnoreCase) == 0))
                            {
                                message = $"Serial number {serial} duplicated";
                                return null;
                            }

                            serialInfos.Add(new SerialInfo(serial, "ELM", oem));
                        }
                    }
                }

                return serialInfos;
            }
            catch (Exception ex)
            {
                message = $"Exception ${ex.Message}";
            }

            return null;
        }

        private bool ReadCredentialsFile(string fileName, out string url, out string userName, out string password)
        {
            url = null;
            userName = null;
            password = null;
            try
            {
                string xmlFile = Path.Combine(_apkPath, fileName);
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
                    GoogleClientSecrets.FromStream(stream).Secrets,
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

        private async Task<ExpansionInfo> GetNewestApkExpansionFile(EditsResource edits, AppEdit appEdit)
        {
            try
            {
                int version = -1;
                bool? fromBundle = null;
                int expansionVersion = -1;
                long fileSize = 0;

                ApksListResponse apksResponse = await edits.Apks.List(PackageName, appEdit.Id).ExecuteAsync(_cts.Token);
                if (apksResponse.Apks != null)
                {
                    foreach (Apk apk in apksResponse.Apks)
                    {
                        // ReSharper disable once UseNullPropagation
                        if (apk.VersionCode != null)
                        {
                            if (apk.VersionCode.Value > version)
                            {
                                try
                                {
                                    ExpansionFile expansionResponse = await edits.Expansionfiles.Get(PackageName, appEdit.Id, apk.VersionCode.Value,
                                            EditsResource.ExpansionfilesResource.GetRequest.ExpansionFileTypeEnum.Main).ExecuteAsync(_cts.Token);
                                    if (expansionResponse.ReferencesVersion != null)
                                    {
                                        expansionVersion = expansionResponse.ReferencesVersion.Value;
                                        try
                                        {
                                            ExpansionFile expansionRefResponse = await edits.Expansionfiles.Get(PackageName, appEdit.Id, expansionResponse.ReferencesVersion.Value,
                                                    EditsResource.ExpansionfilesResource.GetRequest.ExpansionFileTypeEnum.Main).ExecuteAsync(_cts.Token);
                                            if (expansionRefResponse.FileSize != null && expansionRefResponse.FileSize.Value > 0)
                                            {
                                                version = apk.VersionCode.Value;
                                                fromBundle = false;
                                                expansionVersion = expansionResponse.ReferencesVersion.Value;
                                                fileSize = expansionRefResponse.FileSize.Value;
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            version = apk.VersionCode.Value;
                                            fromBundle = false;
                                            expansionVersion = expansionResponse.ReferencesVersion.Value;
                                            fileSize = 0;
                                        }
                                    }
                                    else if (expansionResponse.FileSize != null && expansionResponse.FileSize.Value > 0)
                                    {
                                        version = apk.VersionCode.Value;
                                        fromBundle = false;
                                        expansionVersion = version;
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
                }

                if (version < 0)
                {
                    return null;
                }
                return new ExpansionInfo(version, fromBundle ?? false, expansionVersion, fileSize);
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool UpdateAppInfo(StringBuilder sb, long versionCode, string appVersion, string track, List<UpdateInfo> bundleChanges = null)
        {
            try
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }
                sb.AppendLine("Updating app info");
                UpdateStatus(sb.ToString());

                if (!ReadCredentialsFile("appinfo_credentials.xml", out string url, out string userName, out string password))
                {
                    sb.AppendLine("Reading app info credentials failed!");
                    UpdateStatus(sb.ToString());
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

                if (bundleChanges != null)
                {
                    sb.Append("Changes info for languages present: ");
                    foreach (UpdateInfo updateInfo in bundleChanges)
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

                        if (bundleChanges != null)
                        {
                            foreach (UpdateInfo updateInfo in bundleChanges)
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
                                if (!string.IsNullOrEmpty(messageAttr?.Value))
                                {
                                    sb.AppendLine($"Error: {messageAttr.Value}");
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
                sb.AppendLine(_cts.IsCancellationRequested ? "Cancelled" : $"Exception: {e.Message}");
                return false;
            }

            return true;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool UploadSerials(StringBuilder sb, List<SerialInfo> serialInfos)
        {
            try
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "Uploading {0} serial numbers", serialInfos?.Count));
                UpdateStatus(sb.ToString());

                if (serialInfos == null || serialInfos.Count == 0)
                {
                    sb.AppendLine("No serial numbers present");
                    UpdateStatus(sb.ToString());
                    return false;
                }

                if (!ReadCredentialsFile("serial_credentials.xml", out string url, out string userName, out string password))
                {
                    sb.AppendLine("Reading serial credentials failed!");
                    UpdateStatus(sb.ToString());
                    return false;
                }

                using (HttpClientHandler httpClientHandler = new HttpClientHandler())
                {
                    httpClientHandler.Credentials = new NetworkCredential(userName, password);
                    using (HttpClient httpClient = new HttpClient(httpClientHandler))
                    {
                        for (int stage = 0; stage < 2; stage++)
                        {
                            bool valid = true;
                            int index = 0;
                            foreach (SerialInfo serialInfo in serialInfos)
                            {
                                int percent = 100 * index / serialInfos.Count;
                                string message;
                                string check;
                                if (stage == 0)
                                {
                                    message = string.Format(CultureInfo.InvariantCulture, "Checking: {0}%", percent);
                                    check = "1";
                                }
                                else
                                {
                                    message = string.Format(CultureInfo.InvariantCulture, "Uploading: {0}%", percent);
                                    check = "0";
                                }
                                UpdateStatus(sb + message);

                                // ReSharper disable once UseObjectOrCollectionInitializer
                                MultipartFormDataContent formSerialInfo = new MultipartFormDataContent();
                                formSerialInfo.Add(new StringContent(check), "check");
                                formSerialInfo.Add(new StringContent(serialInfo.Serial), "serial");
                                formSerialInfo.Add(new StringContent(serialInfo.SerialType), "type");
                                if (!string.IsNullOrEmpty(serialInfo.Oem))
                                {
                                    formSerialInfo.Add(new StringContent(serialInfo.Oem), "oem");
                                }
                                formSerialInfo.Add(new StringContent(serialInfo.Disabled ? "1" : "0"), "disabled");

                                HttpResponseMessage responseAppInfo = httpClient.PostAsync(url, formSerialInfo, _cts.Token).Result;
                                responseAppInfo.EnsureSuccessStatusCode();
                                string responseAppInfoXml = responseAppInfo.Content.ReadAsStringAsync().Result;

                                try
                                {
                                    XDocument xmlDoc = XDocument.Parse(responseAppInfoXml);
                                    if (xmlDoc.Root == null)
                                    {
                                        throw new Exception("XML invalid");
                                    }

                                    XElement statusNode = xmlDoc.Root?.Element("status");
                                    if (statusNode != null)
                                    {
                                        XAttribute okAttr = statusNode.Attribute("ok");
                                        if (string.Compare(okAttr?.Value ?? string.Empty, "true", StringComparison.OrdinalIgnoreCase) != 0)
                                        {
                                            valid = false;
                                            sb.AppendLine("Invalid status");
                                        }
                                    }

                                    XElement errorNode = xmlDoc.Root?.Element("error");
                                    if (errorNode != null)
                                    {
                                        valid = false;
                                        XAttribute messageAttr = errorNode.Attribute("message");
                                        if (!string.IsNullOrEmpty(messageAttr?.Value))
                                        {
                                            sb.AppendLine($"Serial: {serialInfo.Serial}, Type: {serialInfo.SerialType}");
                                            sb.AppendLine($"Error: {messageAttr.Value}");
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    valid = false;
                                    sb.AppendLine("Response invalid:");
                                    sb.AppendLine(responseAppInfoXml);
                                }

                                if (!valid)
                                {
                                    break;
                                }

                                if (_cts.IsCancellationRequested)
                                {
                                    break;
                                }

                                index++;
                            }

                            if (stage == 0)
                            {
                                if (valid)
                                {
                                    sb.AppendLine("Check ok");
                                }
                                else
                                {
                                    sb.AppendLine("Check failed");
                                    break;
                                }
                            }
                            else
                            {
                                if (valid)
                                {
                                    sb.AppendLine("Upload ok");
                                }
                                else
                                {
                                    sb.AppendLine("Upload failed");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                sb.AppendLine(_cts.IsCancellationRequested ? "Cancelled" : $"Exception: {e.Message}");
                return false;
            }

            return true;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool ListBundles()
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

                            BundlesListResponse bundlesResponse = await edits.Bundles.List(PackageName, appEdit.Id).ExecuteAsync(_cts.Token);
                            if (bundlesResponse.Bundles == null)
                            {
                                sb.AppendLine("No bundles");
                            }
                            else
                            {
                                sb.AppendLine("Bundles:");
                                foreach (Bundle bundle in bundlesResponse.Bundles)
                                {
                                    if (bundle.VersionCode != null)
                                    {
                                        sb.AppendLine($"Version: {bundle.VersionCode.Value}, SHA1: {bundle.Sha1}");
                                        await PrintExpansion(sb, edits, appEdit, bundle.VersionCode.Value);
                                    }
                                }
                            }

                            ApksListResponse apksResponse = await edits.Apks.List(PackageName, appEdit.Id).ExecuteAsync(_cts.Token);
                            if (apksResponse.Apks == null)
                            {
                                sb.AppendLine("No apks");
                            }
                            else
                            {
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
                    }
                    catch (Exception e)
                    {
                        sb.AppendLine(_cts.IsCancellationRequested ? "Cancelled" : $"Exception: {e.Message}");
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
                    sb.AppendLine(_cts.IsCancellationRequested ? "Cancelled" : $"Exception: {e.Message}");
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
                    sb.AppendLine(_cts.IsCancellationRequested ? "Cancelled" : $"Exception: {e.Message}");
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
                    sb.AppendLine(_cts.IsCancellationRequested ? "Cancelled" : $"Exception: {e.Message}");
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
        private bool UpdateChanges(string track, List<UpdateInfo> bundleChanges, string appVersion)
        {
            if (_serviceThread != null)
            {
                return false;
            }

            if (bundleChanges == null)
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
                        if (trackRelease.VersionCodes == null)
                        {
                            sb.AppendLine("No version codes present");
                            UpdateStatus(sb.ToString());
                            throw new Exception("Invalid versions");
                        }
                        if (trackRelease.VersionCodes.Count != 1 || !trackRelease.VersionCodes[0].HasValue)
                        {
                            sb.AppendLine($"Invalid version count: {trackRelease.VersionCodes?.Count}");
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
                        foreach (UpdateInfo updateInfo in bundleChanges)
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

                        UpdateAppInfo(sb, currentVersion, appVersion, track, bundleChanges);
                    }
                }
                catch (Exception e)
                {
                    sb.AppendLine(_cts.IsCancellationRequested ? "Cancelled" : $"Exception: {e.Message}");
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
        private bool UploadBundle(string bundleFileName, string expansionFileName, string track, List<UpdateInfo> bundleChanges, string appVersion)
        {
            if (_serviceThread != null)
            {
                return false;
            }

            if (!File.Exists(bundleFileName))
            {
                UpdateStatus("Bundle/Apk file not existing");
                return false;
            }

            string ext = Path.GetExtension(bundleFileName);
            bool isApk = !string.IsNullOrEmpty(ext) && string.Compare(ext, ".apk", StringComparison.OrdinalIgnoreCase) == 0;

            if (isApk && !string.IsNullOrEmpty(expansionFileName))
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
                    if (bundleChanges != null)
                    {
                        sb.Append("Changes info for languages present: ");
                        foreach (UpdateInfo updateInfo in bundleChanges)
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
                        ExpansionInfo expansionInfo = null;
                        if (isApk)
                        {
                            expansionInfo = await GetNewestApkExpansionFile(edits, appEdit);
                            if (expansionInfo != null)
                            {
                                sb.AppendLine($"Latest expansion: version={expansionInfo.Version}, bundle = {expansionInfo.FromBundle}, expansion version={expansionInfo.ExpansionVersion}, size={expansionInfo.FileSize}");
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
                            UpdateStatus(sb.ToString());
                        }

                        _cts.Token.ThrowIfCancellationRequested();
                        int? versionCode = null;
                        using (FileStream bundleStream = new FileStream(bundleFileName, FileMode.Open, FileAccess.Read))
                        {
                            long fileLength = (bundleStream.Length > 0) ? bundleStream.Length : 1;

                            if (isApk)
                            {
                                Apk apkUploaded = null;
                                EditsResource.ApksResource.UploadMediaUpload uploadApk = edits.Apks.Upload(PackageName, appEdit.Id, bundleStream, "application/vnd.android.package-archive");
                                uploadApk.ChunkSize = ResumableUpload.MinimumChunkSize;
                                uploadApk.ProgressChanged += progress =>
                                {
                                    UpdateStatus(sb + $"Apk progress: {100 * progress.BytesSent / fileLength}%");
                                };
                                uploadApk.ResponseReceived += apk =>
                                {
                                    apkUploaded = apk;
                                };
                                IUploadProgress uploadProgress = await uploadApk.UploadAsync(_cts.Token);
                                sb.AppendLine($"Upload status: {uploadProgress.Status}");
                                UpdateStatus(sb.ToString());
                                if (uploadProgress.Exception != null)
                                {
                                    throw uploadProgress.Exception;
                                }
                                versionCode = apkUploaded?.VersionCode;
                            }
                            else
                            {
                                Bundle bundleUploaded = null;
                                EditsResource.BundlesResource.UploadMediaUpload uploadBundle = edits.Bundles.Upload(PackageName, appEdit.Id, bundleStream, "application/octet-stream");
                                uploadBundle.ChunkSize = ResumableUpload.MinimumChunkSize;
                                uploadBundle.ProgressChanged += progress =>
                                {
                                    UpdateStatus(sb + $"Bundle progress: {100 * progress.BytesSent / fileLength}%");
                                };
                                uploadBundle.ResponseReceived += bundle =>
                                {
                                    bundleUploaded = bundle;
                                };
                                IUploadProgress uploadProgress = await uploadBundle.UploadAsync(_cts.Token);
                                sb.AppendLine($"Upload status: {uploadProgress.Status}");
                                UpdateStatus(sb.ToString());
                                if (uploadProgress.Exception != null)
                                {
                                    throw uploadProgress.Exception;
                                }
                                versionCode = bundleUploaded?.VersionCode;
                            }
                        }

                        if (!versionCode.HasValue)
                        {
                            throw new Exception("No version code");
                        }
                        sb.AppendLine($"Version code uploaded: {versionCode.Value}");
                        UpdateStatus(sb.ToString());

                        if (isApk)
                        {
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
                                    sb.AppendLine($"Upload status: {uploadProgress.Status}");
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
                        }

                        List<LocalizedText> releaseNotes = new List<LocalizedText>();
                        if (bundleChanges != null)
                        {
                            foreach (UpdateInfo updateInfo in bundleChanges)
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

                        UpdateAppInfo(sb, versionCode.Value, appVersion, track, bundleChanges);
                    }
                }
                catch (Exception e)
                {
                    sb.AppendLine(_cts.IsCancellationRequested ? "Cancelled" : $"Exception: {e.Message}");
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
        private bool SetAppInfo(int versionCode, string appVersion, string track, List<UpdateInfo> bundleChanges)
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
                    UpdateAppInfo(sb, versionCode, appVersion, track, bundleChanges);
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
        private bool UploadSerials(List<SerialInfo> serialInfos)
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
                    UploadSerials(sb, serialInfos);
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
            textBoxBundleFile.Text = Properties.Settings.Default.BundleFileName;
            textBoxObbFile.Text = Properties.Settings.Default.ObbFileName;
            textBoxResourceFolder.Text = Properties.Settings.Default.ResourceFolder;
            textBoxSerialFileName.Text = Properties.Settings.Default.SerialFileName;

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

            comboBoxSerialOem.BeginUpdate();
            foreach (string oem in SerialsOem)
            {
                comboBoxSerialOem.Items.Add(oem);
            }
            comboBoxSerialOem.SelectedIndex = 0;
            comboBoxSerialOem.EndUpdate();
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

        private void buttonListBundles_Click(object sender, EventArgs e)
        {
            ListBundles();
        }

        private void buttonListTracks_Click(object sender, EventArgs e)
        {
            ListTracks();
        }

        private void buttonUpdateChanges_Click(object sender, EventArgs e)
        {
            List<UpdateInfo> bundleChanges = null;
            string appVersion = null;
            if (!string.IsNullOrWhiteSpace(textBoxResourceFolder.Text))
            {
                bundleChanges = ReadUpdateInfo(textBoxResourceFolder.Text);
                if (bundleChanges == null)
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
            UpdateChanges(comboBoxTrackAssign.Text, bundleChanges, appVersion);
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

        private void buttonUploadBundle_Click(object sender, EventArgs e)
        {
            List<UpdateInfo> bundleChanges = null;
            string appVersion = null;
            if (!string.IsNullOrWhiteSpace(textBoxResourceFolder.Text))
            {
                bundleChanges = ReadUpdateInfo(textBoxResourceFolder.Text);
                if (bundleChanges == null)
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

            UploadBundle(textBoxBundleFile.Text, textBoxObbFile.Text, comboBoxTrackAssign.Text, bundleChanges, appVersion);
        }

        private void buttonSetAppInfo_Click(object sender, EventArgs e)
        {
            List<UpdateInfo> bundleChanges = null;
            string appVersion = null;
            int? versionCode = 0;
            if (!string.IsNullOrWhiteSpace(textBoxResourceFolder.Text))
            {
                bundleChanges = ReadUpdateInfo(textBoxResourceFolder.Text);
                if (bundleChanges == null)
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

            SetAppInfo(versionCode.Value, appVersion, comboBoxTrackAssign.Text, bundleChanges);
        }


        private void buttonUploadSerials_Click(object sender, EventArgs e)
        {
            string oem = comboBoxSerialOem.Text;
            if (string.IsNullOrEmpty(oem))
            {
                UpdateStatus("No OEM selected!");
                return;
            }

            List<SerialInfo> serialInfos = ReadSerialInfo(textBoxSerialFileName.Text, oem, out string message);
            if (!string.IsNullOrEmpty(message))
            {
                UpdateStatus(message);
                return;
            }

            if (serialInfos == null || serialInfos.Count == 0)
            {
                UpdateStatus("Reading serial numbers failed!");
                return;
            }

            UploadSerials(serialInfos);
        }

        private void buttonSelectBundleFile_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxBundleFile.Text))
            {
                openFileDialogBundle.FileName = textBoxBundleFile.Text;
                openFileDialogBundle.InitialDirectory = Path.GetDirectoryName(textBoxBundleFile.Text);
            }
            if (openFileDialogBundle.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            textBoxBundleFile.Text = openFileDialogBundle.FileName;
        }

        private void buttonSelectObbFile_Click(object sender, EventArgs e)
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

        private void buttonSelectSerialFile_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxSerialFileName.Text))
            {
                openFileDialogSerial.FileName = textBoxSerialFileName.Text;
                openFileDialogSerial.InitialDirectory = Path.GetDirectoryName(textBoxSerialFileName.Text);
            }
            if (openFileDialogSerial.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            textBoxSerialFileName.Text = openFileDialogSerial.FileName;
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.VersionAssign = textBoxVersion.Text;
                Properties.Settings.Default.UpdateName = checkBoxUpdateName.Checked;
                Properties.Settings.Default.BundleFileName = textBoxBundleFile.Text;
                Properties.Settings.Default.ObbFileName = textBoxObbFile.Text;
                Properties.Settings.Default.ResourceFolder = textBoxResourceFolder.Text;
                Properties.Settings.Default.SerialFileName = textBoxSerialFileName.Text;
                Properties.Settings.Default.TrackAssign = comboBoxTrackAssign.Text;
                Properties.Settings.Default.TrackUnassign = comboBoxTrackUnassign.Text;
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

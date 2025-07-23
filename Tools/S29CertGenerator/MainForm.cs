using EdiabasLib;
using Microsoft.Win32;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace S29CertGenerator
{
    public partial class MainForm : Form
    {
        private string _appDir;
        private string _ediabasPath;
        private AsymmetricKeyParameter _caKeyResource;
        private List<X509CertificateEntry> _caPublicCertificates;
        private AsymmetricKeyParameter _istaKeyResource;
        private List<X509CertificateEntry> _istaPublicCertificates;
        private string _clientConfigText;
        private volatile bool _taskActive = false;
        public const string RegKeyIsta = @"SOFTWARE\BMWGroup\ISPI\ISTA";
        public const string RegValueIstaLocation = @"InstallLocation";
        public const string EdiabasDirName = @"Ediabas";
        public const string EdiabasSecurityDirName = @"Security";
        public const string EdiabasS29DirName = @"S29";
        public const string EdiabasSllTrustDirName = @"SSL_Truststore";

        public MainForm()
        {
            InitializeComponent();

            _appDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            _ediabasPath = DetectEdiabasPath();
        }

        private void MainForm_Load(object sender, System.EventArgs e)
        {
            Icon = Properties.Resources.AppIcon;
            checkBoxForceCreate.Checked = false;

            LoadSettings();
            ValidateSetting();
            UpdateStatusText(string.Empty);
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            comboBoxVinList.BeginUpdate();
            comboBoxVinList.Items.Clear();
            comboBoxVinList.Items.Add(string.Empty);
            comboBoxVinList.SelectedIndex = 0;
            comboBoxVinList.EndUpdate();

            UpdateDisplay();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            StoreSettings();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_taskActive)
            {
                e.Cancel = true;
            }
        }

        private void buttonClose_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private bool LoadSettings()
        {
            try
            {
                textBoxCaCeyFile.Text = Properties.Settings.Default.CaKeyFile;
                textBoxIstaKeyFile.Text = Properties.Settings.Default.IstaKeyFile;
                textBoxCaCertsFile.Text = Properties.Settings.Default.CaCertsFile;
                textBoxSecurityFolder.Text = Properties.Settings.Default.SecurityFolder;
                textBoxJsonRequestFolder.Text = Properties.Settings.Default.JsonRequestFolder;
                textBoxJsonResponseFolder.Text = Properties.Settings.Default.JsonResponseFolder;
                textBoxCertOutputFolder.Text = Properties.Settings.Default.CertOutputFolder;
                textBoxTrustStoreFolder.Text = Properties.Settings.Default.TrustStoreFolder;
                textBoxClientConfigurationFile.Text = Properties.Settings.Default.ClientConfigurationFile;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool StoreSettings()
        {
            try
            {
                Properties.Settings.Default.CaKeyFile = textBoxCaCeyFile.Text;
                Properties.Settings.Default.IstaKeyFile = textBoxIstaKeyFile.Text;
                Properties.Settings.Default.CaCertsFile = textBoxCaCertsFile.Text;
                Properties.Settings.Default.SecurityFolder = textBoxSecurityFolder.Text;
                Properties.Settings.Default.JsonRequestFolder = textBoxJsonRequestFolder.Text;
                Properties.Settings.Default.JsonResponseFolder = textBoxJsonResponseFolder.Text;
                Properties.Settings.Default.CertOutputFolder = textBoxCertOutputFolder.Text;
                Properties.Settings.Default.TrustStoreFolder = textBoxTrustStoreFolder.Text;
                Properties.Settings.Default.ClientConfigurationFile = textBoxClientConfigurationFile.Text;
                Properties.Settings.Default.Save();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void UpdateDisplay()
        {
            try
            {
                bool active = _taskActive;
                bool caKeyValid = LoadCaKey(textBoxCaCeyFile.Text);
                bool istaKeyValid = LoadIstaKey(textBoxIstaKeyFile.Text);
                bool cacertsValid = LoadCaCerts(textBoxCaCertsFile.Text);
                bool clientConfigValid = LoadClientConfiguration(textBoxClientConfigurationFile.Text);
                bool isValid = IsSettingValid();

                if (caKeyValid && istaKeyValid && cacertsValid && clientConfigValid && isValid && !active)
                {
                    buttonInstall.Enabled = true;
                    buttonUninstall.Enabled = true;

                    buttonInstall.Focus();
                }
                else
                {
                    buttonInstall.Enabled = false;
                    buttonUninstall.Enabled = false;
                }

                buttonSearchVehicles.Enabled = !active;
                buttonClose.Enabled = !active;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void UpdateStatusText(string text, bool appendText = false)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() =>
                {
                    UpdateStatusText(text, appendText);
                }));
                return;
            }

            StringBuilder sb = new StringBuilder();
            if (appendText)
            {
                string lastText = richTextBoxStatus.Text;
                if (!string.IsNullOrEmpty(lastText))
                {
                    sb.Append(lastText);
                }
            }

            if (!string.IsNullOrEmpty(text) || appendText)
            {
                if (sb.Length > 0)
                {
                    sb.Append("\r\n");
                }
                sb.Append(text);
            }

            richTextBoxStatus.Text = sb.ToString();
            richTextBoxStatus.SelectionStart = richTextBoxStatus.TextLength;
            richTextBoxStatus.Update();
            richTextBoxStatus.ScrollToCaret();
        }

        private void ValidateSetting(bool force = false)
        {
            if (force || !IsSettingValid(true))
            {
                if (!string.IsNullOrEmpty(_ediabasPath))
                {
                    textBoxSecurityFolder.Text = Path.Combine(_ediabasPath, EdiabasSecurityDirName);
                    SyncFolders(textBoxSecurityFolder.Text);
                }
            }

            if (force || !LoadCaKey(textBoxCaCeyFile.Text))
            {
                if (!string.IsNullOrEmpty(_ediabasPath))
                {
                    SetCaKeyFile(_ediabasPath);
                }
            }

            if (force || !LoadIstaKey(textBoxIstaKeyFile.Text))
            {
                if (!string.IsNullOrEmpty(_ediabasPath))
                {
                    SetIstaKeyFile(_ediabasPath);
                }
            }

            if (force || !LoadCaCerts(textBoxCaCertsFile.Text))
            {
                if (!string.IsNullOrEmpty(_ediabasPath))
                {
                    SetCaCertsFile(_ediabasPath);
                }
            }

            if (force || !LoadClientConfiguration(textBoxClientConfigurationFile.Text))
            {
                string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                SetClientConfiguration(appDataFolder);
            }
        }

        private bool IsSettingValid(bool ignoreKeyFiles = false)
        {
            try
            {
                string caKeyFile = textBoxCaCeyFile.Text.Trim();
                string istaKeyFile = textBoxIstaKeyFile.Text.Trim();
                string s29Folder = textBoxSecurityFolder.Text.Trim();
                string jsonRequestFolder = textBoxJsonRequestFolder.Text.Trim();
                string jsonResponseFolder = textBoxJsonResponseFolder.Text.Trim();
                string certOutputFolder = textBoxCertOutputFolder.Text.Trim();
                string trustStoreFolder = textBoxTrustStoreFolder.Text.Trim();

                if (!ignoreKeyFiles)
                {
                    if (string.IsNullOrEmpty(caKeyFile) || !File.Exists(caKeyFile))
                    {
                        return false;
                    }

                    if (string.IsNullOrEmpty(istaKeyFile) || !File.Exists(istaKeyFile))
                    {
                        return false;
                    }
                }

                if (string.IsNullOrEmpty(s29Folder) || !Directory.Exists(s29Folder))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(jsonRequestFolder) || !Directory.Exists(jsonRequestFolder))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(jsonResponseFolder) || !Directory.Exists(jsonResponseFolder))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(certOutputFolder) || !Directory.Exists(certOutputFolder))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(trustStoreFolder) || !Directory.Exists(trustStoreFolder))
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<List<EdInterfaceEnet.EnetConnection>> SearchVehiclesTask()
        {
            // ReSharper disable once ConvertClosureToMethodGroup
            return await Task.Run(() => SearchVehicles()).ConfigureAwait(false);
        }

        private List<EdInterfaceEnet.EnetConnection> SearchVehicles()
        {
            List<EdInterfaceEnet.EnetConnection> detectedVehicles;
            using (EdInterfaceEnet edInterface = new EdInterfaceEnet(false))
            {
                detectedVehicles = edInterface.DetectedVehicles(EdInterfaceEnet.AutoIp + EdInterfaceEnet.AutoIpAll,
                    new List<EdInterfaceEnet.CommunicationMode>()
                    {
                        EdInterfaceEnet.CommunicationMode.DoIp
                    });
            }

            return detectedVehicles;
        }

        private string DetectEdiabasPath()
        {
            string ediabasPath;
            string ediabasBinPath = Environment.GetEnvironmentVariable("ediabas_config_dir");
            if (!string.IsNullOrEmpty(ediabasBinPath) && Directory.Exists(ediabasBinPath))
            {
                ediabasPath = Directory.GetParent(ediabasBinPath)?.FullName;
                if (IsValidEdiabasPath(ediabasPath))
                {
                    return ediabasPath;
                }
            }

            ediabasPath = Environment.GetEnvironmentVariable("EDIABAS_PATH");
            if (IsValidEdiabasPath(ediabasPath))
            {
                return ediabasPath;
            }

            try
            {
                using (RegistryKey localMachine32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                {
                    using (RegistryKey key = localMachine32.OpenSubKey(RegKeyIsta))
                    {
                        string path = key?.GetValue(RegValueIstaLocation, null) as string;
                        if (!string.IsNullOrEmpty(path))
                        {
                            ediabasPath = Path.Combine(path, EdiabasDirName);
                            if (IsValidEdiabasPath(ediabasPath))
                            {
                                return ediabasPath;
                            }
                        }
                    }
                }

                using (RegistryKey localMachine64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    using (RegistryKey key = localMachine64.OpenSubKey(RegKeyIsta))
                    {
                        string path = key?.GetValue(RegValueIstaLocation, null) as string;
                        if (!string.IsNullOrEmpty(path))
                        {
                            ediabasPath = Path.Combine(path, EdiabasDirName);
                            if (IsValidEdiabasPath(ediabasPath))
                            {
                                return ediabasPath;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        private bool IsValidEdiabasPath(string ediabasPath)
        {
            if (string.IsNullOrEmpty(ediabasPath) || !Directory.Exists(ediabasPath))
            {
                return false;
            }

            string securityPath = Path.Combine(ediabasPath, EdiabasSecurityDirName);
            if (!Directory.Exists(securityPath))
            {
                return false;
            }

            string s29Folder = Path.Combine(securityPath, EdiabasS29DirName);
            if (!Directory.Exists(s29Folder))
            {
                return false;
            }

            string sslTrustFolder = Path.Combine(securityPath, EdiabasSllTrustDirName);
            if (!Directory.Exists(sslTrustFolder))
            {
                return false;
            }

            string jsonRequestFolder = Path.Combine(s29Folder, "JSONRequests");
            string jsonResponseFolder = Path.Combine(s29Folder, "JSONResponses");
            string certOutputFolder = Path.Combine(s29Folder, "Certificates");
            if (!Directory.Exists(jsonRequestFolder) || !Directory.Exists(jsonResponseFolder) || !Directory.Exists(certOutputFolder))
            {
                return false;
            }

            return true;
        }

        private bool SyncFolders(string securityFolder)
        {
            if (string.IsNullOrEmpty(securityFolder) || !Directory.Exists(securityFolder))
            {
                return false;
            }

            string s29Folder = Path.Combine(securityFolder, EdiabasS29DirName);
            string jsonRequestFolder = Path.Combine(s29Folder, "JSONRequests");
            string jsonResponseFolder = Path.Combine(s29Folder, "JSONResponses");
            string certOutputFolder = Path.Combine(s29Folder, "Certificates");
            string trustStoreFolder = Path.Combine(securityFolder, "SSL_Truststore");

            if (!Directory.Exists(jsonRequestFolder))
            {
                jsonRequestFolder = string.Empty;
            }
            textBoxJsonRequestFolder.Text = jsonRequestFolder;

            if (!Directory.Exists(jsonResponseFolder))
            {
                jsonResponseFolder = string.Empty;
            }
            textBoxJsonResponseFolder.Text = jsonResponseFolder;

            if (!Directory.Exists(certOutputFolder))
            {
                certOutputFolder = string.Empty;
            }
            textBoxCertOutputFolder.Text = certOutputFolder;

            if (!Directory.Exists(trustStoreFolder))
            {
                trustStoreFolder = string.Empty;
            }
            textBoxTrustStoreFolder.Text = trustStoreFolder;

            return true;
        }

        private bool SetCaKeyFile(string ediabasPath)
        {
            if (string.IsNullOrWhiteSpace(ediabasPath) || !Directory.Exists(ediabasPath))
            {
                return false;
            }

            string caKeyFile = Path.Combine(AppContext.BaseDirectory, "rootCA_EC.pfx");
            if (!File.Exists(caKeyFile))
            {
                caKeyFile = string.Empty;
            }

            textBoxCaCeyFile.Text = caKeyFile;
            return true;
        }

        private bool SetIstaKeyFile(string ediabasPath)
        {
            if (string.IsNullOrWhiteSpace(ediabasPath) || !Directory.Exists(ediabasPath))
            {
                return false;
            }

            string istaFolder = Directory.GetParent(ediabasPath)?.FullName;
            if (string.IsNullOrEmpty(istaFolder))
            {
                return false;
            }

            string istaKeyFile = Path.Combine(istaFolder, "TesterGUI", "keyContainer.pfx");
            if (!File.Exists(istaKeyFile))
            {
                istaKeyFile = string.Empty;
            }

            textBoxIstaKeyFile.Text = istaKeyFile;
            return true;
        }

        private bool SetCaCertsFile(string ediabasPath)
        {
            if (string.IsNullOrWhiteSpace(ediabasPath) || !Directory.Exists(ediabasPath))
            {
                return false;
            }

            string istaFolder = Directory.GetParent(ediabasPath)?.FullName;
            if (string.IsNullOrEmpty(istaFolder))
            {
                return false;
            }

            string caCertsFile = Path.Combine(istaFolder, "PSdZ", "Security", "cacerts");
            if (!File.Exists(caCertsFile))
            {
                caCertsFile = string.Empty;
            }

            textBoxCaCertsFile.Text = caCertsFile;
            return true;
        }

        private bool SetClientConfiguration(string appDataFolder)
        {
            if (string.IsNullOrWhiteSpace(appDataFolder) || !Directory.Exists(appDataFolder))
            {
                return false;
            }

            string clientConfigFile = Path.Combine(appDataFolder, "BMW", "ISPI", "config", "iLean", "ISPI Admin Client", "ClientConfiguration.enc");
            if (!File.Exists(clientConfigFile))
            {
                clientConfigFile = string.Empty;
            }

            textBoxClientConfigurationFile.Text = clientConfigFile;
            return true;
        }

        private bool LoadCaKey(string caKeyFile)
        {
            _caKeyResource = null;
            _caPublicCertificates = null;

            try
            {
                if (string.IsNullOrWhiteSpace(caKeyFile) || !File.Exists(caKeyFile))
                {
                    return false;
                }

                AsymmetricKeyParameter privateKeyResource = EdBcTlsUtilities.LoadPkcs12Key(caKeyFile, string.Empty, out X509CertificateEntry[] publicCertificateEntries);
                if (privateKeyResource == null)
                {
                    return false; // Failed to load private key
                }

                if (publicCertificateEntries == null || publicCertificateEntries.Length < 1)
                {
                    return false; // Failed to load public certificates
                }

                if (!publicCertificateEntries[0].Certificate.IsValid(DateTime.UtcNow.AddDays(1.0)))
                {
                    return false; // Public certificate is not valid in the future
                }

                _caKeyResource = privateKeyResource;
                _caPublicCertificates = publicCertificateEntries.ToList();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool LoadIstaKey(string istaKeyFile)
        {
            _istaKeyResource = null;
            _istaPublicCertificates = null;

            try
            {
                if (string.IsNullOrWhiteSpace(istaKeyFile) || !File.Exists(istaKeyFile))
                {
                    return false;
                }

                AsymmetricKeyParameter privateKeyResource = EdBcTlsUtilities.LoadPkcs12Key(istaKeyFile, EdSec4Diag.IstaPkcs12KeyPwd, out X509CertificateEntry[] publicCertificateEntries);
                if (privateKeyResource == null)
                {
                    return false; // Failed to load private key
                }

                if (publicCertificateEntries == null || publicCertificateEntries.Length < 1)
                {
                    return false; // Failed to load public certificates
                }

                if (!publicCertificateEntries[0].Certificate.IsValid(DateTime.UtcNow.AddDays(1.0)))
                {
                    return false; // Public certificate is not valid in the future
                }

                _istaKeyResource = privateKeyResource;
                _istaPublicCertificates = publicCertificateEntries.ToList();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool LoadCaCerts(string caCertsFile)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(caCertsFile) || !File.Exists(caCertsFile))
                {
                    return false;
                }

                string certAlias = EdBcTlsUtilities.JksStoreGetCertAlias(null, caCertsFile);
                if (certAlias == null)
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool LoadClientConfiguration(string clientConfigFile)
        {
            _clientConfigText = null;

            try
            {
                if (string.IsNullOrWhiteSpace(clientConfigFile))
                {
                    return true;
                }

                if (!File.Exists(clientConfigFile))
                {
                    return false;
                }

                string text = PsdzClient.Utility.Encryption.DecryptFile(clientConfigFile);
                if (string.IsNullOrEmpty(text))
                {
                    return false;
                }

                _clientConfigText = text;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool InstallCaCert(string caCertsFile)
        {
            try
            {
                if (_caPublicCertificates == null || _caPublicCertificates.Count < 1)
                {
                    UpdateStatusText("CA public certificate is not loaded", true);
                    return false;
                }

                Org.BouncyCastle.X509.X509Certificate caCert = _caPublicCertificates[0].Certificate;
                string certAlias = EdBcTlsUtilities.JksStoreGetCertAlias(caCert, caCertsFile);
                if (certAlias == null)
                {
                    UpdateStatusText("CA public certificate store reading failed", true);
                    return false;
                }

                if (string.IsNullOrEmpty(certAlias))
                {
                    string bakFile = caCertsFile + ".bak";
                    if (!File.Exists(bakFile))
                    {
                        File.Copy(caCertsFile, bakFile, true);
                        UpdateStatusText("CACerts backup created", true);
                    }

                    if (!EdBcTlsUtilities.JksStoreInstallCert(caCert, caCertsFile))
                    {
                        UpdateStatusText("CA public certificate installation failed", true);
                        return false;
                    }

                    UpdateStatusText("CA public certificate installed", true);
                }
                else
                {
                    UpdateStatusText("CA public certificate is already installed", true);
                }

                return true;
            }
            catch (Exception e)
            {
                UpdateStatusText($"Install CA certificate exception: {e.Message}", true);
                return false;
            }
        }

        private bool UninstallCaCert(string caCertsFile)
        {
            try
            {
                string bakFile = caCertsFile + ".bak";
                if (File.Exists(bakFile))
                {
                    File.Move(bakFile, caCertsFile, true);
                    UpdateStatusText("CACerts backup restored", true);
                }
                else
                {
                    UpdateStatusText("No CACerts backup found", true);
                }

                return true;
            }
            catch (Exception e)
            {
                UpdateStatusText($"Uninstall CA certificate exception: {e.Message}", true);
                return false;
            }
        }

        private bool InstallTrustedCaCert(string trustStoreFolder)
        {
            try
            {
                if (_caPublicCertificates == null || _caPublicCertificates.Count < 1)
                {
                    UpdateStatusText("CA public certificate is not loaded", true);
                    return false;
                }

                Org.BouncyCastle.X509.X509Certificate caCert = _caPublicCertificates[0].Certificate;
                string subjectHash = EdBcTlsUtilities.CreateSubjectHash(caCert);
                if (string.IsNullOrEmpty(subjectHash))
                {
                    UpdateStatusText("CA public certificate subject hash is empty", true);
                    return false;
                }

                string caFileName = EdBcTlsUtilities.GetCaCertFileName(caCert, trustStoreFolder);
                if (caFileName == null)
                {
                    UpdateStatusText("Trusted folder reading failed", true);
                    return false;
                }

                string fileNameHash = Path.GetFileNameWithoutExtension(caFileName);
                if (string.IsNullOrEmpty(fileNameHash))
                {
                    string newCaFile = EdBcTlsUtilities.StoreCaCert(caCert, trustStoreFolder);
                    if (string.IsNullOrEmpty(newCaFile))
                    {
                        UpdateStatusText("Trusted CA certificate installation failed", true);
                        return false;
                    }

                    string newCaFileName = Path.GetFileName(newCaFile);
                    UpdateStatusText($"Trusted CA certificate installed: {newCaFileName}", true);
                }
                else
                {
                    if (string.Compare(fileNameHash, subjectHash, StringComparison.Ordinal) != 0)
                    {
                        UpdateStatusText($"Trusted CA certificate file name hash mismatch: {fileNameHash} != {subjectHash}", true);
                        return false;
                    }

                    string currentCaFileName = Path.GetFileName(caFileName);
                    UpdateStatusText($"Trusted CA certificate is already installed: {currentCaFileName}", true);
                }

                return true;
            }
            catch (Exception e)
            {
                UpdateStatusText($"Install CA certificate exception: {e.Message}", true);
                return false;
            }
        }

        private bool UninstallTrustedCaCert(string trustStoreFolder)
        {
            try
            {
                if (_caPublicCertificates == null || _caPublicCertificates.Count < 1)
                {
                    UpdateStatusText("CA public certificate is not loaded", true);
                    return false;
                }

                Org.BouncyCastle.X509.X509Certificate caCert = _caPublicCertificates[0].Certificate;
                string subjectHash = EdBcTlsUtilities.CreateSubjectHash(caCert);
                if (string.IsNullOrEmpty(subjectHash))
                {
                    UpdateStatusText("CA public certificate subject hash is empty", true);
                    return false;
                }

                string caFileName = EdBcTlsUtilities.GetCaCertFileName(caCert, trustStoreFolder);
                if (caFileName == null)
                {
                    UpdateStatusText("Trusted folder reading failed", true);
                    return false;
                }

                string fileNameHash = Path.GetFileNameWithoutExtension(caFileName);
                if (!string.IsNullOrEmpty(fileNameHash))
                {
                    if (string.Compare(fileNameHash, subjectHash, StringComparison.Ordinal) != 0)
                    {
                        UpdateStatusText($"Trusted CA certificate file name hash mismatch: {fileNameHash} != {subjectHash}", true);
                        return false;
                    }

                    File.Delete(caFileName);
                    string currentCaFileName = Path.GetFileName(caFileName);
                    UpdateStatusText($"Trusted CA certificate uninstalled: {currentCaFileName}", true);
                }

                return true;
            }
            catch (Exception e)
            {
                UpdateStatusText($"Uninstall CA certificate exception: {e.Message}", true);
                return false;
            }
        }

        private bool ModifyClientConfiguration(string clientConfigFile)
        {
            try
            {
                if (string.IsNullOrEmpty(_clientConfigText))
                {
                    UpdateStatusText("Client configuration is not loaded", true);
                    return false;
                }

                XDocument xDoc = XDocument.Parse(_clientConfigText);
                List<XElement> environments = xDoc.Root?.Elements().Where(p => p.Name.LocalName == "Environments").ToList();
                if (environments == null || environments.Count != 1)
                {
                    UpdateStatusText("Client configuration contains no Environments nodes", true);
                    return false;
                }

                List<XElement> environment = environments[0]?.Elements().Where(p => p.Name.LocalName == "Environment").ToList();
                if (environment == null || environment.Count == 0)
                {
                    UpdateStatusText("Client configuration contains no Environment nodes", true);
                    return false;
                }

                bool xmlModified = false;
                foreach (XElement envElement in environment)
                {
                    List<XElement> nameElements = envElement.Elements().Where(p => p.Name.LocalName == "Name").ToList();
                    if (nameElements.Count != 1)
                    {
                        continue;
                    }

                    string envName = nameElements[0].Value.Trim();
                    if (string.IsNullOrEmpty(envName) || string.Compare(envName, "Localhost", StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        continue; // Skip environments that are not for ISPI Admin Client
                    }

                    List<XElement> servicesElements = envElement.Elements().Where(p => p.Name.LocalName == "ServerAddressServices").ToList();
                    if (servicesElements.Count != 1)
                    {
                        continue; // Skip if no services address is found
                    }

                    string servicesValue = servicesElements[0].Value;
                    if (!string.IsNullOrEmpty(servicesValue))
                    {
                        servicesElements[0].Value = string.Empty;
                        xmlModified = true;
                    }
                    break;
                }

                if (!xmlModified)
                {
                    UpdateStatusText("Client configuration not modified", true);
                    return true; // No modification needed
                }

                string bakFile = clientConfigFile + ".bak";
                if (!File.Exists(bakFile))
                {
                    File.Copy(clientConfigFile, bakFile, true);
                    UpdateStatusText("Client config backup created", true);
                }

                // Save the modified XML document
                string xmlText;
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Encoding = Encoding.UTF8;
                settings.Indent = false;
                settings.OmitXmlDeclaration = true;
                settings.NewLineOnAttributes = false;
                using (StringWriter sw = new StringWriter())
                {
                    using (XmlWriter xw = XmlWriter.Create(sw, settings))
                    {
                        xDoc.Save(xw);
                    }
                    xmlText = sw.ToString();
                }

                if (string.IsNullOrEmpty(xmlText))
                {
                    UpdateStatusText("Client configuration XML is empty", true);
                    return false;
                }

                if (!PsdzClient.Utility.Encryption.EncryptFile(xmlText, clientConfigFile))
                {
                    UpdateStatusText("Encrypt client configuration failed", true);
                    return false;
                }

                if (!SetFileFullAccessControl(clientConfigFile))
                {
                    UpdateStatusText("Failed to set full access control for client configuration file", true);
                    return false;
                }

                UpdateStatusText("Client configuration modified", true);
                return true;
            }
            catch (Exception e)
            {
                UpdateStatusText($"Modify client configuration exception: {e.Message}", true);
                return false;
            }
        }

        private bool RevertClientConfiguration(string clientConfigFile)
        {
            try
            {
                string bakFile = clientConfigFile + ".bak";
                if (File.Exists(bakFile))
                {
                    File.Move(bakFile, clientConfigFile, true);
                    UpdateStatusText("Client configuration backup restored", true);
                }
                else
                {
                    UpdateStatusText("No Client configuration backup found", true);
                }

                return true;
            }
            catch (Exception e)
            {
                UpdateStatusText($"Revert client configuration exception: {e.Message}", true);
                return false;
            }
        }


        // Using the function from PsdzClient.Utility.Encryption fails (.NetFramework)
        private bool SetFileFullAccessControl(string fileName)
        {
            try
            {
                FileInfo fInfo = new FileInfo(fileName);
                FileSecurity accessControl = fInfo.GetAccessControl();
                accessControl.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, AccessControlType.Allow));
                fInfo.SetAccessControl(accessControl);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private Org.BouncyCastle.X509.X509Certificate LoadIstaSubCaCert(bool forceUpdate)
        {
            try
            {
                if (_caPublicCertificates == null || _caPublicCertificates.Count < 1)
                {
                    UpdateStatusText("CA public certificate is not loaded", true);
                    return null;
                }

                if (_istaPublicCertificates == null || _istaPublicCertificates.Count < 1)
                {
                    UpdateStatusText("ISTA public certificate is not loaded", true);
                    return null;
                }

                AsymmetricKeyParameter istaPublicKey = _istaPublicCertificates[0].Certificate.GetPublicKey();
                if (istaPublicKey == null)
                {
                    UpdateStatusText("ISTA public key is not found", true);
                    return null;
                }

                Org.BouncyCastle.X509.X509Certificate issuerCert = _caPublicCertificates[0].Certificate;
                UpdateStatusText($"CA certificate valid until: {issuerCert.NotAfter.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}", true);

                X509Certificate2 caCert = null;
                X509Certificate2 subCaCert = null;
                Org.BouncyCastle.X509.X509Certificate x509CaCert = null;
                Org.BouncyCastle.X509.X509Certificate x509SubCaCert = null;

                if (!forceUpdate)
                {
                    string thumbprintCa = EdSec4Diag.GetIstaConfigString(EdSec4Diag.S29ThumbprintCa);
                    string thumbprintSubCa = EdSec4Diag.GetIstaConfigString(EdSec4Diag.S29ThumbprintSubCa);
                    if (!string.IsNullOrEmpty(thumbprintCa) && !string.IsNullOrEmpty(thumbprintSubCa))
                    {
                        caCert = EdSec4Diag.GetCertificateFromStoreByThumbprint(thumbprintCa);
                        subCaCert = EdSec4Diag.GetCertificateFromStoreByThumbprint(thumbprintSubCa);
                    }

                    if (caCert != null && subCaCert != null)
                    {
                        bool certValid = true;
                        x509CaCert = new X509CertificateParser().ReadCertificate(caCert.GetRawCertData());
                        x509SubCaCert = new X509CertificateParser().ReadCertificate(subCaCert.GetRawCertData());

                        if (!issuerCert.GetPublicKey().Equals(x509CaCert.GetPublicKey()))
                        {
                            UpdateStatusText("CA certificate public key does not match CA public certificate", true);
                            certValid = false;
                        }

                        if (!x509SubCaCert.GetPublicKey().Equals(istaPublicKey))
                        {
                            UpdateStatusText("SubCA certificate public key does not match ISTA public key", true);
                            certValid = false;
                        }

                        if (DateTime.UtcNow > subCaCert.NotAfter.AddMonths(-1))
                        {
                            UpdateStatusText($"SubCA certificate remaining time too short: {subCaCert.NotAfter.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}", true);
                            certValid = false;
                        }

                        List<Org.BouncyCastle.X509.X509Certificate> x509CertChain = new List<Org.BouncyCastle.X509.X509Certificate>();
                        x509CertChain.Add(x509SubCaCert);
                        x509CertChain.Add(x509CaCert);

                        List<Org.BouncyCastle.X509.X509Certificate> rootCerts = new List<Org.BouncyCastle.X509.X509Certificate>();
                        foreach (X509CertificateEntry caPublicCertificate in _caPublicCertificates)
                        {
                            Org.BouncyCastle.X509.X509Certificate cert = caPublicCertificate.Certificate;
                            if (cert != null)
                            {
                                rootCerts.Add(cert);
                            }
                        }

                        if (!EdBcTlsUtilities.ValidateCertChain(x509CertChain, rootCerts))
                        {
                            UpdateStatusText("SubCA certificate chain validation failed", true);
                            certValid = false;
                        }

                        if (!certValid)
                        {
                            x509CaCert = null;
                            x509SubCaCert = null;
                        }
                    }
                }

                if (x509CaCert == null || x509SubCaCert == null)
                {
                    UpdateStatusText("Creating new SubCA certificate", true);
                    x509SubCaCert = CreateIstaSubCaCert();
                }

                if (x509SubCaCert == null)
                {
                    UpdateStatusText("Failed to create SubCA certificate", true);
                    return null;
                }

                UpdateStatusText($"SubCA certificate valid until: {x509SubCaCert.NotAfter.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}", true);
                UpdateStatusText("SubCA certificates loaded", true);
                return x509SubCaCert;
            }
            catch (Exception e)
            {
                UpdateStatusText($"Load SubCA certificate exception: {e.Message}", true);
                return null;
            }
        }

        private Org.BouncyCastle.X509.X509Certificate CreateIstaSubCaCert()
        {
            try
            {
                if (_caPublicCertificates == null || _caPublicCertificates.Count < 1)
                {
                    UpdateStatusText("CA public certificate is not loaded", true);
                    return null;
                }

                if (_caKeyResource == null)
                {
                    UpdateStatusText("CA private key is not loaded", true);
                    return null;
                }

                if (_istaPublicCertificates == null || _istaPublicCertificates.Count < 1)
                {
                    UpdateStatusText("ISTA public certificate is not loaded", true);
                    return null;
                }

                AsymmetricKeyParameter istaPublicKey = _istaPublicCertificates[0].Certificate.GetPublicKey();
                if (istaPublicKey == null)
                {
                    UpdateStatusText($"ISTA public key is not found", true);
                    return null;
                }

                Org.BouncyCastle.X509.X509Certificate issuerCert = _caPublicCertificates[0].Certificate;
                X509Certificate2 subCaCert = EdSec4Diag.GenerateCertificate(issuerCert, istaPublicKey, _caKeyResource, EdSec4Diag.S29BmwCnName, null, true);
                if (subCaCert == null)
                {
                    UpdateStatusText("Failed to generate SubCA certificate", true);
                    return null;
                }

                Org.BouncyCastle.X509.X509Certificate x509SubCaCert = new X509CertificateParser().ReadCertificate(subCaCert.GetRawCertData());
                List<Org.BouncyCastle.X509.X509Certificate> installCerts = new List<Org.BouncyCastle.X509.X509Certificate>()
                {
                    issuerCert,
                    x509SubCaCert,
                };

                if (!EdSec4Diag.InstallCertificates(installCerts))
                {
                    UpdateStatusText("Failed to install certificates", true);
                    EdSec4Diag.SetIstaConfigString(EdSec4Diag.S29ThumbprintCa);
                    EdSec4Diag.SetIstaConfigString(EdSec4Diag.S29ThumbprintSubCa);
                    return null;
                }

                X509Certificate2 caCert = new X509Certificate2(issuerCert.GetEncoded());
                if (!EdSec4Diag.SetIstaConfigString(EdSec4Diag.S29ThumbprintCa, caCert.Thumbprint) ||
                    !EdSec4Diag.SetIstaConfigString(EdSec4Diag.S29ThumbprintSubCa, subCaCert.Thumbprint))
                {
                    UpdateStatusText("Failed to set CA thumbprints in ISTA config", true);
                    return null;
                }

                UpdateStatusText("CA thumbprints installed in ISTA config", true);
                return x509SubCaCert;
            }
            catch (Exception e)
            {
                UpdateStatusText($"Install SubCA certificate exception: {e.Message}", true);
                return null;
            }
        }

        private bool ConvertJsonRequestFile(Org.BouncyCastle.X509.X509Certificate x509SubCaCert, string jsonRequestFile, string jsonResponseFolder, string certOutputFolder)
        {
            try
            {
                string baseJsonFile = Path.GetFileName(jsonRequestFile);
                UpdateStatusText($"Converting request file: {baseJsonFile}", true);

                if (!File.Exists(jsonRequestFile))
                {
                    UpdateStatusText($"Request file does not exist: {baseJsonFile}", true);
                    return false;
                }

                EdSec4Diag.Sec4DiagRequestData requestData;
                using (StreamReader file = File.OpenText(jsonRequestFile))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    requestData = serializer.Deserialize(file, typeof(EdSec4Diag.Sec4DiagRequestData)) as EdSec4Diag.Sec4DiagRequestData;
                }

                if (requestData == null)
                {
                    UpdateStatusText($"Failed to deserialize request file: {baseJsonFile}", true);
                    return false;
                }

                string vin = requestData.Vin17;
                if (string.IsNullOrWhiteSpace(vin))
                {
                    UpdateStatusText($"VIN is empty in request file: {baseJsonFile}", true);
                    return false;
                }

                string vin17 = vin.Trim().ToUpperInvariant();
                string publicKey = requestData.PublicKey;
                if (string.IsNullOrWhiteSpace(publicKey))
                {
                    UpdateStatusText($"Public key is empty in request file: {baseJsonFile}", true);
                    return false;
                }

                string certReqProfile = requestData.CertReqProfile;
                if (string.IsNullOrWhiteSpace(certReqProfile))
                {
                    UpdateStatusText($"Certificate request profile is empty in request file: {baseJsonFile}", true);
                    return false;
                }

                AsymmetricKeyParameter publicKeyParameter = EdBcTlsUtilities.ConvertPemToPublicKey(publicKey);
                if (publicKeyParameter == null)
                {
                    UpdateStatusText($"Failed to convert public key in request file: {baseJsonFile}", true);
                    return false;
                }

                string jsonResponseFileName = $"ResponseContainer_service-29-{certReqProfile}-{vin17}.json";
                string jsonResponseFile = Path.Combine(jsonResponseFolder, jsonResponseFileName);
                if (!GenerateCertificate(x509SubCaCert, publicKeyParameter, vin17, certOutputFolder, jsonResponseFile))
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                UpdateStatusText($"Convert request file exception: {ex.Message}", true);
                return false;
            }
        }

        protected bool GenerateCertificate(Org.BouncyCastle.X509.X509Certificate x509SubCaCert, AsymmetricKeyParameter publicKeyParameter, string vehicleVin, string certOutputFolder, string jsonResponseFile = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(vehicleVin))
                {
                    UpdateStatusText("Vehicle VIN is empty", true);
                    return false;
                }

                string vin17 = vehicleVin.Trim().ToUpperInvariant();
                UpdateStatusText($"VIN: {vin17}", true);
                if (publicKeyParameter == null)
                {
                    UpdateStatusText("Public key is not provided", true);
                    return false;
                }

                if (_caPublicCertificates == null || _caPublicCertificates.Count < 1)
                {
                    UpdateStatusText("CA public certificate is not loaded", true);
                    return false;
                }

                if (_caKeyResource == null)
                {
                    UpdateStatusText("CA private key is not loaded", true);
                    return false;
                }

                if (_istaPublicCertificates == null || _istaPublicCertificates.Count < 1)
                {
                    UpdateStatusText($"ISTA public certificate is not loaded", true);
                    return false;
                }

                AsymmetricKeyParameter istaPublicKey = _istaPublicCertificates[0].Certificate.GetPublicKey();
                if (istaPublicKey == null)
                {
                    UpdateStatusText("ISTA public key is not found", true);
                    return false;
                }

                X509Certificate2 s29Cert = EdSec4Diag.GenerateCertificate(x509SubCaCert, publicKeyParameter, _istaKeyResource, EdSec4Diag.S29IstaCnName, vin17);
                if (s29Cert == null)
                {
                    UpdateStatusText($"Failed to generate certificate for VIN: {vin17}", true);
                    return false;
                }

                Org.BouncyCastle.X509.X509Certificate x509s29Cert = new X509CertificateParser().ReadCertificate(s29Cert.GetRawCertData());
                List<Org.BouncyCastle.X509.X509Certificate> x509CertChain = new List<Org.BouncyCastle.X509.X509Certificate>();
                x509CertChain.Add(x509s29Cert);
                x509CertChain.Add(x509SubCaCert);

                List<Org.BouncyCastle.X509.X509Certificate> rootCerts = new List<Org.BouncyCastle.X509.X509Certificate>();
                foreach (X509CertificateEntry caPublicCertificate in _caPublicCertificates)
                {
                    Org.BouncyCastle.X509.X509Certificate cert = caPublicCertificate.Certificate;
                    if (cert != null)
                    {
                        x509CertChain.Add(cert);
                        rootCerts.Add(cert);
                    }
                }

                if (!EdBcTlsUtilities.ValidateCertChain(x509CertChain, rootCerts))
                {
                    UpdateStatusText($"Certificate chain validation failed for VIN: {vin17}", true);
                    return false;
                }

                string s29CertData = Convert.ToBase64String(s29Cert.GetRawCertData());

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(EdBcTlsUtilities.BeginCertificate);
                stringBuilder.AppendLine(s29CertData);
                stringBuilder.AppendLine(EdBcTlsUtilities.EndCertificate);

                List<string> certChain = new List<string>();
                string subCaData = Convert.ToBase64String(x509SubCaCert.GetEncoded());
                stringBuilder.AppendLine(EdBcTlsUtilities.BeginCertificate);
                stringBuilder.AppendLine(subCaData);
                stringBuilder.AppendLine(EdBcTlsUtilities.EndCertificate);

                foreach (X509CertificateEntry caPublicCertificate in _caPublicCertificates)
                {
                    string certData = Convert.ToBase64String(caPublicCertificate.Certificate.GetEncoded());

                    stringBuilder.AppendLine(EdBcTlsUtilities.BeginCertificate);
                    stringBuilder.AppendLine(certData);
                    stringBuilder.AppendLine(EdBcTlsUtilities.EndCertificate);

                    certChain.Add(certData);
                }

                string certContent = stringBuilder.ToString();
                string outputCertFileName = $"S29-{vin17}.pem";
                string outputCertFile = Path.Combine(certOutputFolder, outputCertFileName);
                File.WriteAllText(outputCertFile, certContent);

                UpdateStatusText($"Certificate stored: {outputCertFileName}", true);

                if (!string.IsNullOrEmpty(jsonResponseFile))
                {
                    EdSec4Diag.Sec4DiagResponseData responseData = new EdSec4Diag.Sec4DiagResponseData
                    {
                        Vin17 = vin17,
                        Certificate = s29CertData,
                        CertificateChain = certChain.ToArray()
                    };

                    JsonSerializer serializer = new JsonSerializer();
                    serializer.NullValueHandling = NullValueHandling.Ignore;
                    serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
                    serializer.StringEscapeHandling = StringEscapeHandling.EscapeHtml;

                    using (StreamWriter sw = new StreamWriter(jsonResponseFile))
                    {
                        using (JsonWriter writer = new JsonTextWriter(sw))
                        {
                            serializer.Serialize(writer, responseData);
                        }
                    }

                    string jsonResponseFileName = Path.GetFileName(jsonResponseFile);
                    UpdateStatusText($"Response file created: {jsonResponseFileName}", true);
                }

                return true;
            }
            catch (Exception ex)
            {
                UpdateStatusText($"Convert request file exception: {ex.Message}", true);
                return false;
            }
        }

        protected bool InstallCertificates(string caCertsFile, string trustStoreFolder, string jsonRequestFolder, string jsonResponseFolder, string certOutputFolder, string clientConfigFile, string vehicleVin, bool forceUpdate = false)
        {
            try
            {
                UpdateStatusText(string.Empty);

                Org.BouncyCastle.X509.X509Certificate x509SubCaCert = LoadIstaSubCaCert(forceUpdate);
                if (x509SubCaCert == null)
                {
                    UpdateStatusText("Failed to create SubCA certificate", true);
                    return false;
                }

                if (!string.IsNullOrEmpty(caCertsFile))
                {
                    if (!InstallCaCert(caCertsFile))
                    {
                        UpdateStatusText("Installing CA certificate failed", true);
                        return false;
                    }
                }

                if (!string.IsNullOrEmpty(trustStoreFolder))
                {
                    if (!InstallTrustedCaCert(trustStoreFolder))
                    {
                        UpdateStatusText("Installing trusted CA certificate failed", true);
                        return false;
                    }
                }

                if (!string.IsNullOrEmpty(clientConfigFile))
                {
                    if (!ModifyClientConfiguration(clientConfigFile))
                    {
                        UpdateStatusText("Modifying client configuration failed", true);
                        return false;
                    }
                }

                if (string.IsNullOrEmpty(jsonRequestFolder) || !Directory.Exists(jsonRequestFolder))
                {
                    UpdateStatusText($"Request folder is not existing: {jsonRequestFolder}", true);
                    return false;
                }

                if (string.IsNullOrEmpty(jsonResponseFolder) || !Directory.Exists(jsonResponseFolder))
                {
                    UpdateStatusText($"Response folder is not existing: {jsonResponseFolder}", true);
                    return false;
                }

                if (string.IsNullOrEmpty(certOutputFolder) || !Directory.Exists(certOutputFolder))
                {
                    UpdateStatusText($"Output folder is not existing: {certOutputFolder}", true);
                    return false;
                }

                if (string.IsNullOrEmpty(vehicleVin))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(jsonRequestFolder);
                    FileInfo[] files = directoryInfo.GetFiles().OrderBy(p => p.LastWriteTime).ToArray();
                    foreach (FileInfo fileInfo in files)
                    {
                        string jsonFile = fileInfo.FullName;
                        string baseFileName = Path.GetFileName(jsonFile);
                        if (!baseFileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (string.Compare(baseFileName, "template.json", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            continue;
                        }

                        UpdateStatusText(string.Empty, true);
                        ConvertJsonRequestFile(x509SubCaCert, jsonFile, jsonResponseFolder, certOutputFolder);
                    }
                }
                else
                {
                    string machineName = EdSec4Diag.GetMachineName();
                    string machinePublicFile = Path.Combine(certOutputFolder, machineName + EdSec4Diag.S29MachinePublicName);
                    if (!File.Exists(machinePublicFile))
                    {
                        UpdateStatusText($"Machine public key file does not existing: {machinePublicFile}", true);
                        UpdateStatusText("Execute EDIABAS or EdiabasLib in SSL mode first to generate the key files.", true);
                        return false;
                    }

                    AsymmetricKeyParameter publicKeyParameter = EdBcTlsUtilities.LoadPemObject(machinePublicFile) as AsymmetricKeyParameter;
                    if (publicKeyParameter == null)
                    {
                        UpdateStatusText($"Failed to load public key from file: {machinePublicFile}", true);
                        return false;
                    }

                    GenerateCertificate(x509SubCaCert, publicKeyParameter, vehicleVin, certOutputFolder);
                }

                return true;
            }
            catch (Exception ex)
            {
                UpdateStatusText($"Convert request files exception: {ex.Message}", true);
                return false;
            }
        }

        protected bool UninstallCertificates(string caCertsFile, string trustStoreFolder, string jsonRequestFolder, string jsonResponseFolder, string certOutputFolder, string clientConfigFile)
        {
            try
            {
                UpdateStatusText(string.Empty);

                string thumbprintCa = EdSec4Diag.GetIstaConfigString(EdSec4Diag.S29ThumbprintCa);
                string thumbprintSubCa = EdSec4Diag.GetIstaConfigString(EdSec4Diag.S29ThumbprintSubCa);
                if (!string.IsNullOrEmpty(thumbprintCa) || !string.IsNullOrEmpty(thumbprintSubCa))
                {
                    if (EdSec4Diag.DeleteCertificateByThumbprint(thumbprintCa))
                    {
                        UpdateStatusText("CA certificate deleted from store", true);
                    }
                    else
                    {
                        UpdateStatusText($"Failed to delete CA certificate with thumbprint: {thumbprintCa}", true);
                    }

                    if (EdSec4Diag.DeleteCertificateByThumbprint(thumbprintSubCa))
                    {
                        UpdateStatusText("SubCA certificate deleted from store", true);
                    }
                    else
                    {
                        UpdateStatusText($"Failed to delete SubCA certificate with thumbprint: {thumbprintSubCa}", true);
                    }

                    EdSec4Diag.SetIstaConfigString(EdSec4Diag.S29ThumbprintCa);
                    EdSec4Diag.SetIstaConfigString(EdSec4Diag.S29ThumbprintSubCa);
                    UpdateStatusText("CA thumbprints cleared in ISTA config", true);
                }

                if (!string.IsNullOrEmpty(caCertsFile))
                {
                    if (!UninstallCaCert(caCertsFile))
                    {
                        UpdateStatusText("Uninstalling CA certificate failed", true);
                        return false;
                    }
                }

                if (!string.IsNullOrEmpty(clientConfigFile))
                {
                    if (!RevertClientConfiguration(clientConfigFile))
                    {
                        UpdateStatusText("Reverting client configuration failed", true);
                        return false;
                    }
                }

                if (!string.IsNullOrEmpty(trustStoreFolder))
                {
                    if (!UninstallTrustedCaCert(trustStoreFolder))
                    {
                        UpdateStatusText("Uninstalling trusted CA certificate failed", true);
                        return false;
                    }
                }

                if (string.IsNullOrEmpty(jsonRequestFolder) || !Directory.Exists(jsonRequestFolder))
                {
                    UpdateStatusText($"Request folder is not existing: {jsonRequestFolder}", true);
                    return false;
                }

                if (string.IsNullOrEmpty(jsonResponseFolder) || !Directory.Exists(jsonResponseFolder))
                {
                    UpdateStatusText($"Response folder is not existing: {jsonResponseFolder}", true);
                    return false;
                }

                if (string.IsNullOrEmpty(certOutputFolder) || !Directory.Exists(certOutputFolder))
                {
                    UpdateStatusText($"Output folder is not existing: {certOutputFolder}", true);
                    return false;
                }

                string[] requestFiles = Directory.GetFiles(jsonRequestFolder, "*.json", SearchOption.TopDirectoryOnly);
                foreach (string jsonFile in requestFiles)
                {
                    string baseFileName = Path.GetFileName(jsonFile);
                    if (string.Compare(baseFileName, "template.json", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        continue;
                    }

                    File.Delete(jsonFile);
                    UpdateStatusText($"Request file deleted: {baseFileName}", true);
                }

                string[] responseFiles = Directory.GetFiles(jsonResponseFolder, "*.json", SearchOption.TopDirectoryOnly);
                foreach (string jsonFile in responseFiles)
                {
                    string baseFileName = Path.GetFileName(jsonFile);
                    File.Delete(jsonFile);
                    UpdateStatusText($"Response file deleted: {baseFileName}", true);
                }

                string[] certFiles = Directory.GetFiles(certOutputFolder, "S29-*.pem", SearchOption.TopDirectoryOnly);
                foreach (string certFile in certFiles)
                {
                    string baseFileName = Path.GetFileName(certFile);
                    File.Delete(certFile);
                    UpdateStatusText($"Certificate file deleted: {baseFileName}", true);
                }

                return true;
            }
            catch (Exception ex)
            {
                UpdateStatusText($"Convert request files exception: {ex.Message}", true);
                return false;
            }
        }

        private void buttonSelectCaKeyFile_Click(object sender, EventArgs e)
        {
            string initDir = _appDir;
            string certFile = textBoxCaCeyFile.Text;
            string fileName = string.Empty;

            if (File.Exists(certFile))
            {
                fileName = Path.GetFileName(certFile);
                initDir = Path.GetDirectoryName(certFile);
            }

            openCertFileDialog.FileName = fileName;
            openCertFileDialog.InitialDirectory = initDir ?? string.Empty;
            DialogResult result = openCertFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBoxCaCeyFile.Text = openCertFileDialog.FileName;
                UpdateDisplay();
            }
        }

        private void buttonSelectIstaKeyFile_Click(object sender, EventArgs e)
        {
            string initDir = _appDir;
            string certFile = textBoxIstaKeyFile.Text;
            string fileName = string.Empty;

            if (File.Exists(certFile))
            {
                fileName = Path.GetFileName(certFile);
                initDir = Path.GetDirectoryName(certFile);
            }

            openIstaKeyFileDialog.FileName = fileName;
            openIstaKeyFileDialog.InitialDirectory = initDir ?? string.Empty;
            DialogResult result = openIstaKeyFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBoxIstaKeyFile.Text = openIstaKeyFileDialog.FileName;
                UpdateDisplay();
            }
        }

        private void buttonSelectCaCertsFile_Click(object sender, EventArgs e)
        {
            string initDir = _appDir;
            string certFile = textBoxCaCertsFile.Text;
            string fileName = string.Empty;
            if (File.Exists(certFile))
            {
                fileName = Path.GetFileName(certFile);
                initDir = Path.GetDirectoryName(certFile);
            }

            openCaCertsFileDialog.FileName = fileName;
            openCaCertsFileDialog.InitialDirectory = initDir ?? string.Empty;
            DialogResult result = openCaCertsFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBoxCaCertsFile.Text = openCaCertsFileDialog.FileName;
                UpdateDisplay();
            }
        }

        private void buttonSelectSecurityFolder_Click(object sender, EventArgs e)
        {
            string initDir = _appDir;
            string requestFolder = textBoxSecurityFolder.Text;

            if (Directory.Exists(requestFolder))
            {
                initDir = requestFolder;
            }
            else
            {
                requestFolder = string.Empty;
            }

            folderBrowserDialog.InitialDirectory = initDir;
            folderBrowserDialog.SelectedPath = requestFolder;
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBoxSecurityFolder.Text = folderBrowserDialog.SelectedPath;
                SyncFolders(textBoxSecurityFolder.Text);
                UpdateDisplay();
            }
        }

        private void buttonSelectJsonRequestFolder_Click(object sender, EventArgs e)
        {
            string initDir = _appDir;
            string requestFolder = textBoxJsonRequestFolder.Text;

            if (Directory.Exists(requestFolder))
            {
                initDir = requestFolder;
            }
            else
            {
                requestFolder = string.Empty;
            }

            folderBrowserDialog.InitialDirectory = initDir;
            folderBrowserDialog.SelectedPath = requestFolder;
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBoxJsonRequestFolder.Text = folderBrowserDialog.SelectedPath;
                UpdateDisplay();
            }
        }

        private void buttonSelectJsonResponseFolder_Click(object sender, EventArgs e)
        {
            string initDir = _appDir;
            string responseFolder = textBoxJsonResponseFolder.Text;

            if (Directory.Exists(responseFolder))
            {
                initDir = responseFolder;
            }
            else
            {
                responseFolder = string.Empty;
            }

            folderBrowserDialog.InitialDirectory = initDir;
            folderBrowserDialog.SelectedPath = responseFolder;
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBoxJsonResponseFolder.Text = folderBrowserDialog.SelectedPath;
                UpdateDisplay();
            }
        }

        private void buttonSelectCertOutputFolder_Click(object sender, EventArgs e)
        {
            string initDir = _appDir;
            string outputFolder = textBoxCertOutputFolder.Text;

            if (Directory.Exists(outputFolder))
            {
                initDir = outputFolder;
            }
            else
            {
                outputFolder = string.Empty;
            }

            folderBrowserDialog.InitialDirectory = initDir;
            folderBrowserDialog.SelectedPath = outputFolder;
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBoxCertOutputFolder.Text = folderBrowserDialog.SelectedPath;
                UpdateDisplay();
            }
        }

        private void buttonSelectTrustStoreFolder_Click(object sender, EventArgs e)
        {
            string initDir = _appDir;
            string trustStoreFolder = textBoxTrustStoreFolder.Text;

            if (Directory.Exists(trustStoreFolder))
            {
                initDir = trustStoreFolder;
            }
            else
            {
                trustStoreFolder = string.Empty;
            }

            folderBrowserDialog.InitialDirectory = initDir;
            folderBrowserDialog.SelectedPath = trustStoreFolder;
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBoxTrustStoreFolder.Text = folderBrowserDialog.SelectedPath;
                UpdateDisplay();
            }
        }

        private void buttonSearchVehicles_Click(object sender, EventArgs e)
        {
            UpdateStatusText("Searching for DoIP vehicles...");
            comboBoxVinList.Items.Clear();

            SearchVehiclesTask().ContinueWith(task =>
            {
                _taskActive = false;
                List<string> vinList = new List<string>();
                BeginInvoke((Action)(() =>
                {
                    List<EdInterfaceEnet.EnetConnection> detectedVehicles = task.Result;
                    if (detectedVehicles != null)
                    {
                        foreach (EdInterfaceEnet.EnetConnection enetConnection in detectedVehicles)
                        {
                            if (!string.IsNullOrEmpty(enetConnection.Vin))
                            {
                                vinList.Add(enetConnection.Vin);
                            }
                        }
                    }

                    comboBoxVinList.BeginUpdate();
                    comboBoxVinList.Items.Clear();
                    comboBoxVinList.Items.Add(string.Empty);
                    foreach (string vin in vinList.OrderBy(v => v))
                    {
                        if (!comboBoxVinList.Items.Contains(vin))
                        {
                            comboBoxVinList.Items.Add(vin);
                        }
                    }

                    if (comboBoxVinList.Items.Count > 1)
                    {
                        comboBoxVinList.SelectedIndex = 1;
                    }

                    comboBoxVinList.EndUpdate();
                    UpdateStatusText($"Found {vinList.Count} DoIP vehicles", true);

                    UpdateDisplay();
                }));
            });

            _taskActive = true;
            UpdateDisplay();
        }

        private void buttonInstall_Click(object sender, EventArgs e)
        {
            string vehicleVin = comboBoxVinList.SelectedItem as string;
            if (InstallCertificates(textBoxCaCertsFile.Text, textBoxTrustStoreFolder.Text, textBoxJsonRequestFolder.Text, textBoxJsonResponseFolder.Text, textBoxCertOutputFolder.Text, textBoxClientConfigurationFile.Text, vehicleVin, checkBoxForceCreate.Checked))
            {
                checkBoxForceCreate.Checked = false;
            }
        }

        private void buttonUninstall_Click(object sender, EventArgs e)
        {
            UninstallCertificates(textBoxCaCertsFile.Text, textBoxTrustStoreFolder.Text, textBoxJsonRequestFolder.Text, textBoxJsonResponseFolder.Text, textBoxCertOutputFolder.Text, textBoxClientConfigurationFile.Text);
        }

        private void buttonResetSettings_Click(object sender, EventArgs e)
        {
            ValidateSetting(true);
            UpdateDisplay();
        }
    }
}

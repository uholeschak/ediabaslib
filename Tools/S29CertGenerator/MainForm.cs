using EdiabasLib;
using Microsoft.Win32;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;

namespace S29CertGenerator
{
    public partial class MainForm : Form
    {
        private string _appDir;
        private string _ediabasPath;
        private AsymmetricKeyParameter _caKeyResource;
        private List<X509CertificateEntry> _caPublicCertificates;
        private readonly byte[] roleMask = new byte[] { 0, 0, 5, 75 };
        public const string Service29CnName = "Service29-EDIABAS-S29";
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

            LoadSettings();
            if (!IsSettingValid(true))
            {
                if (!string.IsNullOrEmpty(_ediabasPath))
                {
                    textBoxS29Folder.Text = Path.Combine(_ediabasPath, EdiabasSecurityDirName, EdiabasS29DirName);
                    SyncFolders(textBoxS29Folder.Text);
                    SetIstaKeyFile(_ediabasPath);
                }
            }
            UpdateStatusText(string.Empty);
            UpdateDisplay();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            StoreSettings();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {

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
                textBoxS29Folder.Text = Properties.Settings.Default.S29Folder;
                textBoxJsonRequestFolder.Text = Properties.Settings.Default.JsonRequestFolder;
                textBoxJsonResponseFolder.Text = Properties.Settings.Default.JsonResponseFolder;
                textBoxCertOutputFolder.Text = Properties.Settings.Default.CertOutputFolder;
                textBoxIstaKeyFile.Text = Properties.Settings.Default.IstaKeyFile;
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
                Properties.Settings.Default.S29Folder = textBoxS29Folder.Text;
                Properties.Settings.Default.JsonRequestFolder = textBoxJsonRequestFolder.Text;
                Properties.Settings.Default.JsonResponseFolder = textBoxJsonResponseFolder.Text;
                Properties.Settings.Default.CertOutputFolder = textBoxCertOutputFolder.Text;
                Properties.Settings.Default.IstaKeyFile = textBoxIstaKeyFile.Text;
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
                bool caKeyValid = LoadCaKey(textBoxCaCeyFile.Text);
                bool isValid = IsSettingValid();
                buttonExecute.Enabled = isValid;

                if (caKeyValid && isValid)
                {
                    buttonExecute.Focus();
                }
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

        private bool IsSettingValid(bool ignoreCaKey = false)
        {
            try
            {
                string caKeyFile = textBoxCaCeyFile.Text.Trim();
                string s29Folder = textBoxS29Folder.Text.Trim();
                string jsonRequestFolder = textBoxJsonRequestFolder.Text.Trim();
                string jsonResponseFolder = textBoxJsonResponseFolder.Text.Trim();
                string certOutputFolder = textBoxCertOutputFolder.Text.Trim();

                if (!ignoreCaKey)
                {
                    if (string.IsNullOrEmpty(caKeyFile) || !File.Exists(caKeyFile))
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

                return true;
            }
            catch (Exception)
            {
                return false;
            }
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

        private bool SyncFolders(string s29Folder)
        {
            if (string.IsNullOrEmpty(s29Folder) || !Directory.Exists(s29Folder))
            {
                return false;
            }

            string jsonRequestFolder = Path.Combine(s29Folder, "JSONRequests");
            string jsonResponseFolder = Path.Combine(s29Folder, "JSONResponses");
            string certOutputFolder = Path.Combine(s29Folder, "Certificates");

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

            return true;
        }

        private bool SetIstaKeyFile(string ediabasPath)
        {
            if (string.IsNullOrEmpty(ediabasPath) || !Directory.Exists(ediabasPath))
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

        private bool LoadCaKey(string caKeyFile)
        {
            _caKeyResource = null;
            _caPublicCertificates = null;

            try
            {
                if (string.IsNullOrEmpty(caKeyFile) || !File.Exists(caKeyFile))
                {
                    return false;
                }

                string publicCert = Path.ChangeExtension(caKeyFile, ".crt");
                if (!File.Exists(publicCert))
                {
                    return false; // Public certificate file does not exist
                }

                AsymmetricKeyParameter privateKeyResource = EdBcTlsUtilities.LoadBcPrivateKeyResource(caKeyFile);
                if (privateKeyResource == null)
                {
                    return false; // Failed to load private key
                }

                List<X509CertificateEntry> publicCertificateEntries = EdBcTlsUtilities.GetCertificateEntries(EdBcTlsUtilities.LoadBcCertificateResources(publicCert));
                if (publicCertificateEntries == null || publicCertificateEntries.Count < 1)
                {
                    return false; // Failed to load public certificates
                }

                if (!publicCertificateEntries[0].Certificate.IsValid(DateTime.UtcNow.AddDays(1.0)))
                {
                    return false; // Public certificate is not valid in the future
                }

                _caKeyResource = privateKeyResource;
                _caPublicCertificates = publicCertificateEntries;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public X509Certificate2 GenerateCertificate(Org.BouncyCastle.X509.X509Certificate issuerCert, AsymmetricKeyParameter publicKey, AsymmetricKeyParameter issuerPrivateKey, string vin)
        {
            X509Name subject = new X509Name($"ST=Production, O=BMW Group, OU=Service29-PKI-SubCA, CN={Service29CnName}, GIVENNAME=" + vin);
            X509V3CertificateGenerator x509V3CertificateGenerator = new X509V3CertificateGenerator();
            x509V3CertificateGenerator.SetPublicKey(publicKey);
            x509V3CertificateGenerator.SetSerialNumber(BigInteger.ProbablePrime(120, new Random()));
            x509V3CertificateGenerator.SetIssuerDN(issuerCert.SubjectDN);
            x509V3CertificateGenerator.SetNotBefore(DateTime.UtcNow.AddMinutes(-5.0));
            x509V3CertificateGenerator.SetNotAfter(DateTime.UtcNow.AddYears(1));
            x509V3CertificateGenerator.SetSubjectDN(subject);
            DerObjectIdentifier oid = new DerObjectIdentifier("1.3.6.1.4.1.513.29.30");
            byte[] contents = new byte[2] { 14, 243 };
            byte[] contents2 = new byte[2] { 14, 244 };
            byte[] contents3 = new byte[2] { 14, 245 };
            DerOctetString element = new DerOctetString(contents);
            DerOctetString element2 = new DerOctetString(contents2);
            DerOctetString element3 = new DerOctetString(contents3);
            DerSet extensionValue = new DerSet(new Asn1EncodableVector { element, element2, element3 });
            x509V3CertificateGenerator.AddExtension(oid, critical: true, extensionValue);
            DerObjectIdentifier oid2 = new DerObjectIdentifier("1.3.6.1.4.1.513.29.10");
            x509V3CertificateGenerator.AddExtension(oid2, critical: true, roleMask);
            x509V3CertificateGenerator.AddExtension(X509Extensions.KeyUsage, critical: false, new KeyUsage(128));
            x509V3CertificateGenerator.AddExtension(X509Extensions.SubjectKeyIdentifier, critical: false, X509ExtensionUtilities.CreateSubjectKeyIdentifier(publicKey));
            x509V3CertificateGenerator.AddExtension(X509Extensions.BasicConstraints, critical: true, new BasicConstraints(cA: false));
            x509V3CertificateGenerator.AddExtension(X509Extensions.AuthorityKeyIdentifier, critical: false, X509ExtensionUtilities.CreateAuthorityKeyIdentifier(issuerCert.GetPublicKey()));
            ISignatureFactory signatureFactory = new Asn1SignatureFactory("SHA512withECDSA", issuerPrivateKey);
            return new X509Certificate2(x509V3CertificateGenerator.Generate(signatureFactory).GetEncoded());
        }

        private static void InstallCertificate(X509Certificate2 cert)
        {
            using (X509Store x509Store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                x509Store.Open(OpenFlags.ReadWrite);
                x509Store.Add(cert);
                x509Store.Close();
            }
        }

        private static void DeleteCertificateBySubjectName(string subjectName)
        {
            if (string.IsNullOrEmpty(subjectName))
            {
                return; // No subject name provided
            }

            X509Store x509Store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            x509Store.Open(OpenFlags.ReadWrite);
            foreach (X509Certificate2 x509Certificate in x509Store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, false))
            {
                x509Store.Remove(x509Certificate);
            }
            x509Store.Close();
        }

        private bool InstallCertificates(List<Org.BouncyCastle.X509.X509Certificate> x509CertChain)
        {
            try
            {
                if (x509CertChain == null || x509CertChain.Count < 1)
                {
                    return false;
                }

                DeleteCertificateBySubjectName(Service29CnName);
                foreach (Org.BouncyCastle.X509.X509Certificate x509Certificate in x509CertChain)
                {
                    X509Certificate2 cert = new X509Certificate2(x509Certificate.GetEncoded());
                    InstallCertificate(cert);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool ConvertJsonRequestFile(string jsonRequestFile, string jsonResponseFolder, string certOutputFolder)
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

                JsonSerializer serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Ignore;
                serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
                serializer.StringEscapeHandling = StringEscapeHandling.EscapeHtml;

                EdSec4Diag.Sec4DiagRequestData requestData;
                using (StreamReader file = File.OpenText(jsonRequestFile))
                {
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
                UpdateStatusText($"VIN: {vin17}", true);
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

                if (_caPublicCertificates == null || _caPublicCertificates.Count < 1)
                {
                    UpdateStatusText($"CA public certificate is not loaded", true);
                    return false;
                }

                if (_caKeyResource == null)
                {
                    UpdateStatusText($"CA private key is not loaded", true);
                    return false;
                }

                Org.BouncyCastle.X509.X509Certificate issuerCert = _caPublicCertificates[0].Certificate;
                X509Certificate2 s29Cert = GenerateCertificate(issuerCert, publicKeyParameter, _caKeyResource, vin17);
                if (s29Cert == null)
                {
                    UpdateStatusText($"Failed to generate certificate for VIN: {vin17}", true);
                    return false;
                }

                List<Org.BouncyCastle.X509.X509Certificate> x509CertChain = new List<Org.BouncyCastle.X509.X509Certificate>();
                x509CertChain.Add(new X509CertificateParser().ReadCertificate(s29Cert.GetRawCertData()));

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
                stringBuilder.AppendLine("-----BEGIN CERTIFICATE-----");
                stringBuilder.AppendLine(s29CertData);
                stringBuilder.AppendLine("-----END CERTIFICATE-----");

                List<string> certChain = new List<string>();
                foreach (X509CertificateEntry caPublicCertificate in _caPublicCertificates)
                {
                    string certData = Convert.ToBase64String(caPublicCertificate.Certificate.GetEncoded());

                    stringBuilder.AppendLine("-----BEGIN CERTIFICATE-----");
                    stringBuilder.AppendLine(certData);
                    stringBuilder.AppendLine("-----END CERTIFICATE-----");

                    certChain.Add(certData);
                }

                string certContent = stringBuilder.ToString();
                string outputCertFileName = $"S29-{vin17}.pem";
                string outputCertFile = Path.Combine(certOutputFolder, outputCertFileName);
                File.WriteAllText(outputCertFile, certContent);

                UpdateStatusText($"Certificate stored: {outputCertFileName}", true);

                EdSec4Diag.Sec4DiagResponseData responseData = new EdSec4Diag.Sec4DiagResponseData
                {
                    Vin17 = vin17,
                    Certificate = s29CertData,
                    CertificateChain = certChain.ToArray()
                };

                string jsonResponseFileName = $"ResponseContainer_service-29-{certReqProfile}-{vin17}.json";
                string jsonResponseFile = Path.Combine(jsonResponseFolder, jsonResponseFileName);
                using (StreamWriter sw = new StreamWriter(jsonResponseFile))
                {
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        serializer.Serialize(writer, responseData);
                    }
                }

                UpdateStatusText($"Response file created: {jsonResponseFileName}", true);

                if (!InstallCertificates(x509CertChain))
                {
                    UpdateStatusText($"Failed to install certificates for VIN: {vin17}", true);
                    return false;
                }

                UpdateStatusText($"Certificates installed for VIN: {vin17}", true);
                return true;
            }
            catch (Exception ex)
            {
                UpdateStatusText($"Convert request file exception: {ex.Message}", true);
                return false;
            }
        }

        protected bool ConvertAllJsonRequestFiles(string jsonRequestFolder, string jsonResponseFolder, string certOutputFolder)
        {
            try
            {
                UpdateStatusText(string.Empty);

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
                    ConvertJsonRequestFile(jsonFile, jsonResponseFolder, certOutputFolder);
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

        private void buttonSelectS29Folder_Click(object sender, EventArgs e)
        {
            string initDir = _appDir;
            string requestFolder = textBoxS29Folder.Text;

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
                textBoxS29Folder.Text = folderBrowserDialog.SelectedPath;
                SyncFolders(textBoxS29Folder.Text);
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

        private void buttonExecute_Click(object sender, EventArgs e)
        {
            ConvertAllJsonRequestFiles(textBoxJsonRequestFolder.Text, textBoxJsonResponseFolder.Text, textBoxCertOutputFolder.Text);
        }
    }
}

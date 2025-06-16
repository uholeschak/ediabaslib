using EdiabasLib;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;
using Org.BouncyCastle.Math;

namespace S29CertGenerator
{
    public partial class MainForm : Form
    {
        private string _appDir;
        private AsymmetricKeyParameter _caKeyResource;
        private List<X509CertificateEntry> _caPublicCertificates;
        private readonly byte[] roleMask = new byte[] { 0, 0, 5, 75 };

        public MainForm()
        {
            InitializeComponent();

            _appDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        private void MainForm_Load(object sender, System.EventArgs e)
        {
            Icon = Properties.Resources.AppIcon;

            LoadSettings();
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
                textBoxJsonRequestFolder.Text = Properties.Settings.Default.JsonRequestFolder;
                textBoxCertOutputFolder.Text = Properties.Settings.Default.CertOutputFolder;
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
                Properties.Settings.Default.JsonRequestFolder = textBoxJsonRequestFolder.Text;
                Properties.Settings.Default.CertOutputFolder = textBoxCertOutputFolder.Text;
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

        private bool IsSettingValid()
        {
            try
            {
                string caKeyFile = textBoxCaCeyFile.Text.Trim();
                string jsonRequestFolder = textBoxJsonRequestFolder.Text.Trim();
                string certOutputFolder = textBoxCertOutputFolder.Text.Trim();

                if (string.IsNullOrEmpty(caKeyFile) || !File.Exists(caKeyFile))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(jsonRequestFolder) || !Directory.Exists(jsonRequestFolder))
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

                if (!publicCertificateEntries[0].Certificate.IsValidNow)
                {
                    return false; // Public certificate is not valid now
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
            X509Name subject = new X509Name("ST=Production, O=BMW Group, OU=Service29-PKI-SubCA, CN=Service29-EDIABAS-S29, GIVENNAME=" + vin);
            X509V3CertificateGenerator x509V3CertificateGenerator = new X509V3CertificateGenerator();
            x509V3CertificateGenerator.SetPublicKey(publicKey);
            x509V3CertificateGenerator.SetSerialNumber(BigInteger.ProbablePrime(120, new Random()));
            x509V3CertificateGenerator.SetIssuerDN(issuerCert.SubjectDN);
            x509V3CertificateGenerator.SetNotBefore(DateTime.UtcNow.AddMinutes(-5.0));
            x509V3CertificateGenerator.SetNotAfter(DateTime.UtcNow.AddDays(4.0));
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
            x509V3CertificateGenerator.AddExtension(X509Extensions.SubjectKeyIdentifier, critical: false, new SubjectKeyIdentifierStructure(publicKey));
            x509V3CertificateGenerator.AddExtension(X509Extensions.BasicConstraints, critical: true, new BasicConstraints(cA: false));
            x509V3CertificateGenerator.AddExtension(X509Extensions.AuthorityKeyIdentifier, critical: false, new AuthorityKeyIdentifierStructure(issuerCert.GetPublicKey()));
            ISignatureFactory signatureFactory = new Asn1SignatureFactory("SHA512withECDSA", issuerPrivateKey);
            return new X509Certificate2(x509V3CertificateGenerator.Generate(signatureFactory).GetEncoded());
        }

        private bool ConvertJsonRequestFile(string jsonRequestFile, string certOutputFolder)
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

                string vin17 = requestData.Vin17;
                if (string.IsNullOrWhiteSpace(vin17))
                {
                    UpdateStatusText($"VIN is empty in request file: {baseJsonFile}", true);
                    return false;
                }

                UpdateStatusText($"VIN: {vin17}", true);
                string publicKey = requestData.PublicKey;
                if (string.IsNullOrWhiteSpace(publicKey))
                {
                    UpdateStatusText($"Public key is empty in request file: {baseJsonFile}", true);
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
                X509Certificate2 generatedCert = GenerateCertificate(issuerCert, publicKeyParameter, _caKeyResource, vin17);
                if (generatedCert == null)
                {
                    UpdateStatusText($"Failed to generate certificate for VIN: {vin17}", true);
                    return false;
                }

                UpdateStatusText($"Generated certificate for VIN: {vin17}", true);
                return true;
            }
            catch (Exception ex)
            {
                UpdateStatusText($"Convert request file exception: {ex.Message}", true);
                return false;
            }
        }

        protected bool ConvertAllJsonRequestFiles(string jsonRequestFolder, string certOutputFolder)
        {
            try
            {
                UpdateStatusText(string.Empty);

                if (string.IsNullOrEmpty(jsonRequestFolder) || !Directory.Exists(jsonRequestFolder))
                {
                    UpdateStatusText($"Request folder is not existing: {jsonRequestFolder}", true);
                    return false;
                }

                if (string.IsNullOrEmpty(certOutputFolder) || !Directory.Exists(certOutputFolder))
                {
                    UpdateStatusText($"Output folder is not existing: {certOutputFolder}", true);
                    return false;
                }

                IEnumerable<string> jsonFiles = Directory.EnumerateFiles(jsonRequestFolder, "*.json", SearchOption.TopDirectoryOnly);
                foreach (string jsonFile in jsonFiles)
                {
                    string baseFileName = Path.GetFileName(jsonFile);
                    if (string.Compare(baseFileName, "template.json", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        continue;
                    }

                    UpdateStatusText(string.Empty, true);
                    ConvertJsonRequestFile(jsonFile, certOutputFolder);
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
            ConvertAllJsonRequestFiles(textBoxJsonRequestFolder.Text, textBoxCertOutputFolder.Text);
        }
    }
}

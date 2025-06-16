using EdiabasLib;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace S29CertGenerator
{
    public partial class MainForm : Form
    {
        private string _appDir;
        private AsymmetricKeyParameter _caKeyResource;
        private List<X509CertificateEntry> _caPublicCertificates;

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

            if (!string.IsNullOrEmpty(text))
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

        }
    }
}

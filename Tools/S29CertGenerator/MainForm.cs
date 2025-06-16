using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace S29CertGenerator
{
    public partial class MainForm : Form
    {
        private string _appDir;

        public MainForm()
        {
            InitializeComponent();

            _appDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        private void MainForm_Load(object sender, System.EventArgs e)
        {
            Icon = Properties.Resources.AppIcon;

            LoadSettings();
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
                Properties.Settings.Default.Save();
                return true;
            }
            catch (Exception e)
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
            }
        }
    }
}

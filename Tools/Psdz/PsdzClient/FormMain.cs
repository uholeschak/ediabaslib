using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PsdzClient.Programming;

namespace PsdzClient
{
    public partial class FormMain : Form
    {
        private ProgrammingService programmingService;

        public FormMain()
        {
            InitializeComponent();
        }

        private void UpdateDisplay()
        {
            bool hostRunning = programmingService != null && programmingService.IsPsdzPsdzServiceHostInitialized();
            buttonStartHost.Enabled = !hostRunning;
            buttonStopHost.Enabled = hostRunning;
        }

        private bool LoadSettings()
        {
            try
            {
                textBoxIstaFolder.Text = Properties.Settings.Default.IstaFolder;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private bool StoreSettings()
        {
            try
            {
                Properties.Settings.Default.IstaFolder = textBoxIstaFolder.Text;
                Properties.Settings.Default.Save();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private void StopProgrammingService()
        {
            if (programmingService != null)
            {
                programmingService.CloseConnectionsToPsdzHost();
                programmingService = null;
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonAbort_Click(object sender, EventArgs e)
        {

        }

        private void buttonIstaFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialogIsta.SelectedPath = textBoxIstaFolder.Text;
            DialogResult result = folderBrowserDialogIsta.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBoxIstaFolder.Text = folderBrowserDialogIsta.SelectedPath;
                UpdateDisplay();
            }
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            StopProgrammingService();
            UpdateDisplay();
            StoreSettings();
            timerUpdate.Enabled = false;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            LoadSettings();
            UpdateDisplay();
            timerUpdate.Enabled = true;
        }

        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void buttonStartHost_Click(object sender, EventArgs e)
        {
            StopProgrammingService();
            programmingService = new ProgrammingService(textBoxIstaFolder.Text, "32395");
            programmingService.StartPsdzServiceHost();
            UpdateDisplay();
        }

        private void buttonStopHost_Click(object sender, EventArgs e)
        {
            StopProgrammingService();
            UpdateDisplay();
        }
    }
}

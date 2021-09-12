using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz.Model;
using PsdzClient.Programming;

namespace PsdzClient
{
    public partial class FormMain : Form
    {
        private ProgrammingService programmingService;
        private bool taskActive = false;
        private IPsdzConnection psdzConnection;

        public FormMain()
        {
            InitializeComponent();
        }

        private void UpdateDisplay()
        {
            bool active = taskActive;
            bool hostRunning = false;
            bool vehicleConnected = false;
            if (!active)
            {
                hostRunning = programmingService != null && programmingService.IsPsdzPsdzServiceHostInitialized();
            }

            if (psdzConnection != null)
            {
                vehicleConnected = true;
            }

            buttonStartHost.Enabled = !active && !hostRunning;
            buttonStopHost.Enabled = !active && hostRunning;
            buttonConnect.Enabled = !active && hostRunning && !vehicleConnected;
            buttonDisconnect.Enabled = !active && hostRunning && vehicleConnected;
            buttonClose.Enabled = !active && !hostRunning;
            buttonAbort.Enabled = active;
        }

        private bool LoadSettings()
        {
            try
            {
                textBoxIstaFolder.Text = Properties.Settings.Default.IstaFolder;
                ipAddressControlVehicleIp.Text = Properties.Settings.Default.VehicleIp;
                if (string.IsNullOrWhiteSpace(ipAddressControlVehicleIp.Text.Trim('.')))
                {
                    ipAddressControlVehicleIp.Text = @"127.0.0.1";
                }
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
                Properties.Settings.Default.VehicleIp = ipAddressControlVehicleIp.Text;
                Properties.Settings.Default.Save();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private void UpdateStatus(string message = null)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() =>
                {
                    UpdateStatus(message);
                }));
                return;
            }

            textBoxStatus.Text = message ?? string.Empty;
            textBoxStatus.SelectionStart = textBoxStatus.TextLength;
            textBoxStatus.Update();
            textBoxStatus.ScrollToCaret();

            UpdateDisplay();
        }

        private async Task<bool> StartProgrammingServiceTask(string dealerId)
        {
            return await Task.Run(() => StartProgrammingService(dealerId)).ConfigureAwait(false);
        }

        private bool StartProgrammingService(string dealerId)
        {
            try
            {
                if (!StopProgrammingService())
                {
                    return false;
                }
                programmingService = new ProgrammingService(textBoxIstaFolder.Text, dealerId);
                programmingService.PsdzLoglevel = PsdzLoglevel.TRACE;
                programmingService.ProdiasLoglevel = ProdiasLoglevel.ERROR;
                programmingService.StartPsdzServiceHost();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private async Task<bool> StopProgrammingServiceTask()
        {
            // ReSharper disable once ConvertClosureToMethodGroup
            return await Task.Run(() => StopProgrammingService()).ConfigureAwait(false);
        }

        private bool StopProgrammingService()
        {
            try
            {
                if (programmingService != null)
                {
                    programmingService.Psdz.Shutdown();
                    programmingService.CloseConnectionsToPsdzHost();
                    programmingService.Dispose();
                    programmingService = null;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private async Task<bool> ConnectVehicleTask()
        {
            // ReSharper disable once ConvertClosureToMethodGroup
            return await Task.Run(() => ConnectVehicle()).ConfigureAwait(false);
        }

        private bool ConnectVehicle()
        {
            try
            {
                if (programmingService == null)
                {
                    return false;
                }

                if (psdzConnection != null)
                {
                    return false;
                }

                string url = "tcp://" + ipAddressControlVehicleIp.Text + ":6801";
                psdzConnection = programmingService.Psdz.ConnectionManagerService.ConnectOverEthernet("S15A_21_07_540_V_004_000_001", "S15A", url, "G31", "S15A-17-03-509");
                if (psdzConnection == null)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private async Task<bool> DisconnectVehicleTask()
        {
            // ReSharper disable once ConvertClosureToMethodGroup
            return await Task.Run(() => DisconnectVehicle()).ConfigureAwait(false);
        }

        private bool DisconnectVehicle()
        {
            try
            {
                if (programmingService == null)
                {
                    return false;
                }

                if (psdzConnection == null)
                {
                    return false;
                }

                programmingService.Psdz.ConnectionManagerService.CloseConnection(psdzConnection);
            }
            catch (Exception)
            {
                return false;
            }

            psdzConnection = null;
            return true;
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
            StringBuilder sbMessage = new StringBuilder();
            UpdateStatus(sbMessage.ToString());
            if (!StopProgrammingServiceTask().Wait(10000))
            {
                sbMessage.AppendLine("Host stop failed");
                UpdateStatus(sbMessage.ToString());
            }

            UpdateDisplay();
            StoreSettings();
            timerUpdate.Enabled = false;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            LoadSettings();
            UpdateDisplay();
            UpdateStatus();
            timerUpdate.Enabled = true;
        }

        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void buttonStartHost_Click(object sender, EventArgs e)
        {
            StringBuilder sbMessage = new StringBuilder();
            sbMessage.AppendLine("Starting host ...");
            UpdateStatus(sbMessage.ToString());

            StartProgrammingServiceTask("32395").ContinueWith(task =>
            {
                taskActive = false;
                if (task.Result)
                {
                    sbMessage.AppendLine("Host started");
                }
                else
                {
                    sbMessage.AppendLine("Host start failed");
                }
                UpdateStatus(sbMessage.ToString());
            });

            taskActive = true;
            UpdateDisplay();
        }

        private void buttonStopHost_Click(object sender, EventArgs e)
        {
            StringBuilder sbMessage = new StringBuilder();
            sbMessage.AppendLine("Stopping host ...");
            UpdateStatus(sbMessage.ToString());

            StopProgrammingServiceTask().ContinueWith(task =>
            {
                taskActive = false;
                if (task.Result)
                {
                    sbMessage.AppendLine("Host stopped");
                }
                else
                {
                    sbMessage.AppendLine("Host stop failed");
                }
                UpdateStatus(sbMessage.ToString());
            });

            taskActive = true;
            UpdateDisplay();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool active = taskActive;
            if (!active && programmingService != null && programmingService.IsPsdzPsdzServiceHostInitialized())
            {
                active = true;
            }

            if (active)
            {
                e.Cancel = true;
            }
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            StringBuilder sbMessage = new StringBuilder();
            sbMessage.AppendLine("Connecting vehicle ...");
            UpdateStatus(sbMessage.ToString());

            ConnectVehicleTask().ContinueWith(task =>
            {
                taskActive = false;
                if (task.Result)
                {
                    sbMessage.AppendLine("Vehicle connected");
                }
                else
                {
                    sbMessage.AppendLine("Vehicle connect failed");
                }
                UpdateStatus(sbMessage.ToString());
            });

            taskActive = true;
            UpdateDisplay();
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            StringBuilder sbMessage = new StringBuilder();
            sbMessage.AppendLine("Disconnecting vehicle ...");
            UpdateStatus(sbMessage.ToString());

            DisconnectVehicleTask().ContinueWith(task =>
            {
                taskActive = false;
                if (task.Result)
                {
                    sbMessage.AppendLine("Vehicle disconnected");
                }
                else
                {
                    sbMessage.AppendLine("Vehicle disconnect failed");
                }
                UpdateStatus(sbMessage.ToString());
            });

            taskActive = true;
            UpdateDisplay();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Swt;
using PsdzClient.Programming;

namespace PsdzClient
{
    public partial class FormMain : Form
    {
        private const string Baureihe = "G31";
        private const string TestVin = "WBAJM71000B055940";
        private ProgrammingService programmingService;
        private bool taskActive = false;
        private IPsdzConnection activePsdzConnection;

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

            if (activePsdzConnection != null)
            {
                vehicleConnected = true;
            }

            buttonStartHost.Enabled = !active && !hostRunning;
            buttonStopHost.Enabled = !active && hostRunning;
            buttonConnect.Enabled = !active && hostRunning && !vehicleConnected;
            buttonDisconnect.Enabled = !active && hostRunning && vehicleConnected;
            buttonFunc1.Enabled = !active && hostRunning && vehicleConnected;
            buttonFunc2.Enabled = buttonFunc1.Enabled;
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
                programmingService.ProdiasLoglevel = ProdiasLoglevel.TRACE;
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

        private async Task<IPsdzConnection> ConnectVehicleTask(string url, string baureihe)
        {
            // ReSharper disable once ConvertClosureToMethodGroup
            return await Task.Run(() => ConnectVehicle(url, baureihe)).ConfigureAwait(false);
        }

        private IPsdzConnection ConnectVehicle(string url, string baureihe)
        {
            try
            {
                if (programmingService == null)
                {
                    return null;
                }

                string verbund = programmingService.Psdz.ConfigurationService.RequestBaureihenverbund(baureihe);
                IEnumerable<IPsdzTargetSelector> targetSelectors = programmingService.Psdz.ConnectionFactoryService.GetTargetSelectors();
                IPsdzTargetSelector targetSelectorMatch = null;
                foreach (IPsdzTargetSelector targetSelector in targetSelectors)
                {
                    if (!targetSelector.IsDirect &&
                        string.Compare(verbund, targetSelector.Baureihenverbund, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        targetSelectorMatch = targetSelector;
                    }
                }

                if (targetSelectorMatch == null)
                {
                    return null;
                }

                IPsdzConnection psdzConnection = programmingService.Psdz.ConnectionManagerService.ConnectOverEthernet(targetSelectorMatch.Project, targetSelectorMatch.VehicleInfo, url, baureihe, "S15A-17-03-509");
                return psdzConnection;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<bool> DisconnectVehicleTask(IPsdzConnection psdzConnection)
        {
            // ReSharper disable once ConvertClosureToMethodGroup
            return await Task.Run(() => DisconnectVehicle(psdzConnection)).ConfigureAwait(false);
        }

        private bool DisconnectVehicle(IPsdzConnection psdzConnection)
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

        private async Task<string> VehicleFunctionsTask(IPsdzConnection psdzConnection, int function)
        {
            // ReSharper disable once ConvertClosureToMethodGroup
            return await Task.Run(() => VehicleFunctions(psdzConnection, function)).ConfigureAwait(false);
        }

        private string VehicleFunctions(IPsdzConnection psdzConnection, int function)
        {
            try
            {
                if (programmingService == null)
                {
                    return null;
                }

                StringBuilder sbResult = new StringBuilder();
                switch (function)
                {
                    case 0:
                    {
                        IPsdzSwtAction psdzSwtAction = programmingService.Psdz.ProgrammingService.RequestSwtAction(psdzConnection, true);
                        if (psdzSwtAction?.SwtEcus != null)
                        {
                            sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Ecus: {0}", psdzSwtAction.SwtEcus.Count()));
                            foreach (IPsdzSwtEcu psdzSwtEcu in psdzSwtAction.SwtEcus)
                            {
                                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Ecu: Id={0}, Vin={1}, CertState={2}, SwSig={3}",
                                    psdzSwtEcu.EcuIdentifier, psdzSwtEcu.Vin, psdzSwtEcu.RootCertState, psdzSwtEcu.SoftwareSigState));
                            }
                        }
                        break;
                    }

                    case 1:
                    {
                        IPsdzIstufenTriple iStufenTriple = programmingService.Psdz.VcmService.GetIStufenTripleActual(psdzConnection);
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "IStep: Current={0}, Last={1}, Shipment={2}",
                            iStufenTriple.Current, iStufenTriple.Last, iStufenTriple.Shipment));

                        IPsdzStandardFa standardFa = programmingService.Psdz.VcmService.GetStandardFaActual(psdzConnection);
                        IPsdzFa psdzFa = programmingService.Psdz.ObjectBuilder.BuildFa(standardFa, TestVin);
                        sbResult.AppendLine("FA:");
                        sbResult.Append(psdzFa.AsXml);
                        break;
                    }
                }

                return sbResult.ToString();
            }
            catch (Exception)
            {
                return null;
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
            if (activePsdzConnection != null)
            {
                return;
            }

            StringBuilder sbMessage = new StringBuilder();
            sbMessage.AppendLine("Connecting vehicle ...");
            UpdateStatus(sbMessage.ToString());

            string url = "tcp://" + ipAddressControlVehicleIp.Text + ":6801";
            ConnectVehicleTask(url, Baureihe).ContinueWith(task =>
            {
                taskActive = false;
                IPsdzConnection psdzConnection = task.Result;
                if (psdzConnection != null)
                {
                    activePsdzConnection = psdzConnection;
                    sbMessage.AppendLine("Vehicle connected");
                    sbMessage.AppendLine(string.Format(CultureInfo.InvariantCulture, "Id: {0}", psdzConnection.Id));
                    sbMessage.AppendLine(string.Format(CultureInfo.InvariantCulture, "Port: {0}", psdzConnection.Port));
                    sbMessage.AppendLine(string.Format(CultureInfo.InvariantCulture, "Project: {0}", psdzConnection.TargetSelector.Project));
                    sbMessage.AppendLine(string.Format(CultureInfo.InvariantCulture, "Vehicle: {0}", psdzConnection.TargetSelector.VehicleInfo));
                    sbMessage.AppendLine(string.Format(CultureInfo.InvariantCulture, "Series: {0}", psdzConnection.TargetSelector.Baureihenverbund));
                    sbMessage.AppendLine(string.Format(CultureInfo.InvariantCulture, "Direct: {0}", psdzConnection.TargetSelector.IsDirect));
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
            if (activePsdzConnection == null)
            {
                return;
            }

            StringBuilder sbMessage = new StringBuilder();
            sbMessage.AppendLine("Disconnecting vehicle ...");
            UpdateStatus(sbMessage.ToString());

            DisconnectVehicleTask(activePsdzConnection).ContinueWith(task =>
            {
                taskActive = false;
                activePsdzConnection = null;
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

        private void buttonFunc_Click(object sender, EventArgs e)
        {
            if (activePsdzConnection == null)
            {
                return;
            }

            StringBuilder sbMessage = new StringBuilder();
            sbMessage.AppendLine("Executing vehicle functions ...");
            UpdateStatus(sbMessage.ToString());

            int function = 0;
            if (sender == buttonFunc1)
            {
                function = 0;
            }
            if (sender == buttonFunc2)
            {
                function = 1;
            }

            VehicleFunctionsTask(activePsdzConnection, function).ContinueWith(task =>
            {
                taskActive = false;
                string resultMessage = task.Result;
                if (!string.IsNullOrEmpty(resultMessage))
                {
                    sbMessage.AppendLine(resultMessage);
                }
                else
                {
                    sbMessage.AppendLine("Vehicle functions failed");
                }
                UpdateStatus(sbMessage.ToString());
            });

            taskActive = true;
            UpdateDisplay();
        }
    }
}

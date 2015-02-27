namespace CarControl
{
    partial class MainForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.listPorts = new System.Windows.Forms.ListBox();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.timerUpdate = new System.Windows.Forms.Timer();
            this.buttonClose = new System.Windows.Forms.Button();
            this.textBoxErrorCount = new System.Windows.Forms.TextBox();
            this.labelErrorCount = new System.Windows.Forms.Label();
            this.checkBoxConnected = new System.Windows.Forms.CheckBox();
            this.checkBoxLogFile = new System.Windows.Forms.CheckBox();
            this.buttonStoreLog = new System.Windows.Forms.Button();
            this.tabControlDevice = new System.Windows.Forms.TabControl();
            this.tabPageAxis = new System.Windows.Forms.TabPage();
            this.tabPageMotor = new System.Windows.Forms.TabPage();
            this.panelMotorData = new System.Windows.Forms.Panel();
            this.checkBoxMotorPartFilterUnblock = new System.Windows.Forms.CheckBox();
            this.checkBoxMotorPartFilterStatus = new System.Windows.Forms.CheckBox();
            this.textBoxMotorPartFilterDistSinceRegen = new System.Windows.Forms.TextBox();
            this.labelMotorPartFilterDistSinceRegen = new System.Windows.Forms.Label();
            this.textBoxMotorExhaustBackPressure = new System.Windows.Forms.TextBox();
            this.labelMotorExhaustBackPressure = new System.Windows.Forms.Label();
            this.textBoxMotorTempBeforeFilter = new System.Windows.Forms.TextBox();
            this.labelMotorTempBeforeFilter = new System.Windows.Forms.Label();
            this.textBoxMotorTempBeforeCat = new System.Windows.Forms.TextBox();
            this.labelMotorTempBeforeCat = new System.Windows.Forms.Label();
            this.textBoxMotorFuelTemp = new System.Windows.Forms.TextBox();
            this.labelMotorFuelTemp = new System.Windows.Forms.Label();
            this.textBoxMotorAmbientPress = new System.Windows.Forms.TextBox();
            this.labelMotorAmbientPress = new System.Windows.Forms.Label();
            this.textBoxMotorAmbientTemp = new System.Windows.Forms.TextBox();
            this.labelMotorAmbientTemp = new System.Windows.Forms.Label();
            this.checkBoxOilPressSwt = new System.Windows.Forms.CheckBox();
            this.checkBoxMotorPartFilterRequest = new System.Windows.Forms.CheckBox();
            this.textBoxMotorAirMassAct = new System.Windows.Forms.TextBox();
            this.labelMotorAirMassAct = new System.Windows.Forms.Label();
            this.textBoxMotorAirMassSet = new System.Windows.Forms.TextBox();
            this.labelMotorAirMassSet = new System.Windows.Forms.Label();
            this.textBoxMotorRailPressAct = new System.Windows.Forms.TextBox();
            this.labelMotorRailPressAct = new System.Windows.Forms.Label();
            this.textBoxMotorRailPressSet = new System.Windows.Forms.TextBox();
            this.labelMotorRailPressSet = new System.Windows.Forms.Label();
            this.textBoxMotorBoostPressAct = new System.Windows.Forms.TextBox();
            this.labelMotorBoostPressAct = new System.Windows.Forms.Label();
            this.textBoxMotorBoostPressSet = new System.Windows.Forms.TextBox();
            this.labelMotorBoostPressSet = new System.Windows.Forms.Label();
            this.textBoxMotorIntakeAirTemp = new System.Windows.Forms.TextBox();
            this.labelMotorIntakeAirTemp = new System.Windows.Forms.Label();
            this.textBoxMotorTemp = new System.Windows.Forms.TextBox();
            this.labelMotorTemp = new System.Windows.Forms.Label();
            this.textBoxMotorAirMass = new System.Windows.Forms.TextBox();
            this.labelMotorAirMass = new System.Windows.Forms.Label();
            this.textBoxMotorBatteryVoltage = new System.Windows.Forms.TextBox();
            this.labelMotorBatteryVoltage = new System.Windows.Forms.Label();
            this.tabPageMotorUnevenRunning = new System.Windows.Forms.TabPage();
            this.panelMotorUnevenRunning = new System.Windows.Forms.Panel();
            this.labelIdleSpeedControlOn = new System.Windows.Forms.Label();
            this.textBoxMotorQuantCorrCylinder4 = new System.Windows.Forms.TextBox();
            this.labelMotorQuantCorrCylinder4 = new System.Windows.Forms.Label();
            this.textBoxMotorQuantCorrCylinder3 = new System.Windows.Forms.TextBox();
            this.labelMotorQuantCorrCylinder3 = new System.Windows.Forms.Label();
            this.textBoxMotorQuantCorrCylinder2 = new System.Windows.Forms.TextBox();
            this.labelMotorQuantCorrCylinder2 = new System.Windows.Forms.Label();
            this.textBoxMotorQuantCorrCylinder1 = new System.Windows.Forms.TextBox();
            this.labelMotorQuantCorrCylinder1 = new System.Windows.Forms.Label();
            this.tabPageMotorRotIrregular = new System.Windows.Forms.TabPage();
            this.panelMotorRotIrregular = new System.Windows.Forms.Panel();
            this.labelIdleSpeedControlOff = new System.Windows.Forms.Label();
            this.textBoxMotorRpmCylinder4 = new System.Windows.Forms.TextBox();
            this.labelMotorRpmCylinder4 = new System.Windows.Forms.Label();
            this.textBoxMotorRpmCylinder3 = new System.Windows.Forms.TextBox();
            this.labelMotorRpmCylinder3 = new System.Windows.Forms.Label();
            this.textBoxMotorRpmCylinder2 = new System.Windows.Forms.TextBox();
            this.labelMotorRpmCylinder2 = new System.Windows.Forms.Label();
            this.textBoxMotorRpmCylinder1 = new System.Windows.Forms.TextBox();
            this.labelMotorRpmCylinder1 = new System.Windows.Forms.Label();
            this.tabPagePm = new System.Windows.Forms.TabPage();
            this.panelPm = new System.Windows.Forms.Panel();
            this.labelPmSocPercentD1 = new System.Windows.Forms.Label();
            this.labelPmStartCapD1 = new System.Windows.Forms.Label();
            this.labelPmSocQD1 = new System.Windows.Forms.Label();
            this.textBoxPmSocPercentD1 = new System.Windows.Forms.TextBox();
            this.textBoxPmStartCapD1 = new System.Windows.Forms.TextBox();
            this.textBoxPmSocQD1 = new System.Windows.Forms.TextBox();
            this.textBoxPmSocPercent = new System.Windows.Forms.TextBox();
            this.labelPmSocPercent = new System.Windows.Forms.Label();
            this.textBoxPmStartCap = new System.Windows.Forms.TextBox();
            this.labelPmStartCap = new System.Windows.Forms.Label();
            this.textBoxPmSocQ = new System.Windows.Forms.TextBox();
            this.labelPmSocQ = new System.Windows.Forms.Label();
            this.textBoxPmCalEvents = new System.Windows.Forms.TextBox();
            this.labelPmCalEvents = new System.Windows.Forms.Label();
            this.textBoxPmSeasonTemp = new System.Windows.Forms.TextBox();
            this.labelPmSeasonTemp = new System.Windows.Forms.Label();
            this.textBoxPmSocFit = new System.Windows.Forms.TextBox();
            this.labelPmSocFit = new System.Windows.Forms.Label();
            this.textBoxPmSoh = new System.Windows.Forms.TextBox();
            this.labelPmSoh = new System.Windows.Forms.Label();
            this.textBoxPmBatCap = new System.Windows.Forms.TextBox();
            this.labelPmBatCap = new System.Windows.Forms.Label();
            this.tabPageCccNav = new System.Windows.Forms.TabPage();
            this.panelCccNav = new System.Windows.Forms.Panel();
            this.labelCccNavResVert = new System.Windows.Forms.Label();
            this.textBoxCccNavResVert = new System.Windows.Forms.TextBox();
            this.textBoxCccNavResHorz = new System.Windows.Forms.TextBox();
            this.labelCccNavResHorz = new System.Windows.Forms.Label();
            this.textBoxCccNavResPos = new System.Windows.Forms.TextBox();
            this.labelCccNavResPos = new System.Windows.Forms.Label();
            this.textBoxCccNavGpsDateTime = new System.Windows.Forms.TextBox();
            this.labelCccNavGpsDateTime = new System.Windows.Forms.Label();
            this.textBoxCccNavSpeed = new System.Windows.Forms.TextBox();
            this.labelCccNavSpeed = new System.Windows.Forms.Label();
            this.checkBoxHipDriver = new System.Windows.Forms.CheckBox();
            this.checkBoxCccNavAlmanach = new System.Windows.Forms.CheckBox();
            this.labelCccNavPosType = new System.Windows.Forms.Label();
            this.textBoxCccNavPosType = new System.Windows.Forms.TextBox();
            this.textBoxCccNavPosHeight = new System.Windows.Forms.TextBox();
            this.labelCccNavPosHeight = new System.Windows.Forms.Label();
            this.textBoxCccNavPosLong = new System.Windows.Forms.TextBox();
            this.labelCccNavPosLong = new System.Windows.Forms.Label();
            this.textBoxCccNavPosLat = new System.Windows.Forms.TextBox();
            this.labelCccNavPosLat = new System.Windows.Forms.Label();
            this.tabPageIhk = new System.Windows.Forms.TabPage();
            this.panelIhk = new System.Windows.Forms.Panel();
            this.textBoxIhkHeatExSetpoint = new System.Windows.Forms.TextBox();
            this.labelIhkHeatExSetpoint = new System.Windows.Forms.Label();
            this.textBoxIhkHeatExTemp = new System.Windows.Forms.TextBox();
            this.labelIhkHeatExTemp = new System.Windows.Forms.Label();
            this.textBoxIhkSetpoint = new System.Windows.Forms.TextBox();
            this.labelIhkSetpoint = new System.Windows.Forms.Label();
            this.textBoxIhkOutTemp = new System.Windows.Forms.TextBox();
            this.labelIhkOutTemp = new System.Windows.Forms.Label();
            this.textBoxIhkInTempDelay = new System.Windows.Forms.TextBox();
            this.labelIhkInTempDelay = new System.Windows.Forms.Label();
            this.textBoxIhkInTemp = new System.Windows.Forms.TextBox();
            this.labelIhkInTemp = new System.Windows.Forms.Label();
            this.tabPageErrors = new System.Windows.Forms.TabPage();
            this.panelErrors = new System.Windows.Forms.Panel();
            this.textBoxErrors2 = new System.Windows.Forms.TextBox();
            this.textBoxErrors1 = new System.Windows.Forms.TextBox();
            this.tabPageAdapterConfig = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonAdapterConfigCanOff = new System.Windows.Forms.Button();
            this.buttonAdapterConfigCan100 = new System.Windows.Forms.Button();
            this.buttonAdapterConfigCan500 = new System.Windows.Forms.Button();
            this.textBoxAdapterConfigResult = new System.Windows.Forms.TextBox();
            this.labelAdapterConfigResult = new System.Windows.Forms.Label();
            this.tabPageTest = new System.Windows.Forms.TabPage();
            this.panelTest = new System.Windows.Forms.Panel();
            this.textBoxTest = new System.Windows.Forms.TextBox();
            this.buttonPowerOff = new System.Windows.Forms.Button();
            this.labelBatteryLife = new System.Windows.Forms.Label();
            this.pushButtonWlan = new CarControl.PushButton();
            this.groupBoxControl = new CarControl.GroupBox();
            this.pushButtonDown = new CarControl.PushButton();
            this.pushButtonUp = new CarControl.PushButton();
            this.groupBoxStatus = new CarControl.GroupBox();
            this.panelAxisData = new System.Windows.Forms.Panel();
            this.textBoxSpeed = new System.Windows.Forms.TextBox();
            this.labelSpeed = new System.Windows.Forms.Label();
            this.textBoxAxisBatteryVoltage = new System.Windows.Forms.TextBox();
            this.labelAxisBatteryVoltage = new System.Windows.Forms.Label();
            this.textBoxValveState = new System.Windows.Forms.TextBox();
            this.labelOutputState = new System.Windows.Forms.Label();
            this.textBoxAxisMode = new System.Windows.Forms.TextBox();
            this.labelAxisMode = new System.Windows.Forms.Label();
            this.labelAxisLeft = new System.Windows.Forms.Label();
            this.textBoxAxisRigth = new System.Windows.Forms.TextBox();
            this.textBoxAxisLeft = new System.Windows.Forms.TextBox();
            this.labelAxisRight = new System.Windows.Forms.Label();
            this.tabControlDevice.SuspendLayout();
            this.tabPageAxis.SuspendLayout();
            this.tabPageMotor.SuspendLayout();
            this.panelMotorData.SuspendLayout();
            this.tabPageMotorUnevenRunning.SuspendLayout();
            this.panelMotorUnevenRunning.SuspendLayout();
            this.tabPageMotorRotIrregular.SuspendLayout();
            this.panelMotorRotIrregular.SuspendLayout();
            this.tabPagePm.SuspendLayout();
            this.panelPm.SuspendLayout();
            this.tabPageCccNav.SuspendLayout();
            this.panelCccNav.SuspendLayout();
            this.tabPageIhk.SuspendLayout();
            this.panelIhk.SuspendLayout();
            this.tabPageErrors.SuspendLayout();
            this.panelErrors.SuspendLayout();
            this.tabPageAdapterConfig.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabPageTest.SuspendLayout();
            this.panelTest.SuspendLayout();
            this.groupBoxControl.SuspendLayout();
            this.groupBoxStatus.SuspendLayout();
            this.panelAxisData.SuspendLayout();
            this.SuspendLayout();
            // 
            // listPorts
            // 
            resources.ApplyResources(this.listPorts, "listPorts");
            this.listPorts.Name = "listPorts";
            // 
            // buttonConnect
            // 
            resources.ApplyResources(this.buttonConnect, "buttonConnect");
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // timerUpdate
            // 
            this.timerUpdate.Interval = 500;
            this.timerUpdate.Tick += new System.EventHandler(this.timerUpdate_Tick);
            // 
            // buttonClose
            // 
            resources.ApplyResources(this.buttonClose, "buttonClose");
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // textBoxErrorCount
            // 
            resources.ApplyResources(this.textBoxErrorCount, "textBoxErrorCount");
            this.textBoxErrorCount.Name = "textBoxErrorCount";
            this.textBoxErrorCount.ReadOnly = true;
            this.textBoxErrorCount.TabStop = false;
            // 
            // labelErrorCount
            // 
            resources.ApplyResources(this.labelErrorCount, "labelErrorCount");
            this.labelErrorCount.Name = "labelErrorCount";
            // 
            // checkBoxConnected
            // 
            this.checkBoxConnected.AutoCheck = false;
            resources.ApplyResources(this.checkBoxConnected, "checkBoxConnected");
            this.checkBoxConnected.Name = "checkBoxConnected";
            this.checkBoxConnected.TabStop = false;
            // 
            // checkBoxLogFile
            // 
            resources.ApplyResources(this.checkBoxLogFile, "checkBoxLogFile");
            this.checkBoxLogFile.Name = "checkBoxLogFile";
            // 
            // buttonStoreLog
            // 
            resources.ApplyResources(this.buttonStoreLog, "buttonStoreLog");
            this.buttonStoreLog.Name = "buttonStoreLog";
            this.buttonStoreLog.Click += new System.EventHandler(this.buttonStoreLog_Click);
            // 
            // tabControlDevice
            // 
            this.tabControlDevice.Controls.Add(this.tabPageAxis);
            this.tabControlDevice.Controls.Add(this.tabPageMotor);
            this.tabControlDevice.Controls.Add(this.tabPageMotorUnevenRunning);
            this.tabControlDevice.Controls.Add(this.tabPageMotorRotIrregular);
            this.tabControlDevice.Controls.Add(this.tabPagePm);
            this.tabControlDevice.Controls.Add(this.tabPageCccNav);
            this.tabControlDevice.Controls.Add(this.tabPageIhk);
            this.tabControlDevice.Controls.Add(this.tabPageErrors);
            this.tabControlDevice.Controls.Add(this.tabPageAdapterConfig);
            this.tabControlDevice.Controls.Add(this.tabPageTest);
            resources.ApplyResources(this.tabControlDevice, "tabControlDevice");
            this.tabControlDevice.Name = "tabControlDevice";
            this.tabControlDevice.SelectedIndex = 0;
            this.tabControlDevice.SelectedIndexChanged += new System.EventHandler(this.tabControlDevice_SelectedIndexChanged);
            // 
            // tabPageAxis
            // 
            this.tabPageAxis.Controls.Add(this.groupBoxControl);
            this.tabPageAxis.Controls.Add(this.groupBoxStatus);
            resources.ApplyResources(this.tabPageAxis, "tabPageAxis");
            this.tabPageAxis.Name = "tabPageAxis";
            // 
            // tabPageMotor
            // 
            this.tabPageMotor.Controls.Add(this.panelMotorData);
            resources.ApplyResources(this.tabPageMotor, "tabPageMotor");
            this.tabPageMotor.Name = "tabPageMotor";
            // 
            // panelMotorData
            // 
            this.panelMotorData.Controls.Add(this.checkBoxMotorPartFilterUnblock);
            this.panelMotorData.Controls.Add(this.checkBoxMotorPartFilterStatus);
            this.panelMotorData.Controls.Add(this.textBoxMotorPartFilterDistSinceRegen);
            this.panelMotorData.Controls.Add(this.labelMotorPartFilterDistSinceRegen);
            this.panelMotorData.Controls.Add(this.textBoxMotorExhaustBackPressure);
            this.panelMotorData.Controls.Add(this.labelMotorExhaustBackPressure);
            this.panelMotorData.Controls.Add(this.textBoxMotorTempBeforeFilter);
            this.panelMotorData.Controls.Add(this.labelMotorTempBeforeFilter);
            this.panelMotorData.Controls.Add(this.textBoxMotorTempBeforeCat);
            this.panelMotorData.Controls.Add(this.labelMotorTempBeforeCat);
            this.panelMotorData.Controls.Add(this.textBoxMotorFuelTemp);
            this.panelMotorData.Controls.Add(this.labelMotorFuelTemp);
            this.panelMotorData.Controls.Add(this.textBoxMotorAmbientPress);
            this.panelMotorData.Controls.Add(this.labelMotorAmbientPress);
            this.panelMotorData.Controls.Add(this.textBoxMotorAmbientTemp);
            this.panelMotorData.Controls.Add(this.labelMotorAmbientTemp);
            this.panelMotorData.Controls.Add(this.checkBoxOilPressSwt);
            this.panelMotorData.Controls.Add(this.checkBoxMotorPartFilterRequest);
            this.panelMotorData.Controls.Add(this.textBoxMotorAirMassAct);
            this.panelMotorData.Controls.Add(this.labelMotorAirMassAct);
            this.panelMotorData.Controls.Add(this.textBoxMotorAirMassSet);
            this.panelMotorData.Controls.Add(this.labelMotorAirMassSet);
            this.panelMotorData.Controls.Add(this.textBoxMotorRailPressAct);
            this.panelMotorData.Controls.Add(this.labelMotorRailPressAct);
            this.panelMotorData.Controls.Add(this.textBoxMotorRailPressSet);
            this.panelMotorData.Controls.Add(this.labelMotorRailPressSet);
            this.panelMotorData.Controls.Add(this.textBoxMotorBoostPressAct);
            this.panelMotorData.Controls.Add(this.labelMotorBoostPressAct);
            this.panelMotorData.Controls.Add(this.textBoxMotorBoostPressSet);
            this.panelMotorData.Controls.Add(this.labelMotorBoostPressSet);
            this.panelMotorData.Controls.Add(this.textBoxMotorIntakeAirTemp);
            this.panelMotorData.Controls.Add(this.labelMotorIntakeAirTemp);
            this.panelMotorData.Controls.Add(this.textBoxMotorTemp);
            this.panelMotorData.Controls.Add(this.labelMotorTemp);
            this.panelMotorData.Controls.Add(this.textBoxMotorAirMass);
            this.panelMotorData.Controls.Add(this.labelMotorAirMass);
            this.panelMotorData.Controls.Add(this.textBoxMotorBatteryVoltage);
            this.panelMotorData.Controls.Add(this.labelMotorBatteryVoltage);
            resources.ApplyResources(this.panelMotorData, "panelMotorData");
            this.panelMotorData.Name = "panelMotorData";
            // 
            // checkBoxMotorPartFilterUnblock
            // 
            this.checkBoxMotorPartFilterUnblock.AutoCheck = false;
            resources.ApplyResources(this.checkBoxMotorPartFilterUnblock, "checkBoxMotorPartFilterUnblock");
            this.checkBoxMotorPartFilterUnblock.Name = "checkBoxMotorPartFilterUnblock";
            this.checkBoxMotorPartFilterUnblock.TabStop = false;
            // 
            // checkBoxMotorPartFilterStatus
            // 
            this.checkBoxMotorPartFilterStatus.AutoCheck = false;
            resources.ApplyResources(this.checkBoxMotorPartFilterStatus, "checkBoxMotorPartFilterStatus");
            this.checkBoxMotorPartFilterStatus.Name = "checkBoxMotorPartFilterStatus";
            this.checkBoxMotorPartFilterStatus.TabStop = false;
            // 
            // textBoxMotorPartFilterDistSinceRegen
            // 
            resources.ApplyResources(this.textBoxMotorPartFilterDistSinceRegen, "textBoxMotorPartFilterDistSinceRegen");
            this.textBoxMotorPartFilterDistSinceRegen.Name = "textBoxMotorPartFilterDistSinceRegen";
            this.textBoxMotorPartFilterDistSinceRegen.ReadOnly = true;
            this.textBoxMotorPartFilterDistSinceRegen.TabStop = false;
            // 
            // labelMotorPartFilterDistSinceRegen
            // 
            resources.ApplyResources(this.labelMotorPartFilterDistSinceRegen, "labelMotorPartFilterDistSinceRegen");
            this.labelMotorPartFilterDistSinceRegen.Name = "labelMotorPartFilterDistSinceRegen";
            // 
            // textBoxMotorExhaustBackPressure
            // 
            resources.ApplyResources(this.textBoxMotorExhaustBackPressure, "textBoxMotorExhaustBackPressure");
            this.textBoxMotorExhaustBackPressure.Name = "textBoxMotorExhaustBackPressure";
            this.textBoxMotorExhaustBackPressure.ReadOnly = true;
            this.textBoxMotorExhaustBackPressure.TabStop = false;
            // 
            // labelMotorExhaustBackPressure
            // 
            resources.ApplyResources(this.labelMotorExhaustBackPressure, "labelMotorExhaustBackPressure");
            this.labelMotorExhaustBackPressure.Name = "labelMotorExhaustBackPressure";
            // 
            // textBoxMotorTempBeforeFilter
            // 
            resources.ApplyResources(this.textBoxMotorTempBeforeFilter, "textBoxMotorTempBeforeFilter");
            this.textBoxMotorTempBeforeFilter.Name = "textBoxMotorTempBeforeFilter";
            this.textBoxMotorTempBeforeFilter.ReadOnly = true;
            this.textBoxMotorTempBeforeFilter.TabStop = false;
            // 
            // labelMotorTempBeforeFilter
            // 
            resources.ApplyResources(this.labelMotorTempBeforeFilter, "labelMotorTempBeforeFilter");
            this.labelMotorTempBeforeFilter.Name = "labelMotorTempBeforeFilter";
            // 
            // textBoxMotorTempBeforeCat
            // 
            resources.ApplyResources(this.textBoxMotorTempBeforeCat, "textBoxMotorTempBeforeCat");
            this.textBoxMotorTempBeforeCat.Name = "textBoxMotorTempBeforeCat";
            this.textBoxMotorTempBeforeCat.ReadOnly = true;
            this.textBoxMotorTempBeforeCat.TabStop = false;
            // 
            // labelMotorTempBeforeCat
            // 
            resources.ApplyResources(this.labelMotorTempBeforeCat, "labelMotorTempBeforeCat");
            this.labelMotorTempBeforeCat.Name = "labelMotorTempBeforeCat";
            // 
            // textBoxMotorFuelTemp
            // 
            resources.ApplyResources(this.textBoxMotorFuelTemp, "textBoxMotorFuelTemp");
            this.textBoxMotorFuelTemp.Name = "textBoxMotorFuelTemp";
            this.textBoxMotorFuelTemp.ReadOnly = true;
            this.textBoxMotorFuelTemp.TabStop = false;
            // 
            // labelMotorFuelTemp
            // 
            resources.ApplyResources(this.labelMotorFuelTemp, "labelMotorFuelTemp");
            this.labelMotorFuelTemp.Name = "labelMotorFuelTemp";
            // 
            // textBoxMotorAmbientPress
            // 
            resources.ApplyResources(this.textBoxMotorAmbientPress, "textBoxMotorAmbientPress");
            this.textBoxMotorAmbientPress.Name = "textBoxMotorAmbientPress";
            this.textBoxMotorAmbientPress.ReadOnly = true;
            this.textBoxMotorAmbientPress.TabStop = false;
            // 
            // labelMotorAmbientPress
            // 
            resources.ApplyResources(this.labelMotorAmbientPress, "labelMotorAmbientPress");
            this.labelMotorAmbientPress.Name = "labelMotorAmbientPress";
            // 
            // textBoxMotorAmbientTemp
            // 
            resources.ApplyResources(this.textBoxMotorAmbientTemp, "textBoxMotorAmbientTemp");
            this.textBoxMotorAmbientTemp.Name = "textBoxMotorAmbientTemp";
            this.textBoxMotorAmbientTemp.ReadOnly = true;
            this.textBoxMotorAmbientTemp.TabStop = false;
            // 
            // labelMotorAmbientTemp
            // 
            resources.ApplyResources(this.labelMotorAmbientTemp, "labelMotorAmbientTemp");
            this.labelMotorAmbientTemp.Name = "labelMotorAmbientTemp";
            // 
            // checkBoxOilPressSwt
            // 
            this.checkBoxOilPressSwt.AutoCheck = false;
            resources.ApplyResources(this.checkBoxOilPressSwt, "checkBoxOilPressSwt");
            this.checkBoxOilPressSwt.Name = "checkBoxOilPressSwt";
            this.checkBoxOilPressSwt.TabStop = false;
            // 
            // checkBoxMotorPartFilterRequest
            // 
            this.checkBoxMotorPartFilterRequest.AutoCheck = false;
            resources.ApplyResources(this.checkBoxMotorPartFilterRequest, "checkBoxMotorPartFilterRequest");
            this.checkBoxMotorPartFilterRequest.Name = "checkBoxMotorPartFilterRequest";
            this.checkBoxMotorPartFilterRequest.TabStop = false;
            // 
            // textBoxMotorAirMassAct
            // 
            resources.ApplyResources(this.textBoxMotorAirMassAct, "textBoxMotorAirMassAct");
            this.textBoxMotorAirMassAct.Name = "textBoxMotorAirMassAct";
            this.textBoxMotorAirMassAct.ReadOnly = true;
            this.textBoxMotorAirMassAct.TabStop = false;
            // 
            // labelMotorAirMassAct
            // 
            resources.ApplyResources(this.labelMotorAirMassAct, "labelMotorAirMassAct");
            this.labelMotorAirMassAct.Name = "labelMotorAirMassAct";
            // 
            // textBoxMotorAirMassSet
            // 
            resources.ApplyResources(this.textBoxMotorAirMassSet, "textBoxMotorAirMassSet");
            this.textBoxMotorAirMassSet.Name = "textBoxMotorAirMassSet";
            this.textBoxMotorAirMassSet.ReadOnly = true;
            this.textBoxMotorAirMassSet.TabStop = false;
            // 
            // labelMotorAirMassSet
            // 
            resources.ApplyResources(this.labelMotorAirMassSet, "labelMotorAirMassSet");
            this.labelMotorAirMassSet.Name = "labelMotorAirMassSet";
            // 
            // textBoxMotorRailPressAct
            // 
            resources.ApplyResources(this.textBoxMotorRailPressAct, "textBoxMotorRailPressAct");
            this.textBoxMotorRailPressAct.Name = "textBoxMotorRailPressAct";
            this.textBoxMotorRailPressAct.ReadOnly = true;
            this.textBoxMotorRailPressAct.TabStop = false;
            // 
            // labelMotorRailPressAct
            // 
            resources.ApplyResources(this.labelMotorRailPressAct, "labelMotorRailPressAct");
            this.labelMotorRailPressAct.Name = "labelMotorRailPressAct";
            // 
            // textBoxMotorRailPressSet
            // 
            resources.ApplyResources(this.textBoxMotorRailPressSet, "textBoxMotorRailPressSet");
            this.textBoxMotorRailPressSet.Name = "textBoxMotorRailPressSet";
            this.textBoxMotorRailPressSet.ReadOnly = true;
            this.textBoxMotorRailPressSet.TabStop = false;
            // 
            // labelMotorRailPressSet
            // 
            resources.ApplyResources(this.labelMotorRailPressSet, "labelMotorRailPressSet");
            this.labelMotorRailPressSet.Name = "labelMotorRailPressSet";
            // 
            // textBoxMotorBoostPressAct
            // 
            resources.ApplyResources(this.textBoxMotorBoostPressAct, "textBoxMotorBoostPressAct");
            this.textBoxMotorBoostPressAct.Name = "textBoxMotorBoostPressAct";
            this.textBoxMotorBoostPressAct.ReadOnly = true;
            this.textBoxMotorBoostPressAct.TabStop = false;
            // 
            // labelMotorBoostPressAct
            // 
            resources.ApplyResources(this.labelMotorBoostPressAct, "labelMotorBoostPressAct");
            this.labelMotorBoostPressAct.Name = "labelMotorBoostPressAct";
            // 
            // textBoxMotorBoostPressSet
            // 
            resources.ApplyResources(this.textBoxMotorBoostPressSet, "textBoxMotorBoostPressSet");
            this.textBoxMotorBoostPressSet.Name = "textBoxMotorBoostPressSet";
            this.textBoxMotorBoostPressSet.ReadOnly = true;
            this.textBoxMotorBoostPressSet.TabStop = false;
            // 
            // labelMotorBoostPressSet
            // 
            resources.ApplyResources(this.labelMotorBoostPressSet, "labelMotorBoostPressSet");
            this.labelMotorBoostPressSet.Name = "labelMotorBoostPressSet";
            // 
            // textBoxMotorIntakeAirTemp
            // 
            resources.ApplyResources(this.textBoxMotorIntakeAirTemp, "textBoxMotorIntakeAirTemp");
            this.textBoxMotorIntakeAirTemp.Name = "textBoxMotorIntakeAirTemp";
            this.textBoxMotorIntakeAirTemp.ReadOnly = true;
            this.textBoxMotorIntakeAirTemp.TabStop = false;
            // 
            // labelMotorIntakeAirTemp
            // 
            resources.ApplyResources(this.labelMotorIntakeAirTemp, "labelMotorIntakeAirTemp");
            this.labelMotorIntakeAirTemp.Name = "labelMotorIntakeAirTemp";
            // 
            // textBoxMotorTemp
            // 
            resources.ApplyResources(this.textBoxMotorTemp, "textBoxMotorTemp");
            this.textBoxMotorTemp.Name = "textBoxMotorTemp";
            this.textBoxMotorTemp.ReadOnly = true;
            this.textBoxMotorTemp.TabStop = false;
            // 
            // labelMotorTemp
            // 
            resources.ApplyResources(this.labelMotorTemp, "labelMotorTemp");
            this.labelMotorTemp.Name = "labelMotorTemp";
            // 
            // textBoxMotorAirMass
            // 
            resources.ApplyResources(this.textBoxMotorAirMass, "textBoxMotorAirMass");
            this.textBoxMotorAirMass.Name = "textBoxMotorAirMass";
            this.textBoxMotorAirMass.ReadOnly = true;
            this.textBoxMotorAirMass.TabStop = false;
            // 
            // labelMotorAirMass
            // 
            resources.ApplyResources(this.labelMotorAirMass, "labelMotorAirMass");
            this.labelMotorAirMass.Name = "labelMotorAirMass";
            // 
            // textBoxMotorBatteryVoltage
            // 
            resources.ApplyResources(this.textBoxMotorBatteryVoltage, "textBoxMotorBatteryVoltage");
            this.textBoxMotorBatteryVoltage.Name = "textBoxMotorBatteryVoltage";
            this.textBoxMotorBatteryVoltage.ReadOnly = true;
            this.textBoxMotorBatteryVoltage.TabStop = false;
            // 
            // labelMotorBatteryVoltage
            // 
            resources.ApplyResources(this.labelMotorBatteryVoltage, "labelMotorBatteryVoltage");
            this.labelMotorBatteryVoltage.Name = "labelMotorBatteryVoltage";
            // 
            // tabPageMotorUnevenRunning
            // 
            this.tabPageMotorUnevenRunning.Controls.Add(this.panelMotorUnevenRunning);
            resources.ApplyResources(this.tabPageMotorUnevenRunning, "tabPageMotorUnevenRunning");
            this.tabPageMotorUnevenRunning.Name = "tabPageMotorUnevenRunning";
            // 
            // panelMotorUnevenRunning
            // 
            this.panelMotorUnevenRunning.Controls.Add(this.labelIdleSpeedControlOn);
            this.panelMotorUnevenRunning.Controls.Add(this.textBoxMotorQuantCorrCylinder4);
            this.panelMotorUnevenRunning.Controls.Add(this.labelMotorQuantCorrCylinder4);
            this.panelMotorUnevenRunning.Controls.Add(this.textBoxMotorQuantCorrCylinder3);
            this.panelMotorUnevenRunning.Controls.Add(this.labelMotorQuantCorrCylinder3);
            this.panelMotorUnevenRunning.Controls.Add(this.textBoxMotorQuantCorrCylinder2);
            this.panelMotorUnevenRunning.Controls.Add(this.labelMotorQuantCorrCylinder2);
            this.panelMotorUnevenRunning.Controls.Add(this.textBoxMotorQuantCorrCylinder1);
            this.panelMotorUnevenRunning.Controls.Add(this.labelMotorQuantCorrCylinder1);
            resources.ApplyResources(this.panelMotorUnevenRunning, "panelMotorUnevenRunning");
            this.panelMotorUnevenRunning.Name = "panelMotorUnevenRunning";
            // 
            // labelIdleSpeedControlOn
            // 
            resources.ApplyResources(this.labelIdleSpeedControlOn, "labelIdleSpeedControlOn");
            this.labelIdleSpeedControlOn.Name = "labelIdleSpeedControlOn";
            // 
            // textBoxMotorQuantCorrCylinder4
            // 
            resources.ApplyResources(this.textBoxMotorQuantCorrCylinder4, "textBoxMotorQuantCorrCylinder4");
            this.textBoxMotorQuantCorrCylinder4.Name = "textBoxMotorQuantCorrCylinder4";
            this.textBoxMotorQuantCorrCylinder4.ReadOnly = true;
            this.textBoxMotorQuantCorrCylinder4.TabStop = false;
            // 
            // labelMotorQuantCorrCylinder4
            // 
            resources.ApplyResources(this.labelMotorQuantCorrCylinder4, "labelMotorQuantCorrCylinder4");
            this.labelMotorQuantCorrCylinder4.Name = "labelMotorQuantCorrCylinder4";
            // 
            // textBoxMotorQuantCorrCylinder3
            // 
            resources.ApplyResources(this.textBoxMotorQuantCorrCylinder3, "textBoxMotorQuantCorrCylinder3");
            this.textBoxMotorQuantCorrCylinder3.Name = "textBoxMotorQuantCorrCylinder3";
            this.textBoxMotorQuantCorrCylinder3.ReadOnly = true;
            this.textBoxMotorQuantCorrCylinder3.TabStop = false;
            // 
            // labelMotorQuantCorrCylinder3
            // 
            resources.ApplyResources(this.labelMotorQuantCorrCylinder3, "labelMotorQuantCorrCylinder3");
            this.labelMotorQuantCorrCylinder3.Name = "labelMotorQuantCorrCylinder3";
            // 
            // textBoxMotorQuantCorrCylinder2
            // 
            resources.ApplyResources(this.textBoxMotorQuantCorrCylinder2, "textBoxMotorQuantCorrCylinder2");
            this.textBoxMotorQuantCorrCylinder2.Name = "textBoxMotorQuantCorrCylinder2";
            this.textBoxMotorQuantCorrCylinder2.ReadOnly = true;
            this.textBoxMotorQuantCorrCylinder2.TabStop = false;
            // 
            // labelMotorQuantCorrCylinder2
            // 
            resources.ApplyResources(this.labelMotorQuantCorrCylinder2, "labelMotorQuantCorrCylinder2");
            this.labelMotorQuantCorrCylinder2.Name = "labelMotorQuantCorrCylinder2";
            // 
            // textBoxMotorQuantCorrCylinder1
            // 
            resources.ApplyResources(this.textBoxMotorQuantCorrCylinder1, "textBoxMotorQuantCorrCylinder1");
            this.textBoxMotorQuantCorrCylinder1.Name = "textBoxMotorQuantCorrCylinder1";
            this.textBoxMotorQuantCorrCylinder1.ReadOnly = true;
            this.textBoxMotorQuantCorrCylinder1.TabStop = false;
            // 
            // labelMotorQuantCorrCylinder1
            // 
            resources.ApplyResources(this.labelMotorQuantCorrCylinder1, "labelMotorQuantCorrCylinder1");
            this.labelMotorQuantCorrCylinder1.Name = "labelMotorQuantCorrCylinder1";
            // 
            // tabPageMotorRotIrregular
            // 
            this.tabPageMotorRotIrregular.Controls.Add(this.panelMotorRotIrregular);
            resources.ApplyResources(this.tabPageMotorRotIrregular, "tabPageMotorRotIrregular");
            this.tabPageMotorRotIrregular.Name = "tabPageMotorRotIrregular";
            // 
            // panelMotorRotIrregular
            // 
            this.panelMotorRotIrregular.Controls.Add(this.labelIdleSpeedControlOff);
            this.panelMotorRotIrregular.Controls.Add(this.textBoxMotorRpmCylinder4);
            this.panelMotorRotIrregular.Controls.Add(this.labelMotorRpmCylinder4);
            this.panelMotorRotIrregular.Controls.Add(this.textBoxMotorRpmCylinder3);
            this.panelMotorRotIrregular.Controls.Add(this.labelMotorRpmCylinder3);
            this.panelMotorRotIrregular.Controls.Add(this.textBoxMotorRpmCylinder2);
            this.panelMotorRotIrregular.Controls.Add(this.labelMotorRpmCylinder2);
            this.panelMotorRotIrregular.Controls.Add(this.textBoxMotorRpmCylinder1);
            this.panelMotorRotIrregular.Controls.Add(this.labelMotorRpmCylinder1);
            resources.ApplyResources(this.panelMotorRotIrregular, "panelMotorRotIrregular");
            this.panelMotorRotIrregular.Name = "panelMotorRotIrregular";
            // 
            // labelIdleSpeedControlOff
            // 
            resources.ApplyResources(this.labelIdleSpeedControlOff, "labelIdleSpeedControlOff");
            this.labelIdleSpeedControlOff.Name = "labelIdleSpeedControlOff";
            // 
            // textBoxMotorRpmCylinder4
            // 
            resources.ApplyResources(this.textBoxMotorRpmCylinder4, "textBoxMotorRpmCylinder4");
            this.textBoxMotorRpmCylinder4.Name = "textBoxMotorRpmCylinder4";
            this.textBoxMotorRpmCylinder4.ReadOnly = true;
            this.textBoxMotorRpmCylinder4.TabStop = false;
            // 
            // labelMotorRpmCylinder4
            // 
            resources.ApplyResources(this.labelMotorRpmCylinder4, "labelMotorRpmCylinder4");
            this.labelMotorRpmCylinder4.Name = "labelMotorRpmCylinder4";
            // 
            // textBoxMotorRpmCylinder3
            // 
            resources.ApplyResources(this.textBoxMotorRpmCylinder3, "textBoxMotorRpmCylinder3");
            this.textBoxMotorRpmCylinder3.Name = "textBoxMotorRpmCylinder3";
            this.textBoxMotorRpmCylinder3.ReadOnly = true;
            this.textBoxMotorRpmCylinder3.TabStop = false;
            // 
            // labelMotorRpmCylinder3
            // 
            resources.ApplyResources(this.labelMotorRpmCylinder3, "labelMotorRpmCylinder3");
            this.labelMotorRpmCylinder3.Name = "labelMotorRpmCylinder3";
            // 
            // textBoxMotorRpmCylinder2
            // 
            resources.ApplyResources(this.textBoxMotorRpmCylinder2, "textBoxMotorRpmCylinder2");
            this.textBoxMotorRpmCylinder2.Name = "textBoxMotorRpmCylinder2";
            this.textBoxMotorRpmCylinder2.ReadOnly = true;
            this.textBoxMotorRpmCylinder2.TabStop = false;
            // 
            // labelMotorRpmCylinder2
            // 
            resources.ApplyResources(this.labelMotorRpmCylinder2, "labelMotorRpmCylinder2");
            this.labelMotorRpmCylinder2.Name = "labelMotorRpmCylinder2";
            // 
            // textBoxMotorRpmCylinder1
            // 
            resources.ApplyResources(this.textBoxMotorRpmCylinder1, "textBoxMotorRpmCylinder1");
            this.textBoxMotorRpmCylinder1.Name = "textBoxMotorRpmCylinder1";
            this.textBoxMotorRpmCylinder1.ReadOnly = true;
            this.textBoxMotorRpmCylinder1.TabStop = false;
            // 
            // labelMotorRpmCylinder1
            // 
            resources.ApplyResources(this.labelMotorRpmCylinder1, "labelMotorRpmCylinder1");
            this.labelMotorRpmCylinder1.Name = "labelMotorRpmCylinder1";
            // 
            // tabPagePm
            // 
            this.tabPagePm.Controls.Add(this.panelPm);
            resources.ApplyResources(this.tabPagePm, "tabPagePm");
            this.tabPagePm.Name = "tabPagePm";
            // 
            // panelPm
            // 
            this.panelPm.Controls.Add(this.labelPmSocPercentD1);
            this.panelPm.Controls.Add(this.labelPmStartCapD1);
            this.panelPm.Controls.Add(this.labelPmSocQD1);
            this.panelPm.Controls.Add(this.textBoxPmSocPercentD1);
            this.panelPm.Controls.Add(this.textBoxPmStartCapD1);
            this.panelPm.Controls.Add(this.textBoxPmSocQD1);
            this.panelPm.Controls.Add(this.textBoxPmSocPercent);
            this.panelPm.Controls.Add(this.labelPmSocPercent);
            this.panelPm.Controls.Add(this.textBoxPmStartCap);
            this.panelPm.Controls.Add(this.labelPmStartCap);
            this.panelPm.Controls.Add(this.textBoxPmSocQ);
            this.panelPm.Controls.Add(this.labelPmSocQ);
            this.panelPm.Controls.Add(this.textBoxPmCalEvents);
            this.panelPm.Controls.Add(this.labelPmCalEvents);
            this.panelPm.Controls.Add(this.textBoxPmSeasonTemp);
            this.panelPm.Controls.Add(this.labelPmSeasonTemp);
            this.panelPm.Controls.Add(this.textBoxPmSocFit);
            this.panelPm.Controls.Add(this.labelPmSocFit);
            this.panelPm.Controls.Add(this.textBoxPmSoh);
            this.panelPm.Controls.Add(this.labelPmSoh);
            this.panelPm.Controls.Add(this.textBoxPmBatCap);
            this.panelPm.Controls.Add(this.labelPmBatCap);
            resources.ApplyResources(this.panelPm, "panelPm");
            this.panelPm.Name = "panelPm";
            // 
            // labelPmSocPercentD1
            // 
            resources.ApplyResources(this.labelPmSocPercentD1, "labelPmSocPercentD1");
            this.labelPmSocPercentD1.Name = "labelPmSocPercentD1";
            // 
            // labelPmStartCapD1
            // 
            resources.ApplyResources(this.labelPmStartCapD1, "labelPmStartCapD1");
            this.labelPmStartCapD1.Name = "labelPmStartCapD1";
            // 
            // labelPmSocQD1
            // 
            resources.ApplyResources(this.labelPmSocQD1, "labelPmSocQD1");
            this.labelPmSocQD1.Name = "labelPmSocQD1";
            // 
            // textBoxPmSocPercentD1
            // 
            resources.ApplyResources(this.textBoxPmSocPercentD1, "textBoxPmSocPercentD1");
            this.textBoxPmSocPercentD1.Name = "textBoxPmSocPercentD1";
            this.textBoxPmSocPercentD1.ReadOnly = true;
            this.textBoxPmSocPercentD1.TabStop = false;
            // 
            // textBoxPmStartCapD1
            // 
            resources.ApplyResources(this.textBoxPmStartCapD1, "textBoxPmStartCapD1");
            this.textBoxPmStartCapD1.Name = "textBoxPmStartCapD1";
            this.textBoxPmStartCapD1.ReadOnly = true;
            this.textBoxPmStartCapD1.TabStop = false;
            // 
            // textBoxPmSocQD1
            // 
            resources.ApplyResources(this.textBoxPmSocQD1, "textBoxPmSocQD1");
            this.textBoxPmSocQD1.Name = "textBoxPmSocQD1";
            this.textBoxPmSocQD1.ReadOnly = true;
            this.textBoxPmSocQD1.TabStop = false;
            // 
            // textBoxPmSocPercent
            // 
            resources.ApplyResources(this.textBoxPmSocPercent, "textBoxPmSocPercent");
            this.textBoxPmSocPercent.Name = "textBoxPmSocPercent";
            this.textBoxPmSocPercent.ReadOnly = true;
            this.textBoxPmSocPercent.TabStop = false;
            // 
            // labelPmSocPercent
            // 
            resources.ApplyResources(this.labelPmSocPercent, "labelPmSocPercent");
            this.labelPmSocPercent.Name = "labelPmSocPercent";
            // 
            // textBoxPmStartCap
            // 
            resources.ApplyResources(this.textBoxPmStartCap, "textBoxPmStartCap");
            this.textBoxPmStartCap.Name = "textBoxPmStartCap";
            this.textBoxPmStartCap.ReadOnly = true;
            this.textBoxPmStartCap.TabStop = false;
            // 
            // labelPmStartCap
            // 
            resources.ApplyResources(this.labelPmStartCap, "labelPmStartCap");
            this.labelPmStartCap.Name = "labelPmStartCap";
            // 
            // textBoxPmSocQ
            // 
            resources.ApplyResources(this.textBoxPmSocQ, "textBoxPmSocQ");
            this.textBoxPmSocQ.Name = "textBoxPmSocQ";
            this.textBoxPmSocQ.ReadOnly = true;
            this.textBoxPmSocQ.TabStop = false;
            // 
            // labelPmSocQ
            // 
            resources.ApplyResources(this.labelPmSocQ, "labelPmSocQ");
            this.labelPmSocQ.Name = "labelPmSocQ";
            // 
            // textBoxPmCalEvents
            // 
            resources.ApplyResources(this.textBoxPmCalEvents, "textBoxPmCalEvents");
            this.textBoxPmCalEvents.Name = "textBoxPmCalEvents";
            this.textBoxPmCalEvents.ReadOnly = true;
            this.textBoxPmCalEvents.TabStop = false;
            // 
            // labelPmCalEvents
            // 
            resources.ApplyResources(this.labelPmCalEvents, "labelPmCalEvents");
            this.labelPmCalEvents.Name = "labelPmCalEvents";
            // 
            // textBoxPmSeasonTemp
            // 
            resources.ApplyResources(this.textBoxPmSeasonTemp, "textBoxPmSeasonTemp");
            this.textBoxPmSeasonTemp.Name = "textBoxPmSeasonTemp";
            this.textBoxPmSeasonTemp.ReadOnly = true;
            this.textBoxPmSeasonTemp.TabStop = false;
            // 
            // labelPmSeasonTemp
            // 
            resources.ApplyResources(this.labelPmSeasonTemp, "labelPmSeasonTemp");
            this.labelPmSeasonTemp.Name = "labelPmSeasonTemp";
            // 
            // textBoxPmSocFit
            // 
            resources.ApplyResources(this.textBoxPmSocFit, "textBoxPmSocFit");
            this.textBoxPmSocFit.Name = "textBoxPmSocFit";
            this.textBoxPmSocFit.ReadOnly = true;
            this.textBoxPmSocFit.TabStop = false;
            // 
            // labelPmSocFit
            // 
            resources.ApplyResources(this.labelPmSocFit, "labelPmSocFit");
            this.labelPmSocFit.Name = "labelPmSocFit";
            // 
            // textBoxPmSoh
            // 
            resources.ApplyResources(this.textBoxPmSoh, "textBoxPmSoh");
            this.textBoxPmSoh.Name = "textBoxPmSoh";
            this.textBoxPmSoh.ReadOnly = true;
            this.textBoxPmSoh.TabStop = false;
            // 
            // labelPmSoh
            // 
            resources.ApplyResources(this.labelPmSoh, "labelPmSoh");
            this.labelPmSoh.Name = "labelPmSoh";
            // 
            // textBoxPmBatCap
            // 
            resources.ApplyResources(this.textBoxPmBatCap, "textBoxPmBatCap");
            this.textBoxPmBatCap.Name = "textBoxPmBatCap";
            this.textBoxPmBatCap.ReadOnly = true;
            this.textBoxPmBatCap.TabStop = false;
            // 
            // labelPmBatCap
            // 
            resources.ApplyResources(this.labelPmBatCap, "labelPmBatCap");
            this.labelPmBatCap.Name = "labelPmBatCap";
            // 
            // tabPageCccNav
            // 
            this.tabPageCccNav.Controls.Add(this.panelCccNav);
            resources.ApplyResources(this.tabPageCccNav, "tabPageCccNav");
            this.tabPageCccNav.Name = "tabPageCccNav";
            // 
            // panelCccNav
            // 
            this.panelCccNav.Controls.Add(this.labelCccNavResVert);
            this.panelCccNav.Controls.Add(this.textBoxCccNavResVert);
            this.panelCccNav.Controls.Add(this.textBoxCccNavResHorz);
            this.panelCccNav.Controls.Add(this.labelCccNavResHorz);
            this.panelCccNav.Controls.Add(this.textBoxCccNavResPos);
            this.panelCccNav.Controls.Add(this.labelCccNavResPos);
            this.panelCccNav.Controls.Add(this.textBoxCccNavGpsDateTime);
            this.panelCccNav.Controls.Add(this.labelCccNavGpsDateTime);
            this.panelCccNav.Controls.Add(this.textBoxCccNavSpeed);
            this.panelCccNav.Controls.Add(this.labelCccNavSpeed);
            this.panelCccNav.Controls.Add(this.checkBoxHipDriver);
            this.panelCccNav.Controls.Add(this.checkBoxCccNavAlmanach);
            this.panelCccNav.Controls.Add(this.labelCccNavPosType);
            this.panelCccNav.Controls.Add(this.textBoxCccNavPosType);
            this.panelCccNav.Controls.Add(this.textBoxCccNavPosHeight);
            this.panelCccNav.Controls.Add(this.labelCccNavPosHeight);
            this.panelCccNav.Controls.Add(this.textBoxCccNavPosLong);
            this.panelCccNav.Controls.Add(this.labelCccNavPosLong);
            this.panelCccNav.Controls.Add(this.textBoxCccNavPosLat);
            this.panelCccNav.Controls.Add(this.labelCccNavPosLat);
            resources.ApplyResources(this.panelCccNav, "panelCccNav");
            this.panelCccNav.Name = "panelCccNav";
            // 
            // labelCccNavResVert
            // 
            resources.ApplyResources(this.labelCccNavResVert, "labelCccNavResVert");
            this.labelCccNavResVert.Name = "labelCccNavResVert";
            // 
            // textBoxCccNavResVert
            // 
            resources.ApplyResources(this.textBoxCccNavResVert, "textBoxCccNavResVert");
            this.textBoxCccNavResVert.Name = "textBoxCccNavResVert";
            this.textBoxCccNavResVert.ReadOnly = true;
            this.textBoxCccNavResVert.TabStop = false;
            // 
            // textBoxCccNavResHorz
            // 
            resources.ApplyResources(this.textBoxCccNavResHorz, "textBoxCccNavResHorz");
            this.textBoxCccNavResHorz.Name = "textBoxCccNavResHorz";
            this.textBoxCccNavResHorz.ReadOnly = true;
            this.textBoxCccNavResHorz.TabStop = false;
            // 
            // labelCccNavResHorz
            // 
            resources.ApplyResources(this.labelCccNavResHorz, "labelCccNavResHorz");
            this.labelCccNavResHorz.Name = "labelCccNavResHorz";
            // 
            // textBoxCccNavResPos
            // 
            resources.ApplyResources(this.textBoxCccNavResPos, "textBoxCccNavResPos");
            this.textBoxCccNavResPos.Name = "textBoxCccNavResPos";
            this.textBoxCccNavResPos.ReadOnly = true;
            this.textBoxCccNavResPos.TabStop = false;
            // 
            // labelCccNavResPos
            // 
            resources.ApplyResources(this.labelCccNavResPos, "labelCccNavResPos");
            this.labelCccNavResPos.Name = "labelCccNavResPos";
            // 
            // textBoxCccNavGpsDateTime
            // 
            resources.ApplyResources(this.textBoxCccNavGpsDateTime, "textBoxCccNavGpsDateTime");
            this.textBoxCccNavGpsDateTime.Name = "textBoxCccNavGpsDateTime";
            this.textBoxCccNavGpsDateTime.ReadOnly = true;
            this.textBoxCccNavGpsDateTime.TabStop = false;
            // 
            // labelCccNavGpsDateTime
            // 
            resources.ApplyResources(this.labelCccNavGpsDateTime, "labelCccNavGpsDateTime");
            this.labelCccNavGpsDateTime.Name = "labelCccNavGpsDateTime";
            // 
            // textBoxCccNavSpeed
            // 
            resources.ApplyResources(this.textBoxCccNavSpeed, "textBoxCccNavSpeed");
            this.textBoxCccNavSpeed.Name = "textBoxCccNavSpeed";
            this.textBoxCccNavSpeed.ReadOnly = true;
            this.textBoxCccNavSpeed.TabStop = false;
            // 
            // labelCccNavSpeed
            // 
            resources.ApplyResources(this.labelCccNavSpeed, "labelCccNavSpeed");
            this.labelCccNavSpeed.Name = "labelCccNavSpeed";
            // 
            // checkBoxHipDriver
            // 
            this.checkBoxHipDriver.AutoCheck = false;
            resources.ApplyResources(this.checkBoxHipDriver, "checkBoxHipDriver");
            this.checkBoxHipDriver.Name = "checkBoxHipDriver";
            this.checkBoxHipDriver.TabStop = false;
            // 
            // checkBoxCccNavAlmanach
            // 
            this.checkBoxCccNavAlmanach.AutoCheck = false;
            resources.ApplyResources(this.checkBoxCccNavAlmanach, "checkBoxCccNavAlmanach");
            this.checkBoxCccNavAlmanach.Name = "checkBoxCccNavAlmanach";
            this.checkBoxCccNavAlmanach.TabStop = false;
            // 
            // labelCccNavPosType
            // 
            resources.ApplyResources(this.labelCccNavPosType, "labelCccNavPosType");
            this.labelCccNavPosType.Name = "labelCccNavPosType";
            // 
            // textBoxCccNavPosType
            // 
            resources.ApplyResources(this.textBoxCccNavPosType, "textBoxCccNavPosType");
            this.textBoxCccNavPosType.Name = "textBoxCccNavPosType";
            this.textBoxCccNavPosType.ReadOnly = true;
            this.textBoxCccNavPosType.TabStop = false;
            // 
            // textBoxCccNavPosHeight
            // 
            resources.ApplyResources(this.textBoxCccNavPosHeight, "textBoxCccNavPosHeight");
            this.textBoxCccNavPosHeight.Name = "textBoxCccNavPosHeight";
            this.textBoxCccNavPosHeight.ReadOnly = true;
            this.textBoxCccNavPosHeight.TabStop = false;
            // 
            // labelCccNavPosHeight
            // 
            resources.ApplyResources(this.labelCccNavPosHeight, "labelCccNavPosHeight");
            this.labelCccNavPosHeight.Name = "labelCccNavPosHeight";
            // 
            // textBoxCccNavPosLong
            // 
            resources.ApplyResources(this.textBoxCccNavPosLong, "textBoxCccNavPosLong");
            this.textBoxCccNavPosLong.Name = "textBoxCccNavPosLong";
            this.textBoxCccNavPosLong.ReadOnly = true;
            this.textBoxCccNavPosLong.TabStop = false;
            // 
            // labelCccNavPosLong
            // 
            resources.ApplyResources(this.labelCccNavPosLong, "labelCccNavPosLong");
            this.labelCccNavPosLong.Name = "labelCccNavPosLong";
            // 
            // textBoxCccNavPosLat
            // 
            resources.ApplyResources(this.textBoxCccNavPosLat, "textBoxCccNavPosLat");
            this.textBoxCccNavPosLat.Name = "textBoxCccNavPosLat";
            this.textBoxCccNavPosLat.ReadOnly = true;
            this.textBoxCccNavPosLat.TabStop = false;
            // 
            // labelCccNavPosLat
            // 
            resources.ApplyResources(this.labelCccNavPosLat, "labelCccNavPosLat");
            this.labelCccNavPosLat.Name = "labelCccNavPosLat";
            // 
            // tabPageIhk
            // 
            this.tabPageIhk.Controls.Add(this.panelIhk);
            resources.ApplyResources(this.tabPageIhk, "tabPageIhk");
            this.tabPageIhk.Name = "tabPageIhk";
            // 
            // panelIhk
            // 
            this.panelIhk.Controls.Add(this.textBoxIhkHeatExSetpoint);
            this.panelIhk.Controls.Add(this.labelIhkHeatExSetpoint);
            this.panelIhk.Controls.Add(this.textBoxIhkHeatExTemp);
            this.panelIhk.Controls.Add(this.labelIhkHeatExTemp);
            this.panelIhk.Controls.Add(this.textBoxIhkSetpoint);
            this.panelIhk.Controls.Add(this.labelIhkSetpoint);
            this.panelIhk.Controls.Add(this.textBoxIhkOutTemp);
            this.panelIhk.Controls.Add(this.labelIhkOutTemp);
            this.panelIhk.Controls.Add(this.textBoxIhkInTempDelay);
            this.panelIhk.Controls.Add(this.labelIhkInTempDelay);
            this.panelIhk.Controls.Add(this.textBoxIhkInTemp);
            this.panelIhk.Controls.Add(this.labelIhkInTemp);
            resources.ApplyResources(this.panelIhk, "panelIhk");
            this.panelIhk.Name = "panelIhk";
            // 
            // textBoxIhkHeatExSetpoint
            // 
            resources.ApplyResources(this.textBoxIhkHeatExSetpoint, "textBoxIhkHeatExSetpoint");
            this.textBoxIhkHeatExSetpoint.Name = "textBoxIhkHeatExSetpoint";
            this.textBoxIhkHeatExSetpoint.ReadOnly = true;
            this.textBoxIhkHeatExSetpoint.TabStop = false;
            // 
            // labelIhkHeatExSetpoint
            // 
            resources.ApplyResources(this.labelIhkHeatExSetpoint, "labelIhkHeatExSetpoint");
            this.labelIhkHeatExSetpoint.Name = "labelIhkHeatExSetpoint";
            // 
            // textBoxIhkHeatExTemp
            // 
            resources.ApplyResources(this.textBoxIhkHeatExTemp, "textBoxIhkHeatExTemp");
            this.textBoxIhkHeatExTemp.Name = "textBoxIhkHeatExTemp";
            this.textBoxIhkHeatExTemp.ReadOnly = true;
            this.textBoxIhkHeatExTemp.TabStop = false;
            // 
            // labelIhkHeatExTemp
            // 
            resources.ApplyResources(this.labelIhkHeatExTemp, "labelIhkHeatExTemp");
            this.labelIhkHeatExTemp.Name = "labelIhkHeatExTemp";
            // 
            // textBoxIhkSetpoint
            // 
            resources.ApplyResources(this.textBoxIhkSetpoint, "textBoxIhkSetpoint");
            this.textBoxIhkSetpoint.Name = "textBoxIhkSetpoint";
            this.textBoxIhkSetpoint.ReadOnly = true;
            this.textBoxIhkSetpoint.TabStop = false;
            // 
            // labelIhkSetpoint
            // 
            resources.ApplyResources(this.labelIhkSetpoint, "labelIhkSetpoint");
            this.labelIhkSetpoint.Name = "labelIhkSetpoint";
            // 
            // textBoxIhkOutTemp
            // 
            resources.ApplyResources(this.textBoxIhkOutTemp, "textBoxIhkOutTemp");
            this.textBoxIhkOutTemp.Name = "textBoxIhkOutTemp";
            this.textBoxIhkOutTemp.ReadOnly = true;
            this.textBoxIhkOutTemp.TabStop = false;
            // 
            // labelIhkOutTemp
            // 
            resources.ApplyResources(this.labelIhkOutTemp, "labelIhkOutTemp");
            this.labelIhkOutTemp.Name = "labelIhkOutTemp";
            // 
            // textBoxIhkInTempDelay
            // 
            resources.ApplyResources(this.textBoxIhkInTempDelay, "textBoxIhkInTempDelay");
            this.textBoxIhkInTempDelay.Name = "textBoxIhkInTempDelay";
            this.textBoxIhkInTempDelay.ReadOnly = true;
            this.textBoxIhkInTempDelay.TabStop = false;
            // 
            // labelIhkInTempDelay
            // 
            resources.ApplyResources(this.labelIhkInTempDelay, "labelIhkInTempDelay");
            this.labelIhkInTempDelay.Name = "labelIhkInTempDelay";
            // 
            // textBoxIhkInTemp
            // 
            resources.ApplyResources(this.textBoxIhkInTemp, "textBoxIhkInTemp");
            this.textBoxIhkInTemp.Name = "textBoxIhkInTemp";
            this.textBoxIhkInTemp.ReadOnly = true;
            this.textBoxIhkInTemp.TabStop = false;
            // 
            // labelIhkInTemp
            // 
            resources.ApplyResources(this.labelIhkInTemp, "labelIhkInTemp");
            this.labelIhkInTemp.Name = "labelIhkInTemp";
            // 
            // tabPageErrors
            // 
            this.tabPageErrors.Controls.Add(this.panelErrors);
            resources.ApplyResources(this.tabPageErrors, "tabPageErrors");
            this.tabPageErrors.Name = "tabPageErrors";
            // 
            // panelErrors
            // 
            this.panelErrors.Controls.Add(this.textBoxErrors2);
            this.panelErrors.Controls.Add(this.textBoxErrors1);
            resources.ApplyResources(this.panelErrors, "panelErrors");
            this.panelErrors.Name = "panelErrors";
            // 
            // textBoxErrors2
            // 
            resources.ApplyResources(this.textBoxErrors2, "textBoxErrors2");
            this.textBoxErrors2.Name = "textBoxErrors2";
            this.textBoxErrors2.ReadOnly = true;
            this.textBoxErrors2.TabStop = false;
            // 
            // textBoxErrors1
            // 
            resources.ApplyResources(this.textBoxErrors1, "textBoxErrors1");
            this.textBoxErrors1.Name = "textBoxErrors1";
            this.textBoxErrors1.ReadOnly = true;
            this.textBoxErrors1.TabStop = false;
            // 
            // tabPageAdapterConfig
            // 
            this.tabPageAdapterConfig.Controls.Add(this.panel1);
            resources.ApplyResources(this.tabPageAdapterConfig, "tabPageAdapterConfig");
            this.tabPageAdapterConfig.Name = "tabPageAdapterConfig";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonAdapterConfigCanOff);
            this.panel1.Controls.Add(this.buttonAdapterConfigCan100);
            this.panel1.Controls.Add(this.buttonAdapterConfigCan500);
            this.panel1.Controls.Add(this.textBoxAdapterConfigResult);
            this.panel1.Controls.Add(this.labelAdapterConfigResult);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // buttonAdapterConfigCanOff
            // 
            resources.ApplyResources(this.buttonAdapterConfigCanOff, "buttonAdapterConfigCanOff");
            this.buttonAdapterConfigCanOff.Name = "buttonAdapterConfigCanOff";
            this.buttonAdapterConfigCanOff.Click += new System.EventHandler(this.buttonAdapterConfig_Click);
            // 
            // buttonAdapterConfigCan100
            // 
            resources.ApplyResources(this.buttonAdapterConfigCan100, "buttonAdapterConfigCan100");
            this.buttonAdapterConfigCan100.Name = "buttonAdapterConfigCan100";
            this.buttonAdapterConfigCan100.Click += new System.EventHandler(this.buttonAdapterConfig_Click);
            // 
            // buttonAdapterConfigCan500
            // 
            resources.ApplyResources(this.buttonAdapterConfigCan500, "buttonAdapterConfigCan500");
            this.buttonAdapterConfigCan500.Name = "buttonAdapterConfigCan500";
            this.buttonAdapterConfigCan500.Click += new System.EventHandler(this.buttonAdapterConfig_Click);
            // 
            // textBoxAdapterConfigResult
            // 
            resources.ApplyResources(this.textBoxAdapterConfigResult, "textBoxAdapterConfigResult");
            this.textBoxAdapterConfigResult.Name = "textBoxAdapterConfigResult";
            this.textBoxAdapterConfigResult.ReadOnly = true;
            this.textBoxAdapterConfigResult.TabStop = false;
            // 
            // labelAdapterConfigResult
            // 
            resources.ApplyResources(this.labelAdapterConfigResult, "labelAdapterConfigResult");
            this.labelAdapterConfigResult.Name = "labelAdapterConfigResult";
            // 
            // tabPageTest
            // 
            this.tabPageTest.Controls.Add(this.panelTest);
            resources.ApplyResources(this.tabPageTest, "tabPageTest");
            this.tabPageTest.Name = "tabPageTest";
            // 
            // panelTest
            // 
            this.panelTest.Controls.Add(this.textBoxTest);
            resources.ApplyResources(this.panelTest, "panelTest");
            this.panelTest.Name = "panelTest";
            // 
            // textBoxTest
            // 
            resources.ApplyResources(this.textBoxTest, "textBoxTest");
            this.textBoxTest.Name = "textBoxTest";
            this.textBoxTest.ReadOnly = true;
            this.textBoxTest.TabStop = false;
            // 
            // buttonPowerOff
            // 
            resources.ApplyResources(this.buttonPowerOff, "buttonPowerOff");
            this.buttonPowerOff.Name = "buttonPowerOff";
            this.buttonPowerOff.Click += new System.EventHandler(this.buttonPowerOff_Click);
            // 
            // labelBatteryLife
            // 
            resources.ApplyResources(this.labelBatteryLife, "labelBatteryLife");
            this.labelBatteryLife.Name = "labelBatteryLife";
            // 
            // pushButtonWlan
            // 
            this.pushButtonWlan.ButtonState = false;
            resources.ApplyResources(this.pushButtonWlan, "pushButtonWlan");
            this.pushButtonWlan.Name = "pushButtonWlan";
            this.pushButtonWlan.Click += new System.EventHandler(this.pushButtonWlan_Click);
            // 
            // groupBoxControl
            // 
            this.groupBoxControl.Controls.Add(this.pushButtonDown);
            this.groupBoxControl.Controls.Add(this.pushButtonUp);
            resources.ApplyResources(this.groupBoxControl, "groupBoxControl");
            this.groupBoxControl.Name = "groupBoxControl";
            // 
            // pushButtonDown
            // 
            this.pushButtonDown.ButtonState = false;
            resources.ApplyResources(this.pushButtonDown, "pushButtonDown");
            this.pushButtonDown.Name = "pushButtonDown";
            this.pushButtonDown.Click += new System.EventHandler(this.pushButtonDown_Click);
            // 
            // pushButtonUp
            // 
            this.pushButtonUp.ButtonState = false;
            resources.ApplyResources(this.pushButtonUp, "pushButtonUp");
            this.pushButtonUp.Name = "pushButtonUp";
            this.pushButtonUp.Click += new System.EventHandler(this.pushButtonUp_Click);
            // 
            // groupBoxStatus
            // 
            this.groupBoxStatus.Controls.Add(this.panelAxisData);
            resources.ApplyResources(this.groupBoxStatus, "groupBoxStatus");
            this.groupBoxStatus.Name = "groupBoxStatus";
            // 
            // panelAxisData
            // 
            this.panelAxisData.Controls.Add(this.textBoxSpeed);
            this.panelAxisData.Controls.Add(this.labelSpeed);
            this.panelAxisData.Controls.Add(this.textBoxAxisBatteryVoltage);
            this.panelAxisData.Controls.Add(this.labelAxisBatteryVoltage);
            this.panelAxisData.Controls.Add(this.textBoxValveState);
            this.panelAxisData.Controls.Add(this.labelOutputState);
            this.panelAxisData.Controls.Add(this.textBoxAxisMode);
            this.panelAxisData.Controls.Add(this.labelAxisMode);
            this.panelAxisData.Controls.Add(this.labelAxisLeft);
            this.panelAxisData.Controls.Add(this.textBoxAxisRigth);
            this.panelAxisData.Controls.Add(this.textBoxAxisLeft);
            this.panelAxisData.Controls.Add(this.labelAxisRight);
            resources.ApplyResources(this.panelAxisData, "panelAxisData");
            this.panelAxisData.Name = "panelAxisData";
            // 
            // textBoxSpeed
            // 
            resources.ApplyResources(this.textBoxSpeed, "textBoxSpeed");
            this.textBoxSpeed.Name = "textBoxSpeed";
            this.textBoxSpeed.ReadOnly = true;
            this.textBoxSpeed.TabStop = false;
            // 
            // labelSpeed
            // 
            resources.ApplyResources(this.labelSpeed, "labelSpeed");
            this.labelSpeed.Name = "labelSpeed";
            // 
            // textBoxAxisBatteryVoltage
            // 
            resources.ApplyResources(this.textBoxAxisBatteryVoltage, "textBoxAxisBatteryVoltage");
            this.textBoxAxisBatteryVoltage.Name = "textBoxAxisBatteryVoltage";
            this.textBoxAxisBatteryVoltage.ReadOnly = true;
            this.textBoxAxisBatteryVoltage.TabStop = false;
            // 
            // labelAxisBatteryVoltage
            // 
            resources.ApplyResources(this.labelAxisBatteryVoltage, "labelAxisBatteryVoltage");
            this.labelAxisBatteryVoltage.Name = "labelAxisBatteryVoltage";
            // 
            // textBoxValveState
            // 
            resources.ApplyResources(this.textBoxValveState, "textBoxValveState");
            this.textBoxValveState.Name = "textBoxValveState";
            this.textBoxValveState.ReadOnly = true;
            this.textBoxValveState.TabStop = false;
            // 
            // labelOutputState
            // 
            resources.ApplyResources(this.labelOutputState, "labelOutputState");
            this.labelOutputState.Name = "labelOutputState";
            // 
            // textBoxAxisMode
            // 
            resources.ApplyResources(this.textBoxAxisMode, "textBoxAxisMode");
            this.textBoxAxisMode.Name = "textBoxAxisMode";
            this.textBoxAxisMode.ReadOnly = true;
            this.textBoxAxisMode.TabStop = false;
            // 
            // labelAxisMode
            // 
            resources.ApplyResources(this.labelAxisMode, "labelAxisMode");
            this.labelAxisMode.Name = "labelAxisMode";
            // 
            // labelAxisLeft
            // 
            resources.ApplyResources(this.labelAxisLeft, "labelAxisLeft");
            this.labelAxisLeft.Name = "labelAxisLeft";
            // 
            // textBoxAxisRigth
            // 
            resources.ApplyResources(this.textBoxAxisRigth, "textBoxAxisRigth");
            this.textBoxAxisRigth.Name = "textBoxAxisRigth";
            this.textBoxAxisRigth.ReadOnly = true;
            this.textBoxAxisRigth.TabStop = false;
            // 
            // textBoxAxisLeft
            // 
            resources.ApplyResources(this.textBoxAxisLeft, "textBoxAxisLeft");
            this.textBoxAxisLeft.Name = "textBoxAxisLeft";
            this.textBoxAxisLeft.ReadOnly = true;
            this.textBoxAxisLeft.TabStop = false;
            // 
            // labelAxisRight
            // 
            resources.ApplyResources(this.labelAxisRight, "labelAxisRight");
            this.labelAxisRight.Name = "labelAxisRight";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.labelBatteryLife);
            this.Controls.Add(this.pushButtonWlan);
            this.Controls.Add(this.buttonPowerOff);
            this.Controls.Add(this.tabControlDevice);
            this.Controls.Add(this.buttonStoreLog);
            this.Controls.Add(this.checkBoxLogFile);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.listPorts);
            this.Controls.Add(this.labelErrorCount);
            this.Controls.Add(this.textBoxErrorCount);
            this.Controls.Add(this.checkBoxConnected);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.Closed += new System.EventHandler(this.MainForm_Closed);
            this.tabControlDevice.ResumeLayout(false);
            this.tabPageAxis.ResumeLayout(false);
            this.tabPageMotor.ResumeLayout(false);
            this.panelMotorData.ResumeLayout(false);
            this.tabPageMotorUnevenRunning.ResumeLayout(false);
            this.panelMotorUnevenRunning.ResumeLayout(false);
            this.tabPageMotorRotIrregular.ResumeLayout(false);
            this.panelMotorRotIrregular.ResumeLayout(false);
            this.tabPagePm.ResumeLayout(false);
            this.panelPm.ResumeLayout(false);
            this.tabPageCccNav.ResumeLayout(false);
            this.panelCccNav.ResumeLayout(false);
            this.tabPageIhk.ResumeLayout(false);
            this.panelIhk.ResumeLayout(false);
            this.tabPageErrors.ResumeLayout(false);
            this.panelErrors.ResumeLayout(false);
            this.tabPageAdapterConfig.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tabPageTest.ResumeLayout(false);
            this.panelTest.ResumeLayout(false);
            this.groupBoxControl.ResumeLayout(false);
            this.groupBoxStatus.ResumeLayout(false);
            this.panelAxisData.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listPorts;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Timer timerUpdate;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Label labelAxisLeft;
        private System.Windows.Forms.TextBox textBoxAxisLeft;
        private System.Windows.Forms.Label labelAxisRight;
        private System.Windows.Forms.TextBox textBoxAxisRigth;
        private System.Windows.Forms.Panel panelAxisData;
        private System.Windows.Forms.CheckBox checkBoxConnected;
        private System.Windows.Forms.TextBox textBoxErrorCount;
        private System.Windows.Forms.Label labelErrorCount;
        private System.Windows.Forms.TextBox textBoxAxisMode;
        private System.Windows.Forms.Label labelAxisMode;
        private System.Windows.Forms.TextBox textBoxValveState;
        private System.Windows.Forms.Label labelOutputState;
        private PushButton pushButtonDown;
        private PushButton pushButtonUp;
        private GroupBox groupBoxStatus;
        private GroupBox groupBoxControl;
        private System.Windows.Forms.CheckBox checkBoxLogFile;
        private System.Windows.Forms.Button buttonStoreLog;
        private System.Windows.Forms.TextBox textBoxAxisBatteryVoltage;
        private System.Windows.Forms.Label labelAxisBatteryVoltage;
        private System.Windows.Forms.TextBox textBoxSpeed;
        private System.Windows.Forms.Label labelSpeed;
        private System.Windows.Forms.TabControl tabControlDevice;
        private System.Windows.Forms.TabPage tabPageAxis;
        private System.Windows.Forms.TabPage tabPageMotor;
        private System.Windows.Forms.Panel panelMotorData;
        private System.Windows.Forms.TextBox textBoxMotorBatteryVoltage;
        private System.Windows.Forms.Label labelMotorBatteryVoltage;
        private System.Windows.Forms.TextBox textBoxMotorAirMass;
        private System.Windows.Forms.Label labelMotorAirMass;
        private System.Windows.Forms.TextBox textBoxMotorTemp;
        private System.Windows.Forms.Label labelMotorTemp;
        private System.Windows.Forms.TextBox textBoxMotorIntakeAirTemp;
        private System.Windows.Forms.Label labelMotorIntakeAirTemp;
        private System.Windows.Forms.TextBox textBoxMotorBoostPressSet;
        private System.Windows.Forms.Label labelMotorBoostPressSet;
        private System.Windows.Forms.TextBox textBoxMotorBoostPressAct;
        private System.Windows.Forms.Label labelMotorBoostPressAct;
        private System.Windows.Forms.TextBox textBoxMotorRailPressAct;
        private System.Windows.Forms.Label labelMotorRailPressAct;
        private System.Windows.Forms.TextBox textBoxMotorRailPressSet;
        private System.Windows.Forms.Label labelMotorRailPressSet;
        private System.Windows.Forms.TextBox textBoxMotorAirMassAct;
        private System.Windows.Forms.Label labelMotorAirMassAct;
        private System.Windows.Forms.TextBox textBoxMotorAirMassSet;
        private System.Windows.Forms.Label labelMotorAirMassSet;
        private System.Windows.Forms.CheckBox checkBoxMotorPartFilterRequest;
        private System.Windows.Forms.CheckBox checkBoxOilPressSwt;
        private System.Windows.Forms.TabPage tabPageMotorUnevenRunning;
        private System.Windows.Forms.Panel panelMotorUnevenRunning;
        private System.Windows.Forms.TextBox textBoxMotorQuantCorrCylinder2;
        private System.Windows.Forms.Label labelMotorQuantCorrCylinder2;
        private System.Windows.Forms.TextBox textBoxMotorQuantCorrCylinder1;
        private System.Windows.Forms.Label labelMotorQuantCorrCylinder1;
        private System.Windows.Forms.TextBox textBoxMotorQuantCorrCylinder4;
        private System.Windows.Forms.Label labelMotorQuantCorrCylinder4;
        private System.Windows.Forms.TextBox textBoxMotorQuantCorrCylinder3;
        private System.Windows.Forms.Label labelMotorQuantCorrCylinder3;
        private System.Windows.Forms.TabPage tabPageMotorRotIrregular;
        private System.Windows.Forms.Panel panelMotorRotIrregular;
        private System.Windows.Forms.TextBox textBoxMotorRpmCylinder4;
        private System.Windows.Forms.Label labelMotorRpmCylinder4;
        private System.Windows.Forms.TextBox textBoxMotorRpmCylinder3;
        private System.Windows.Forms.Label labelMotorRpmCylinder3;
        private System.Windows.Forms.TextBox textBoxMotorRpmCylinder2;
        private System.Windows.Forms.Label labelMotorRpmCylinder2;
        private System.Windows.Forms.TextBox textBoxMotorRpmCylinder1;
        private System.Windows.Forms.Label labelMotorRpmCylinder1;
        private System.Windows.Forms.Label labelIdleSpeedControlOn;
        private System.Windows.Forms.Label labelIdleSpeedControlOff;
        private System.Windows.Forms.TabPage tabPagePm;
        private System.Windows.Forms.Panel panelPm;
        private System.Windows.Forms.TextBox textBoxPmBatCap;
        private System.Windows.Forms.Label labelPmBatCap;
        private System.Windows.Forms.TextBox textBoxPmSoh;
        private System.Windows.Forms.Label labelPmSoh;
        private System.Windows.Forms.TextBox textBoxPmSocFit;
        private System.Windows.Forms.Label labelPmSocFit;
        private System.Windows.Forms.TextBox textBoxPmSeasonTemp;
        private System.Windows.Forms.Label labelPmSeasonTemp;
        private System.Windows.Forms.TextBox textBoxPmCalEvents;
        private System.Windows.Forms.Label labelPmCalEvents;
        private System.Windows.Forms.TextBox textBoxPmSocQ;
        private System.Windows.Forms.Label labelPmSocQ;
        private System.Windows.Forms.TextBox textBoxPmStartCap;
        private System.Windows.Forms.Label labelPmStartCap;
        private System.Windows.Forms.TextBox textBoxPmSocPercent;
        private System.Windows.Forms.Label labelPmSocPercent;
        private System.Windows.Forms.Label labelPmSocQD1;
        private System.Windows.Forms.TextBox textBoxPmSocPercentD1;
        private System.Windows.Forms.TextBox textBoxPmStartCapD1;
        private System.Windows.Forms.TextBox textBoxPmSocQD1;
        private System.Windows.Forms.Label labelPmSocPercentD1;
        private System.Windows.Forms.Label labelPmStartCapD1;
        private System.Windows.Forms.Button buttonPowerOff;
        private PushButton pushButtonWlan;
        private System.Windows.Forms.Label labelBatteryLife;
        private System.Windows.Forms.TabPage tabPageCccNav;
        private System.Windows.Forms.Panel panelCccNav;
        private System.Windows.Forms.TextBox textBoxCccNavPosLat;
        private System.Windows.Forms.Label labelCccNavPosLat;
        private System.Windows.Forms.TextBox textBoxCccNavPosLong;
        private System.Windows.Forms.Label labelCccNavPosLong;
        private System.Windows.Forms.Label labelCccNavPosHeight;
        private System.Windows.Forms.TextBox textBoxCccNavPosHeight;
        private System.Windows.Forms.TextBox textBoxCccNavPosType;
        private System.Windows.Forms.Label labelCccNavPosType;
        private System.Windows.Forms.CheckBox checkBoxCccNavAlmanach;
        private System.Windows.Forms.CheckBox checkBoxHipDriver;
        private System.Windows.Forms.Label labelCccNavSpeed;
        private System.Windows.Forms.TextBox textBoxCccNavSpeed;
        private System.Windows.Forms.TextBox textBoxMotorAmbientTemp;
        private System.Windows.Forms.Label labelMotorAmbientTemp;
        private System.Windows.Forms.TextBox textBoxMotorAmbientPress;
        private System.Windows.Forms.Label labelMotorAmbientPress;
        private System.Windows.Forms.TextBox textBoxMotorFuelTemp;
        private System.Windows.Forms.Label labelMotorFuelTemp;
        private System.Windows.Forms.TextBox textBoxMotorTempBeforeCat;
        private System.Windows.Forms.Label labelMotorTempBeforeCat;
        private System.Windows.Forms.TextBox textBoxMotorTempBeforeFilter;
        private System.Windows.Forms.Label labelMotorTempBeforeFilter;
        private System.Windows.Forms.TextBox textBoxMotorExhaustBackPressure;
        private System.Windows.Forms.Label labelMotorExhaustBackPressure;
        private System.Windows.Forms.TextBox textBoxMotorPartFilterDistSinceRegen;
        private System.Windows.Forms.Label labelMotorPartFilterDistSinceRegen;
        private System.Windows.Forms.CheckBox checkBoxMotorPartFilterStatus;
        private System.Windows.Forms.TextBox textBoxCccNavGpsDateTime;
        private System.Windows.Forms.Label labelCccNavGpsDateTime;
        private System.Windows.Forms.Label labelCccNavResPos;
        private System.Windows.Forms.TextBox textBoxCccNavResPos;
        private System.Windows.Forms.TabPage tabPageErrors;
        private System.Windows.Forms.Panel panelErrors;
        private System.Windows.Forms.TextBox textBoxErrors1;
        private System.Windows.Forms.TextBox textBoxErrors2;
        private System.Windows.Forms.CheckBox checkBoxMotorPartFilterUnblock;
        private System.Windows.Forms.TabPage tabPageIhk;
        private System.Windows.Forms.Panel panelIhk;
        private System.Windows.Forms.TextBox textBoxIhkInTemp;
        private System.Windows.Forms.Label labelIhkInTemp;
        private System.Windows.Forms.TextBox textBoxIhkInTempDelay;
        private System.Windows.Forms.Label labelIhkInTempDelay;
        private System.Windows.Forms.TextBox textBoxIhkOutTemp;
        private System.Windows.Forms.Label labelIhkOutTemp;
        private System.Windows.Forms.TextBox textBoxIhkSetpoint;
        private System.Windows.Forms.Label labelIhkSetpoint;
        private System.Windows.Forms.TextBox textBoxIhkHeatExSetpoint;
        private System.Windows.Forms.Label labelIhkHeatExSetpoint;
        private System.Windows.Forms.TextBox textBoxIhkHeatExTemp;
        private System.Windows.Forms.Label labelIhkHeatExTemp;
        private System.Windows.Forms.TabPage tabPageTest;
        private System.Windows.Forms.Panel panelTest;
        private System.Windows.Forms.TextBox textBoxTest;
        private System.Windows.Forms.TextBox textBoxCccNavResHorz;
        private System.Windows.Forms.Label labelCccNavResHorz;
        private System.Windows.Forms.Label labelCccNavResVert;
        private System.Windows.Forms.TextBox textBoxCccNavResVert;
        private System.Windows.Forms.TabPage tabPageAdapterConfig;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonAdapterConfigCan500;
        private System.Windows.Forms.TextBox textBoxAdapterConfigResult;
        private System.Windows.Forms.Label labelAdapterConfigResult;
        private System.Windows.Forms.Button buttonAdapterConfigCanOff;
        private System.Windows.Forms.Button buttonAdapterConfigCan100;
    }
}


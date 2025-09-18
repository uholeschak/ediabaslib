namespace CarSimulator
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
            components = new System.ComponentModel.Container();
            buttonConnect = new System.Windows.Forms.Button();
            listPorts = new System.Windows.Forms.ListBox();
            timerUpdate = new System.Windows.Forms.Timer(components);
            checkBoxMoving = new System.Windows.Forms.CheckBox();
            checkBoxVariableValues = new System.Windows.Forms.CheckBox();
            radioButtonBmwFast = new System.Windows.Forms.RadioButton();
            groupBoxConcepts = new System.Windows.Forms.GroupBox();
            radioButtonTp20 = new System.Windows.Forms.RadioButton();
            radioButtonKwp2000 = new System.Windows.Forms.RadioButton();
            radioButtonKwp2000Bmw = new System.Windows.Forms.RadioButton();
            radioButtonConcept3 = new System.Windows.Forms.RadioButton();
            radioButtonConcept1 = new System.Windows.Forms.RadioButton();
            radioButtonKwp1281 = new System.Windows.Forms.RadioButton();
            radioButtonDs2 = new System.Windows.Forms.RadioButton();
            radioButtonKwp2000S = new System.Windows.Forms.RadioButton();
            listBoxResponseFiles = new System.Windows.Forms.ListBox();
            checkBoxIgnitionOk = new System.Windows.Forms.CheckBox();
            checkBoxAdsAdapter = new System.Windows.Forms.CheckBox();
            checkBoxKLineResponder = new System.Windows.Forms.CheckBox();
            buttonErrorDefault = new System.Windows.Forms.Button();
            treeViewDirectories = new System.Windows.Forms.TreeView();
            folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            buttonRootFolder = new System.Windows.Forms.Button();
            buttonDeviceTestBtEdr = new System.Windows.Forms.Button();
            buttonDeviceTestWifi = new System.Windows.Forms.Button();
            checkBoxBtNameStd = new System.Windows.Forms.CheckBox();
            buttonAbortTest = new System.Windows.Forms.Button();
            buttonEcuFolder = new System.Windows.Forms.Button();
            textBoxEcuFolder = new System.Windows.Forms.TextBox();
            checkBoxEnetDoIp = new System.Windows.Forms.CheckBox();
            checkBoxHighTestVoltage = new System.Windows.Forms.CheckBox();
            buttonServerCert = new System.Windows.Forms.Button();
            textBoxServerCert = new System.Windows.Forms.TextBox();
            openCertFileDialog = new System.Windows.Forms.OpenFileDialog();
            textBoxCertPwd = new System.Windows.Forms.TextBox();
            labelCertPwd = new System.Windows.Forms.Label();
            labelSslPort = new System.Windows.Forms.Label();
            textBoxSslPort = new System.Windows.Forms.TextBox();
            richTextBoxTestResults = new System.Windows.Forms.RichTextBox();
            checkBoxBcSsl = new System.Windows.Forms.CheckBox();
            buttonDeviceTestBtBle = new System.Windows.Forms.Button();
            groupBoxConcepts.SuspendLayout();
            SuspendLayout();
            // 
            // buttonConnect
            // 
            buttonConnect.Location = new System.Drawing.Point(111, 12);
            buttonConnect.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonConnect.Name = "buttonConnect";
            buttonConnect.Size = new System.Drawing.Size(88, 27);
            buttonConnect.TabIndex = 0;
            buttonConnect.Text = "Connect";
            buttonConnect.UseVisualStyleBackColor = true;
            buttonConnect.Click += buttonConnect_Click;
            // 
            // listPorts
            // 
            listPorts.FormattingEnabled = true;
            listPorts.ItemHeight = 15;
            listPorts.Location = new System.Drawing.Point(13, 12);
            listPorts.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            listPorts.Name = "listPorts";
            listPorts.Size = new System.Drawing.Size(90, 109);
            listPorts.TabIndex = 5;
            // 
            // timerUpdate
            // 
            timerUpdate.Interval = 500;
            timerUpdate.Tick += timerUpdate_Tick;
            // 
            // checkBoxMoving
            // 
            checkBoxMoving.AutoSize = true;
            checkBoxMoving.Location = new System.Drawing.Point(111, 77);
            checkBoxMoving.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBoxMoving.Name = "checkBoxMoving";
            checkBoxMoving.Size = new System.Drawing.Size(64, 19);
            checkBoxMoving.TabIndex = 6;
            checkBoxMoving.Text = "Driving";
            checkBoxMoving.UseVisualStyleBackColor = true;
            checkBoxMoving.CheckedChanged += checkBoxMoving_CheckedChanged;
            // 
            // checkBoxVariableValues
            // 
            checkBoxVariableValues.AutoSize = true;
            checkBoxVariableValues.Location = new System.Drawing.Point(111, 102);
            checkBoxVariableValues.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBoxVariableValues.Name = "checkBoxVariableValues";
            checkBoxVariableValues.Size = new System.Drawing.Size(103, 19);
            checkBoxVariableValues.TabIndex = 7;
            checkBoxVariableValues.Text = "Variable values";
            checkBoxVariableValues.UseVisualStyleBackColor = true;
            checkBoxVariableValues.CheckedChanged += checkBoxVariableValues_CheckedChanged;
            // 
            // radioButtonBmwFast
            // 
            radioButtonBmwFast.AutoSize = true;
            radioButtonBmwFast.Checked = true;
            radioButtonBmwFast.Location = new System.Drawing.Point(7, 22);
            radioButtonBmwFast.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButtonBmwFast.Name = "radioButtonBmwFast";
            radioButtonBmwFast.Size = new System.Drawing.Size(78, 19);
            radioButtonBmwFast.TabIndex = 20;
            radioButtonBmwFast.TabStop = true;
            radioButtonBmwFast.Text = "BMW Fast";
            radioButtonBmwFast.UseVisualStyleBackColor = true;
            // 
            // groupBoxConcepts
            // 
            groupBoxConcepts.Controls.Add(radioButtonTp20);
            groupBoxConcepts.Controls.Add(radioButtonKwp2000);
            groupBoxConcepts.Controls.Add(radioButtonKwp2000Bmw);
            groupBoxConcepts.Controls.Add(radioButtonConcept3);
            groupBoxConcepts.Controls.Add(radioButtonConcept1);
            groupBoxConcepts.Controls.Add(radioButtonKwp1281);
            groupBoxConcepts.Controls.Add(radioButtonDs2);
            groupBoxConcepts.Controls.Add(radioButtonKwp2000S);
            groupBoxConcepts.Controls.Add(radioButtonBmwFast);
            groupBoxConcepts.Location = new System.Drawing.Point(511, 274);
            groupBoxConcepts.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBoxConcepts.Name = "groupBoxConcepts";
            groupBoxConcepts.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBoxConcepts.Size = new System.Drawing.Size(240, 272);
            groupBoxConcepts.TabIndex = 17;
            groupBoxConcepts.TabStop = false;
            groupBoxConcepts.Text = "Concepts";
            // 
            // radioButtonTp20
            // 
            radioButtonTp20.AutoSize = true;
            radioButtonTp20.Location = new System.Drawing.Point(7, 234);
            radioButtonTp20.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButtonTp20.Name = "radioButtonTp20";
            radioButtonTp20.Size = new System.Drawing.Size(93, 19);
            radioButtonTp20.TabIndex = 28;
            radioButtonTp20.TabStop = true;
            radioButtonTp20.Text = "TP 2.0 (CAN)";
            radioButtonTp20.UseVisualStyleBackColor = true;
            // 
            // radioButtonKwp2000
            // 
            radioButtonKwp2000.AutoSize = true;
            radioButtonKwp2000.Location = new System.Drawing.Point(7, 208);
            radioButtonKwp2000.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButtonKwp2000.Name = "radioButtonKwp2000";
            radioButtonKwp2000.Size = new System.Drawing.Size(132, 19);
            radioButtonKwp2000.TabIndex = 27;
            radioButtonKwp2000.TabStop = true;
            radioButtonKwp2000.Text = "KWP2000 (Standard)";
            radioButtonKwp2000.UseVisualStyleBackColor = true;
            // 
            // radioButtonKwp2000Bmw
            // 
            radioButtonKwp2000Bmw.AutoSize = true;
            radioButtonKwp2000Bmw.Location = new System.Drawing.Point(7, 48);
            radioButtonKwp2000Bmw.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButtonKwp2000Bmw.Name = "radioButtonKwp2000Bmw";
            radioButtonKwp2000Bmw.Size = new System.Drawing.Size(106, 19);
            radioButtonKwp2000Bmw.TabIndex = 21;
            radioButtonKwp2000Bmw.TabStop = true;
            radioButtonKwp2000Bmw.Text = "KWP2000 BMW";
            radioButtonKwp2000Bmw.UseVisualStyleBackColor = true;
            // 
            // radioButtonConcept3
            // 
            radioButtonConcept3.AutoSize = true;
            radioButtonConcept3.Location = new System.Drawing.Point(7, 181);
            radioButtonConcept3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButtonConcept3.Name = "radioButtonConcept3";
            radioButtonConcept3.Size = new System.Drawing.Size(79, 19);
            radioButtonConcept3.TabIndex = 26;
            radioButtonConcept3.TabStop = true;
            radioButtonConcept3.Text = "Concept 3";
            radioButtonConcept3.UseVisualStyleBackColor = true;
            // 
            // radioButtonConcept1
            // 
            radioButtonConcept1.AutoSize = true;
            radioButtonConcept1.Location = new System.Drawing.Point(7, 128);
            radioButtonConcept1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButtonConcept1.Name = "radioButtonConcept1";
            radioButtonConcept1.Size = new System.Drawing.Size(79, 19);
            radioButtonConcept1.TabIndex = 24;
            radioButtonConcept1.TabStop = true;
            radioButtonConcept1.Text = "Concept 1";
            radioButtonConcept1.UseVisualStyleBackColor = true;
            // 
            // radioButtonKwp1281
            // 
            radioButtonKwp1281.AutoSize = true;
            radioButtonKwp1281.Location = new System.Drawing.Point(7, 155);
            radioButtonKwp1281.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButtonKwp1281.Name = "radioButtonKwp1281";
            radioButtonKwp1281.Size = new System.Drawing.Size(130, 19);
            radioButtonKwp1281.TabIndex = 25;
            radioButtonKwp1281.TabStop = true;
            radioButtonKwp1281.Text = "KWP1281 (ISO 9141)";
            radioButtonKwp1281.UseVisualStyleBackColor = true;
            // 
            // radioButtonDs2
            // 
            radioButtonDs2.AutoSize = true;
            radioButtonDs2.Location = new System.Drawing.Point(7, 102);
            radioButtonDs2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButtonDs2.Name = "radioButtonDs2";
            radioButtonDs2.Size = new System.Drawing.Size(45, 19);
            radioButtonDs2.TabIndex = 23;
            radioButtonDs2.TabStop = true;
            radioButtonDs2.Text = "DS2";
            radioButtonDs2.UseVisualStyleBackColor = true;
            // 
            // radioButtonKwp2000S
            // 
            radioButtonKwp2000S.AutoSize = true;
            radioButtonKwp2000S.Location = new System.Drawing.Point(7, 75);
            radioButtonKwp2000S.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButtonKwp2000S.Name = "radioButtonKwp2000S";
            radioButtonKwp2000S.Size = new System.Drawing.Size(79, 19);
            radioButtonKwp2000S.TabIndex = 22;
            radioButtonKwp2000S.TabStop = true;
            radioButtonKwp2000S.Text = "KWP2000*";
            radioButtonKwp2000S.UseVisualStyleBackColor = true;
            // 
            // listBoxResponseFiles
            // 
            listBoxResponseFiles.FormattingEnabled = true;
            listBoxResponseFiles.ItemHeight = 15;
            listBoxResponseFiles.Location = new System.Drawing.Point(263, 254);
            listBoxResponseFiles.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            listBoxResponseFiles.Name = "listBoxResponseFiles";
            listBoxResponseFiles.Size = new System.Drawing.Size(241, 349);
            listBoxResponseFiles.Sorted = true;
            listBoxResponseFiles.TabIndex = 15;
            // 
            // checkBoxIgnitionOk
            // 
            checkBoxIgnitionOk.AutoSize = true;
            checkBoxIgnitionOk.Location = new System.Drawing.Point(111, 52);
            checkBoxIgnitionOk.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBoxIgnitionOk.Name = "checkBoxIgnitionOk";
            checkBoxIgnitionOk.Size = new System.Drawing.Size(86, 19);
            checkBoxIgnitionOk.TabIndex = 8;
            checkBoxIgnitionOk.Text = "Ignition OK";
            checkBoxIgnitionOk.UseVisualStyleBackColor = true;
            checkBoxIgnitionOk.CheckedChanged += checkBoxIgnitionOk_CheckedChanged;
            // 
            // checkBoxAdsAdapter
            // 
            checkBoxAdsAdapter.AutoSize = true;
            checkBoxAdsAdapter.Location = new System.Drawing.Point(262, 63);
            checkBoxAdsAdapter.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBoxAdsAdapter.Name = "checkBoxAdsAdapter";
            checkBoxAdsAdapter.Size = new System.Drawing.Size(91, 19);
            checkBoxAdsAdapter.TabIndex = 9;
            checkBoxAdsAdapter.Text = "ADS adapter";
            checkBoxAdsAdapter.UseVisualStyleBackColor = true;
            // 
            // checkBoxKLineResponder
            // 
            checkBoxKLineResponder.AutoSize = true;
            checkBoxKLineResponder.Location = new System.Drawing.Point(262, 88);
            checkBoxKLineResponder.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBoxKLineResponder.Name = "checkBoxKLineResponder";
            checkBoxKLineResponder.Size = new System.Drawing.Size(91, 19);
            checkBoxKLineResponder.TabIndex = 10;
            checkBoxKLineResponder.Text = "K-Line Resp.";
            checkBoxKLineResponder.UseVisualStyleBackColor = true;
            // 
            // buttonErrorDefault
            // 
            buttonErrorDefault.Location = new System.Drawing.Point(262, 12);
            buttonErrorDefault.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonErrorDefault.Name = "buttonErrorDefault";
            buttonErrorDefault.Size = new System.Drawing.Size(88, 27);
            buttonErrorDefault.TabIndex = 1;
            buttonErrorDefault.Text = "Error Default";
            buttonErrorDefault.UseVisualStyleBackColor = true;
            buttonErrorDefault.Click += buttonErrorReset_Click;
            // 
            // treeViewDirectories
            // 
            treeViewDirectories.Location = new System.Drawing.Point(14, 254);
            treeViewDirectories.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            treeViewDirectories.Name = "treeViewDirectories";
            treeViewDirectories.Size = new System.Drawing.Size(241, 349);
            treeViewDirectories.TabIndex = 14;
            treeViewDirectories.AfterSelect += treeViewDirectories_AfterSelect;
            // 
            // folderBrowserDialog
            // 
            folderBrowserDialog.Description = "Select response root folder";
            folderBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
            folderBrowserDialog.ShowNewFolderButton = false;
            // 
            // buttonRootFolder
            // 
            buttonRootFolder.Location = new System.Drawing.Point(14, 127);
            buttonRootFolder.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonRootFolder.Name = "buttonRootFolder";
            buttonRootFolder.Size = new System.Drawing.Size(240, 27);
            buttonRootFolder.TabIndex = 12;
            buttonRootFolder.Text = "Select Root Folder";
            buttonRootFolder.UseVisualStyleBackColor = true;
            buttonRootFolder.Click += buttonRootFolder_Click;
            // 
            // buttonDeviceTestBtEdr
            // 
            buttonDeviceTestBtEdr.Location = new System.Drawing.Point(392, 12);
            buttonDeviceTestBtEdr.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonDeviceTestBtEdr.Name = "buttonDeviceTestBtEdr";
            buttonDeviceTestBtEdr.Size = new System.Drawing.Size(111, 27);
            buttonDeviceTestBtEdr.TabIndex = 2;
            buttonDeviceTestBtEdr.Text = "Device Test EDR";
            buttonDeviceTestBtEdr.UseVisualStyleBackColor = true;
            buttonDeviceTestBtEdr.Click += buttonDeviceTest_Click;
            // 
            // buttonDeviceTestWifi
            // 
            buttonDeviceTestWifi.Location = new System.Drawing.Point(393, 78);
            buttonDeviceTestWifi.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonDeviceTestWifi.Name = "buttonDeviceTestWifi";
            buttonDeviceTestWifi.Size = new System.Drawing.Size(111, 27);
            buttonDeviceTestWifi.TabIndex = 3;
            buttonDeviceTestWifi.Text = "Device Test Wifi";
            buttonDeviceTestWifi.UseVisualStyleBackColor = true;
            buttonDeviceTestWifi.Click += buttonDeviceTest_Click;
            // 
            // checkBoxBtNameStd
            // 
            checkBoxBtNameStd.AutoSize = true;
            checkBoxBtNameStd.Location = new System.Drawing.Point(263, 38);
            checkBoxBtNameStd.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBoxBtNameStd.Name = "checkBoxBtNameStd";
            checkBoxBtNameStd.Size = new System.Drawing.Size(95, 19);
            checkBoxBtNameStd.TabIndex = 11;
            checkBoxBtNameStd.Text = "Bt Name Std.";
            checkBoxBtNameStd.UseVisualStyleBackColor = true;
            // 
            // buttonAbortTest
            // 
            buttonAbortTest.Location = new System.Drawing.Point(392, 111);
            buttonAbortTest.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonAbortTest.Name = "buttonAbortTest";
            buttonAbortTest.Size = new System.Drawing.Size(111, 27);
            buttonAbortTest.TabIndex = 4;
            buttonAbortTest.Text = "Abort Test";
            buttonAbortTest.UseVisualStyleBackColor = true;
            buttonAbortTest.Click += buttonAbortTest_Click;
            // 
            // buttonEcuFolder
            // 
            buttonEcuFolder.Location = new System.Drawing.Point(14, 160);
            buttonEcuFolder.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonEcuFolder.Name = "buttonEcuFolder";
            buttonEcuFolder.Size = new System.Drawing.Size(240, 27);
            buttonEcuFolder.TabIndex = 13;
            buttonEcuFolder.Text = "Select Ecu Folder";
            buttonEcuFolder.UseVisualStyleBackColor = true;
            buttonEcuFolder.Click += buttonEcuFolder_Click;
            // 
            // textBoxEcuFolder
            // 
            textBoxEcuFolder.Location = new System.Drawing.Point(262, 163);
            textBoxEcuFolder.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxEcuFolder.Name = "textBoxEcuFolder";
            textBoxEcuFolder.ReadOnly = true;
            textBoxEcuFolder.Size = new System.Drawing.Size(241, 23);
            textBoxEcuFolder.TabIndex = 18;
            // 
            // checkBoxEnetDoIp
            // 
            checkBoxEnetDoIp.AutoSize = true;
            checkBoxEnetDoIp.Checked = true;
            checkBoxEnetDoIp.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxEnetDoIp.Location = new System.Drawing.Point(262, 138);
            checkBoxEnetDoIp.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBoxEnetDoIp.Name = "checkBoxEnetDoIp";
            checkBoxEnetDoIp.Size = new System.Drawing.Size(51, 19);
            checkBoxEnetDoIp.TabIndex = 20;
            checkBoxEnetDoIp.Text = "DoIP";
            checkBoxEnetDoIp.UseVisualStyleBackColor = true;
            checkBoxEnetDoIp.CheckedChanged += checkBoxEnetDoIp_CheckedChanged;
            // 
            // checkBoxHighTestVoltage
            // 
            checkBoxHighTestVoltage.AutoSize = true;
            checkBoxHighTestVoltage.Location = new System.Drawing.Point(262, 113);
            checkBoxHighTestVoltage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBoxHighTestVoltage.Name = "checkBoxHighTestVoltage";
            checkBoxHighTestVoltage.Size = new System.Drawing.Size(116, 19);
            checkBoxHighTestVoltage.TabIndex = 21;
            checkBoxHighTestVoltage.Text = "High test voltage";
            checkBoxHighTestVoltage.UseVisualStyleBackColor = true;
            // 
            // buttonServerCert
            // 
            buttonServerCert.Location = new System.Drawing.Point(14, 193);
            buttonServerCert.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonServerCert.Name = "buttonServerCert";
            buttonServerCert.Size = new System.Drawing.Size(240, 27);
            buttonServerCert.TabIndex = 22;
            buttonServerCert.Text = "Select Server Cert";
            buttonServerCert.UseVisualStyleBackColor = true;
            buttonServerCert.Click += buttonServerCert_Click;
            // 
            // textBoxServerCert
            // 
            textBoxServerCert.Location = new System.Drawing.Point(262, 196);
            textBoxServerCert.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxServerCert.Name = "textBoxServerCert";
            textBoxServerCert.ReadOnly = true;
            textBoxServerCert.Size = new System.Drawing.Size(241, 23);
            textBoxServerCert.TabIndex = 23;
            // 
            // openCertFileDialog
            // 
            openCertFileDialog.DefaultExt = "key";
            openCertFileDialog.Filter = "Cert|*.key|All files|*.*";
            openCertFileDialog.Title = "Select server cert";
            // 
            // textBoxCertPwd
            // 
            textBoxCertPwd.Location = new System.Drawing.Point(77, 225);
            textBoxCertPwd.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxCertPwd.Name = "textBoxCertPwd";
            textBoxCertPwd.Size = new System.Drawing.Size(177, 23);
            textBoxCertPwd.TabIndex = 24;
            textBoxCertPwd.TextChanged += textBoxCertPwd_TextChanged;
            // 
            // labelCertPwd
            // 
            labelCertPwd.AutoSize = true;
            labelCertPwd.Location = new System.Drawing.Point(12, 228);
            labelCertPwd.Name = "labelCertPwd";
            labelCertPwd.Size = new System.Drawing.Size(58, 15);
            labelCertPwd.TabIndex = 25;
            labelCertPwd.Text = "Cert pwd:";
            // 
            // labelSslPort
            // 
            labelSslPort.AutoSize = true;
            labelSslPort.Location = new System.Drawing.Point(263, 228);
            labelSslPort.Name = "labelSslPort";
            labelSslPort.Size = new System.Drawing.Size(53, 15);
            labelSslPort.TabIndex = 26;
            labelSslPort.Text = "SSL port:";
            // 
            // textBoxSslPort
            // 
            textBoxSslPort.Location = new System.Drawing.Point(323, 225);
            textBoxSslPort.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxSslPort.Name = "textBoxSslPort";
            textBoxSslPort.Size = new System.Drawing.Size(55, 23);
            textBoxSslPort.TabIndex = 27;
            textBoxSslPort.TextChanged += textBoxSslPort_TextChanged;
            // 
            // richTextBoxTestResults
            // 
            richTextBoxTestResults.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            richTextBoxTestResults.Location = new System.Drawing.Point(510, 12);
            richTextBoxTestResults.Name = "richTextBoxTestResults";
            richTextBoxTestResults.ReadOnly = true;
            richTextBoxTestResults.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            richTextBoxTestResults.Size = new System.Drawing.Size(242, 256);
            richTextBoxTestResults.TabIndex = 28;
            richTextBoxTestResults.Text = "";
            richTextBoxTestResults.LinkClicked += richTextBoxTestResults_LinkClicked;
            // 
            // checkBoxBcSsl
            // 
            checkBoxBcSsl.AutoSize = true;
            checkBoxBcSsl.Checked = true;
            checkBoxBcSsl.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxBcSsl.Location = new System.Drawing.Point(392, 227);
            checkBoxBcSsl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBoxBcSsl.Name = "checkBoxBcSsl";
            checkBoxBcSsl.Size = new System.Drawing.Size(71, 19);
            checkBoxBcSsl.TabIndex = 29;
            checkBoxBcSsl.Text = "BcCastle";
            checkBoxBcSsl.UseVisualStyleBackColor = true;
            checkBoxBcSsl.CheckedChanged += checkBoxBcSsl_CheckedChanged;
            // 
            // buttonDeviceTestBtBle
            // 
            buttonDeviceTestBtBle.Location = new System.Drawing.Point(393, 45);
            buttonDeviceTestBtBle.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonDeviceTestBtBle.Name = "buttonDeviceTestBtBle";
            buttonDeviceTestBtBle.Size = new System.Drawing.Size(111, 27);
            buttonDeviceTestBtBle.TabIndex = 30;
            buttonDeviceTestBtBle.Text = "Device Test BLE";
            buttonDeviceTestBtBle.UseVisualStyleBackColor = true;
            buttonDeviceTestBtBle.Click += buttonDeviceTest_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(764, 618);
            Controls.Add(buttonDeviceTestBtBle);
            Controls.Add(checkBoxBcSsl);
            Controls.Add(richTextBoxTestResults);
            Controls.Add(textBoxSslPort);
            Controls.Add(labelSslPort);
            Controls.Add(labelCertPwd);
            Controls.Add(textBoxCertPwd);
            Controls.Add(textBoxServerCert);
            Controls.Add(buttonServerCert);
            Controls.Add(checkBoxHighTestVoltage);
            Controls.Add(checkBoxEnetDoIp);
            Controls.Add(textBoxEcuFolder);
            Controls.Add(buttonEcuFolder);
            Controls.Add(buttonAbortTest);
            Controls.Add(checkBoxBtNameStd);
            Controls.Add(buttonDeviceTestWifi);
            Controls.Add(buttonDeviceTestBtEdr);
            Controls.Add(buttonRootFolder);
            Controls.Add(treeViewDirectories);
            Controls.Add(buttonErrorDefault);
            Controls.Add(checkBoxKLineResponder);
            Controls.Add(checkBoxAdsAdapter);
            Controls.Add(checkBoxIgnitionOk);
            Controls.Add(listBoxResponseFiles);
            Controls.Add(groupBoxConcepts);
            Controls.Add(checkBoxVariableValues);
            Controls.Add(checkBoxMoving);
            Controls.Add(listPorts);
            Controls.Add(buttonConnect);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            Name = "MainForm";
            Text = "Car Simulator";
            FormClosing += MainForm_FormClosing;
            FormClosed += MainForm_FormClosed;
            Load += MainForm_Load;
            groupBoxConcepts.ResumeLayout(false);
            groupBoxConcepts.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.ListBox listPorts;
        private System.Windows.Forms.Timer timerUpdate;
        private System.Windows.Forms.CheckBox checkBoxMoving;
        private System.Windows.Forms.CheckBox checkBoxVariableValues;
        private System.Windows.Forms.RadioButton radioButtonBmwFast;
        private System.Windows.Forms.GroupBox groupBoxConcepts;
        private System.Windows.Forms.RadioButton radioButtonKwp2000S;
        private System.Windows.Forms.RadioButton radioButtonDs2;
        private System.Windows.Forms.ListBox listBoxResponseFiles;
        private System.Windows.Forms.CheckBox checkBoxIgnitionOk;
        private System.Windows.Forms.RadioButton radioButtonKwp1281;
        private System.Windows.Forms.CheckBox checkBoxAdsAdapter;
        private System.Windows.Forms.RadioButton radioButtonConcept1;
        private System.Windows.Forms.RadioButton radioButtonConcept3;
        private System.Windows.Forms.RadioButton radioButtonKwp2000Bmw;
        private System.Windows.Forms.CheckBox checkBoxKLineResponder;
        private System.Windows.Forms.Button buttonErrorDefault;
        private System.Windows.Forms.TreeView treeViewDirectories;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.Button buttonRootFolder;
        private System.Windows.Forms.RadioButton radioButtonKwp2000;
        private System.Windows.Forms.RadioButton radioButtonTp20;
        private System.Windows.Forms.Button buttonDeviceTestBtEdr;
        private System.Windows.Forms.Button buttonDeviceTestWifi;
        private System.Windows.Forms.CheckBox checkBoxBtNameStd;
        private System.Windows.Forms.Button buttonAbortTest;
        private System.Windows.Forms.Button buttonEcuFolder;
        private System.Windows.Forms.TextBox textBoxEcuFolder;
        private System.Windows.Forms.CheckBox checkBoxEnetDoIp;
        private System.Windows.Forms.CheckBox checkBoxHighTestVoltage;
        private System.Windows.Forms.Button buttonServerCert;
        private System.Windows.Forms.TextBox textBoxServerCert;
        private System.Windows.Forms.OpenFileDialog openCertFileDialog;
        private System.Windows.Forms.TextBox textBoxCertPwd;
        private System.Windows.Forms.Label labelCertPwd;
        private System.Windows.Forms.Label labelSslPort;
        private System.Windows.Forms.TextBox textBoxSslPort;
        private System.Windows.Forms.RichTextBox richTextBoxTestResults;
        private System.Windows.Forms.CheckBox checkBoxBcSsl;
        private System.Windows.Forms.Button buttonDeviceTestBtBle;
    }
}


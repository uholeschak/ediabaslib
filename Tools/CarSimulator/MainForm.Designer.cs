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
            this.components = new System.ComponentModel.Container();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.listPorts = new System.Windows.Forms.ListBox();
            this.timerUpdate = new System.Windows.Forms.Timer(this.components);
            this.checkBoxMoving = new System.Windows.Forms.CheckBox();
            this.checkBoxVariableValues = new System.Windows.Forms.CheckBox();
            this.radioButtonBmwFast = new System.Windows.Forms.RadioButton();
            this.groupBoxConcepts = new System.Windows.Forms.GroupBox();
            this.radioButtonTp20 = new System.Windows.Forms.RadioButton();
            this.radioButtonKwp2000 = new System.Windows.Forms.RadioButton();
            this.radioButtonKwp2000Bmw = new System.Windows.Forms.RadioButton();
            this.radioButtonConcept3 = new System.Windows.Forms.RadioButton();
            this.radioButtonConcept1 = new System.Windows.Forms.RadioButton();
            this.radioButtonKwp1281 = new System.Windows.Forms.RadioButton();
            this.radioButtonDs2 = new System.Windows.Forms.RadioButton();
            this.radioButtonKwp2000S = new System.Windows.Forms.RadioButton();
            this.listBoxResponseFiles = new System.Windows.Forms.ListBox();
            this.checkBoxIgnitionOk = new System.Windows.Forms.CheckBox();
            this.checkBoxAdsAdapter = new System.Windows.Forms.CheckBox();
            this.checkBoxKLineResponder = new System.Windows.Forms.CheckBox();
            this.buttonErrorDefault = new System.Windows.Forms.Button();
            this.treeViewDirectories = new System.Windows.Forms.TreeView();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.buttonRootFolder = new System.Windows.Forms.Button();
            this.buttonDeviceTestBt = new System.Windows.Forms.Button();
            this.textBoxTestResults = new System.Windows.Forms.TextBox();
            this.buttonDeviceTestWifi = new System.Windows.Forms.Button();
            this.groupBoxConcepts.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(96, 12);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(75, 23);
            this.buttonConnect.TabIndex = 0;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // listPorts
            // 
            this.listPorts.FormattingEnabled = true;
            this.listPorts.Location = new System.Drawing.Point(12, 12);
            this.listPorts.Name = "listPorts";
            this.listPorts.Size = new System.Drawing.Size(78, 69);
            this.listPorts.TabIndex = 1;
            // 
            // timerUpdate
            // 
            this.timerUpdate.Interval = 500;
            this.timerUpdate.Tick += new System.EventHandler(this.timerUpdate_Tick);
            // 
            // checkBoxMoving
            // 
            this.checkBoxMoving.AutoSize = true;
            this.checkBoxMoving.Location = new System.Drawing.Point(97, 42);
            this.checkBoxMoving.Name = "checkBoxMoving";
            this.checkBoxMoving.Size = new System.Drawing.Size(59, 17);
            this.checkBoxMoving.TabIndex = 2;
            this.checkBoxMoving.Text = "Driving";
            this.checkBoxMoving.UseVisualStyleBackColor = true;
            this.checkBoxMoving.CheckedChanged += new System.EventHandler(this.checkBoxMoving_CheckedChanged);
            // 
            // checkBoxVariableValues
            // 
            this.checkBoxVariableValues.AutoSize = true;
            this.checkBoxVariableValues.Location = new System.Drawing.Point(97, 65);
            this.checkBoxVariableValues.Name = "checkBoxVariableValues";
            this.checkBoxVariableValues.Size = new System.Drawing.Size(98, 17);
            this.checkBoxVariableValues.TabIndex = 3;
            this.checkBoxVariableValues.Text = "Variable values";
            this.checkBoxVariableValues.UseVisualStyleBackColor = true;
            // 
            // radioButtonBmwFast
            // 
            this.radioButtonBmwFast.AutoSize = true;
            this.radioButtonBmwFast.Checked = true;
            this.radioButtonBmwFast.Location = new System.Drawing.Point(6, 19);
            this.radioButtonBmwFast.Name = "radioButtonBmwFast";
            this.radioButtonBmwFast.Size = new System.Drawing.Size(75, 17);
            this.radioButtonBmwFast.TabIndex = 20;
            this.radioButtonBmwFast.TabStop = true;
            this.radioButtonBmwFast.Text = "BMW Fast";
            this.radioButtonBmwFast.UseVisualStyleBackColor = true;
            // 
            // groupBoxConcepts
            // 
            this.groupBoxConcepts.Controls.Add(this.radioButtonTp20);
            this.groupBoxConcepts.Controls.Add(this.radioButtonKwp2000);
            this.groupBoxConcepts.Controls.Add(this.radioButtonKwp2000Bmw);
            this.groupBoxConcepts.Controls.Add(this.radioButtonConcept3);
            this.groupBoxConcepts.Controls.Add(this.radioButtonConcept1);
            this.groupBoxConcepts.Controls.Add(this.radioButtonKwp1281);
            this.groupBoxConcepts.Controls.Add(this.radioButtonDs2);
            this.groupBoxConcepts.Controls.Add(this.radioButtonKwp2000S);
            this.groupBoxConcepts.Controls.Add(this.radioButtonBmwFast);
            this.groupBoxConcepts.Location = new System.Drawing.Point(438, 162);
            this.groupBoxConcepts.Name = "groupBoxConcepts";
            this.groupBoxConcepts.Size = new System.Drawing.Size(207, 232);
            this.groupBoxConcepts.TabIndex = 6;
            this.groupBoxConcepts.TabStop = false;
            this.groupBoxConcepts.Text = "Concepts";
            // 
            // radioButtonTp20
            // 
            this.radioButtonTp20.AutoSize = true;
            this.radioButtonTp20.Location = new System.Drawing.Point(6, 203);
            this.radioButtonTp20.Name = "radioButtonTp20";
            this.radioButtonTp20.Size = new System.Drawing.Size(88, 17);
            this.radioButtonTp20.TabIndex = 28;
            this.radioButtonTp20.TabStop = true;
            this.radioButtonTp20.Text = "TP 2.0 (CAN)";
            this.radioButtonTp20.UseVisualStyleBackColor = true;
            // 
            // radioButtonKwp2000
            // 
            this.radioButtonKwp2000.AutoSize = true;
            this.radioButtonKwp2000.Location = new System.Drawing.Point(6, 180);
            this.radioButtonKwp2000.Name = "radioButtonKwp2000";
            this.radioButtonKwp2000.Size = new System.Drawing.Size(126, 17);
            this.radioButtonKwp2000.TabIndex = 27;
            this.radioButtonKwp2000.TabStop = true;
            this.radioButtonKwp2000.Text = "KWP2000 (Standard)";
            this.radioButtonKwp2000.UseVisualStyleBackColor = true;
            // 
            // radioButtonKwp2000Bmw
            // 
            this.radioButtonKwp2000Bmw.AutoSize = true;
            this.radioButtonKwp2000Bmw.Location = new System.Drawing.Point(6, 42);
            this.radioButtonKwp2000Bmw.Name = "radioButtonKwp2000Bmw";
            this.radioButtonKwp2000Bmw.Size = new System.Drawing.Size(104, 17);
            this.radioButtonKwp2000Bmw.TabIndex = 21;
            this.radioButtonKwp2000Bmw.TabStop = true;
            this.radioButtonKwp2000Bmw.Text = "KWP2000 BMW";
            this.radioButtonKwp2000Bmw.UseVisualStyleBackColor = true;
            // 
            // radioButtonConcept3
            // 
            this.radioButtonConcept3.AutoSize = true;
            this.radioButtonConcept3.Location = new System.Drawing.Point(6, 157);
            this.radioButtonConcept3.Name = "radioButtonConcept3";
            this.radioButtonConcept3.Size = new System.Drawing.Size(74, 17);
            this.radioButtonConcept3.TabIndex = 26;
            this.radioButtonConcept3.TabStop = true;
            this.radioButtonConcept3.Text = "Concept 3";
            this.radioButtonConcept3.UseVisualStyleBackColor = true;
            // 
            // radioButtonConcept1
            // 
            this.radioButtonConcept1.AutoSize = true;
            this.radioButtonConcept1.Location = new System.Drawing.Point(6, 111);
            this.radioButtonConcept1.Name = "radioButtonConcept1";
            this.radioButtonConcept1.Size = new System.Drawing.Size(74, 17);
            this.radioButtonConcept1.TabIndex = 24;
            this.radioButtonConcept1.TabStop = true;
            this.radioButtonConcept1.Text = "Concept 1";
            this.radioButtonConcept1.UseVisualStyleBackColor = true;
            // 
            // radioButtonKwp1281
            // 
            this.radioButtonKwp1281.AutoSize = true;
            this.radioButtonKwp1281.Location = new System.Drawing.Point(6, 134);
            this.radioButtonKwp1281.Name = "radioButtonKwp1281";
            this.radioButtonKwp1281.Size = new System.Drawing.Size(128, 17);
            this.radioButtonKwp1281.TabIndex = 25;
            this.radioButtonKwp1281.TabStop = true;
            this.radioButtonKwp1281.Text = "KWP1281 (ISO 9141)";
            this.radioButtonKwp1281.UseVisualStyleBackColor = true;
            // 
            // radioButtonDs2
            // 
            this.radioButtonDs2.AutoSize = true;
            this.radioButtonDs2.Location = new System.Drawing.Point(6, 88);
            this.radioButtonDs2.Name = "radioButtonDs2";
            this.radioButtonDs2.Size = new System.Drawing.Size(46, 17);
            this.radioButtonDs2.TabIndex = 23;
            this.radioButtonDs2.TabStop = true;
            this.radioButtonDs2.Text = "DS2";
            this.radioButtonDs2.UseVisualStyleBackColor = true;
            // 
            // radioButtonKwp2000S
            // 
            this.radioButtonKwp2000S.AutoSize = true;
            this.radioButtonKwp2000S.Location = new System.Drawing.Point(6, 65);
            this.radioButtonKwp2000S.Name = "radioButtonKwp2000S";
            this.radioButtonKwp2000S.Size = new System.Drawing.Size(78, 17);
            this.radioButtonKwp2000S.TabIndex = 22;
            this.radioButtonKwp2000S.TabStop = true;
            this.radioButtonKwp2000S.Text = "KWP2000*";
            this.radioButtonKwp2000S.UseVisualStyleBackColor = true;
            // 
            // listBoxResponseFiles
            // 
            this.listBoxResponseFiles.FormattingEnabled = true;
            this.listBoxResponseFiles.Location = new System.Drawing.Point(224, 117);
            this.listBoxResponseFiles.Name = "listBoxResponseFiles";
            this.listBoxResponseFiles.Size = new System.Drawing.Size(207, 277);
            this.listBoxResponseFiles.Sorted = true;
            this.listBoxResponseFiles.TabIndex = 10;
            // 
            // checkBoxIgnitionOk
            // 
            this.checkBoxIgnitionOk.AutoSize = true;
            this.checkBoxIgnitionOk.Checked = true;
            this.checkBoxIgnitionOk.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxIgnitionOk.Location = new System.Drawing.Point(224, 71);
            this.checkBoxIgnitionOk.Name = "checkBoxIgnitionOk";
            this.checkBoxIgnitionOk.Size = new System.Drawing.Size(78, 17);
            this.checkBoxIgnitionOk.TabIndex = 4;
            this.checkBoxIgnitionOk.Text = "Ignition OK";
            this.checkBoxIgnitionOk.UseVisualStyleBackColor = true;
            this.checkBoxIgnitionOk.CheckedChanged += new System.EventHandler(this.checkBoxIgnitionOk_CheckedChanged);
            // 
            // checkBoxAdsAdapter
            // 
            this.checkBoxAdsAdapter.AutoSize = true;
            this.checkBoxAdsAdapter.Location = new System.Drawing.Point(224, 94);
            this.checkBoxAdsAdapter.Name = "checkBoxAdsAdapter";
            this.checkBoxAdsAdapter.Size = new System.Drawing.Size(87, 17);
            this.checkBoxAdsAdapter.TabIndex = 5;
            this.checkBoxAdsAdapter.Text = "ADS adapter";
            this.checkBoxAdsAdapter.UseVisualStyleBackColor = true;
            // 
            // checkBoxKLineResponder
            // 
            this.checkBoxKLineResponder.AutoSize = true;
            this.checkBoxKLineResponder.Checked = true;
            this.checkBoxKLineResponder.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxKLineResponder.Location = new System.Drawing.Point(336, 94);
            this.checkBoxKLineResponder.Name = "checkBoxKLineResponder";
            this.checkBoxKLineResponder.Size = new System.Drawing.Size(87, 17);
            this.checkBoxKLineResponder.TabIndex = 6;
            this.checkBoxKLineResponder.Text = "K-Line Resp.";
            this.checkBoxKLineResponder.UseVisualStyleBackColor = true;
            // 
            // buttonErrorDefault
            // 
            this.buttonErrorDefault.Location = new System.Drawing.Point(224, 12);
            this.buttonErrorDefault.Name = "buttonErrorDefault";
            this.buttonErrorDefault.Size = new System.Drawing.Size(75, 23);
            this.buttonErrorDefault.TabIndex = 1;
            this.buttonErrorDefault.Text = "Error Default";
            this.buttonErrorDefault.UseVisualStyleBackColor = true;
            this.buttonErrorDefault.Click += new System.EventHandler(this.buttonErrorReset_Click);
            // 
            // treeViewDirectories
            // 
            this.treeViewDirectories.Location = new System.Drawing.Point(12, 117);
            this.treeViewDirectories.Name = "treeViewDirectories";
            this.treeViewDirectories.Size = new System.Drawing.Size(206, 277);
            this.treeViewDirectories.TabIndex = 9;
            this.treeViewDirectories.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewDirectories_AfterSelect);
            // 
            // folderBrowserDialog
            // 
            this.folderBrowserDialog.Description = "Select response root folder";
            this.folderBrowserDialog.ShowNewFolderButton = false;
            // 
            // buttonRootFolder
            // 
            this.buttonRootFolder.Location = new System.Drawing.Point(12, 88);
            this.buttonRootFolder.Name = "buttonRootFolder";
            this.buttonRootFolder.Size = new System.Drawing.Size(206, 23);
            this.buttonRootFolder.TabIndex = 8;
            this.buttonRootFolder.Text = "Select Root Folder";
            this.buttonRootFolder.UseVisualStyleBackColor = true;
            this.buttonRootFolder.Click += new System.EventHandler(this.buttonRootFolder_Click);
            // 
            // buttonDeviceTestBt
            // 
            this.buttonDeviceTestBt.Location = new System.Drawing.Point(336, 12);
            this.buttonDeviceTestBt.Name = "buttonDeviceTestBt";
            this.buttonDeviceTestBt.Size = new System.Drawing.Size(95, 23);
            this.buttonDeviceTestBt.TabIndex = 11;
            this.buttonDeviceTestBt.Text = "Device Test Bt";
            this.buttonDeviceTestBt.UseVisualStyleBackColor = true;
            this.buttonDeviceTestBt.Click += new System.EventHandler(this.buttonDeviceTest_Click);
            // 
            // textBoxTestResults
            // 
            this.textBoxTestResults.Location = new System.Drawing.Point(437, 14);
            this.textBoxTestResults.Multiline = true;
            this.textBoxTestResults.Name = "textBoxTestResults";
            this.textBoxTestResults.ReadOnly = true;
            this.textBoxTestResults.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxTestResults.Size = new System.Drawing.Size(207, 142);
            this.textBoxTestResults.TabIndex = 13;
            // 
            // buttonDeviceTestWifi
            // 
            this.buttonDeviceTestWifi.Location = new System.Drawing.Point(336, 41);
            this.buttonDeviceTestWifi.Name = "buttonDeviceTestWifi";
            this.buttonDeviceTestWifi.Size = new System.Drawing.Size(95, 23);
            this.buttonDeviceTestWifi.TabIndex = 14;
            this.buttonDeviceTestWifi.Text = "Device Test Wifi";
            this.buttonDeviceTestWifi.UseVisualStyleBackColor = true;
            this.buttonDeviceTestWifi.Click += new System.EventHandler(this.buttonDeviceTest_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(657, 408);
            this.Controls.Add(this.buttonDeviceTestWifi);
            this.Controls.Add(this.textBoxTestResults);
            this.Controls.Add(this.buttonDeviceTestBt);
            this.Controls.Add(this.buttonRootFolder);
            this.Controls.Add(this.treeViewDirectories);
            this.Controls.Add(this.buttonErrorDefault);
            this.Controls.Add(this.checkBoxKLineResponder);
            this.Controls.Add(this.checkBoxAdsAdapter);
            this.Controls.Add(this.checkBoxIgnitionOk);
            this.Controls.Add(this.listBoxResponseFiles);
            this.Controls.Add(this.groupBoxConcepts);
            this.Controls.Add(this.checkBoxVariableValues);
            this.Controls.Add(this.checkBoxMoving);
            this.Controls.Add(this.listPorts);
            this.Controls.Add(this.buttonConnect);
            this.Name = "MainForm";
            this.Text = "Car Simulator";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.groupBoxConcepts.ResumeLayout(false);
            this.groupBoxConcepts.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.Button buttonDeviceTestBt;
        private System.Windows.Forms.TextBox textBoxTestResults;
        private System.Windows.Forms.Button buttonDeviceTestWifi;
    }
}


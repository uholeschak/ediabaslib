namespace EdiabasLibConfigTool
{
    partial class FormMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            listViewDevices = new System.Windows.Forms.ListView();
            columnHeaderAddress = new System.Windows.Forms.ColumnHeader();
            columnHeaderName = new System.Windows.Forms.ColumnHeader();
            buttonSearch = new System.Windows.Forms.Button();
            buttonClose = new System.Windows.Forms.Button();
            labelStatus = new System.Windows.Forms.Label();
            buttonTest = new System.Windows.Forms.Button();
            textBoxBluetoothPin = new System.Windows.Forms.TextBox();
            labelBluetoothPin = new System.Windows.Forms.Label();
            labelBtDevices = new System.Windows.Forms.Label();
            buttonPatchEdiabas = new System.Windows.Forms.Button();
            openFileDialogConfigFile = new System.Windows.Forms.OpenFileDialog();
            textBoxWifiPassword = new System.Windows.Forms.TextBox();
            labelWiFiPassword = new System.Windows.Forms.Label();
            buttonRestoreEdiabas = new System.Windows.Forms.Button();
            groupBoxEdiabas = new System.Windows.Forms.GroupBox();
            groupBoxVasPc = new System.Windows.Forms.GroupBox();
            buttonPatchVasPc = new System.Windows.Forms.Button();
            buttonRestoreVasPc = new System.Windows.Forms.Button();
            groupBoxIstad = new System.Windows.Forms.GroupBox();
            textBoxIstaLocation = new System.Windows.Forms.TextBox();
            buttonDirIstad = new System.Windows.Forms.Button();
            buttonPatchIstad = new System.Windows.Forms.Button();
            buttonRestoreIstad = new System.Windows.Forms.Button();
            comboBoxLanguage = new System.Windows.Forms.ComboBox();
            labelLanguage = new System.Windows.Forms.Label();
            richTextBoxStatus = new System.Windows.Forms.RichTextBox();
            checkBoxEnableBle = new System.Windows.Forms.CheckBox();
            groupBoxEdiabas.SuspendLayout();
            groupBoxVasPc.SuspendLayout();
            groupBoxIstad.SuspendLayout();
            SuspendLayout();
            // 
            // listViewDevices
            // 
            resources.ApplyResources(listViewDevices, "listViewDevices");
            listViewDevices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderAddress, columnHeaderName });
            listViewDevices.FullRowSelect = true;
            listViewDevices.GridLines = true;
            listViewDevices.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            listViewDevices.MultiSelect = false;
            listViewDevices.Name = "listViewDevices";
            listViewDevices.ShowGroups = false;
            listViewDevices.UseCompatibleStateImageBehavior = false;
            listViewDevices.View = System.Windows.Forms.View.Details;
            listViewDevices.ColumnWidthChanging += listViewDevices_ColumnWidthChanging;
            listViewDevices.SelectedIndexChanged += listViewDevices_SelectedIndexChanged;
            listViewDevices.DoubleClick += listViewDevices_DoubleClick;
            // 
            // columnHeaderAddress
            // 
            resources.ApplyResources(columnHeaderAddress, "columnHeaderAddress");
            // 
            // columnHeaderName
            // 
            resources.ApplyResources(columnHeaderName, "columnHeaderName");
            // 
            // buttonSearch
            // 
            resources.ApplyResources(buttonSearch, "buttonSearch");
            buttonSearch.Name = "buttonSearch";
            buttonSearch.UseVisualStyleBackColor = true;
            buttonSearch.Click += buttonSearch_Click;
            // 
            // buttonClose
            // 
            resources.ApplyResources(buttonClose, "buttonClose");
            buttonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonClose.Name = "buttonClose";
            buttonClose.UseVisualStyleBackColor = true;
            buttonClose.Click += buttonClose_Click;
            // 
            // labelStatus
            // 
            resources.ApplyResources(labelStatus, "labelStatus");
            labelStatus.Name = "labelStatus";
            // 
            // buttonTest
            // 
            resources.ApplyResources(buttonTest, "buttonTest");
            buttonTest.Name = "buttonTest";
            buttonTest.UseVisualStyleBackColor = true;
            buttonTest.Click += buttonTest_Click;
            // 
            // textBoxBluetoothPin
            // 
            resources.ApplyResources(textBoxBluetoothPin, "textBoxBluetoothPin");
            textBoxBluetoothPin.Name = "textBoxBluetoothPin";
            textBoxBluetoothPin.KeyPress += textBoxBluetoothPin_KeyPress;
            // 
            // labelBluetoothPin
            // 
            resources.ApplyResources(labelBluetoothPin, "labelBluetoothPin");
            labelBluetoothPin.Name = "labelBluetoothPin";
            // 
            // labelBtDevices
            // 
            resources.ApplyResources(labelBtDevices, "labelBtDevices");
            labelBtDevices.Name = "labelBtDevices";
            // 
            // buttonPatchEdiabas
            // 
            resources.ApplyResources(buttonPatchEdiabas, "buttonPatchEdiabas");
            buttonPatchEdiabas.Name = "buttonPatchEdiabas";
            buttonPatchEdiabas.UseVisualStyleBackColor = true;
            buttonPatchEdiabas.Click += buttonPatch_Click;
            // 
            // openFileDialogConfigFile
            // 
            resources.ApplyResources(openFileDialogConfigFile, "openFileDialogConfigFile");
            // 
            // textBoxWifiPassword
            // 
            resources.ApplyResources(textBoxWifiPassword, "textBoxWifiPassword");
            textBoxWifiPassword.Name = "textBoxWifiPassword";
            // 
            // labelWiFiPassword
            // 
            resources.ApplyResources(labelWiFiPassword, "labelWiFiPassword");
            labelWiFiPassword.Name = "labelWiFiPassword";
            // 
            // buttonRestoreEdiabas
            // 
            resources.ApplyResources(buttonRestoreEdiabas, "buttonRestoreEdiabas");
            buttonRestoreEdiabas.Name = "buttonRestoreEdiabas";
            buttonRestoreEdiabas.UseVisualStyleBackColor = true;
            buttonRestoreEdiabas.Click += buttonRestore_Click;
            // 
            // groupBoxEdiabas
            // 
            resources.ApplyResources(groupBoxEdiabas, "groupBoxEdiabas");
            groupBoxEdiabas.Controls.Add(buttonPatchEdiabas);
            groupBoxEdiabas.Controls.Add(buttonRestoreEdiabas);
            groupBoxEdiabas.Name = "groupBoxEdiabas";
            groupBoxEdiabas.TabStop = false;
            // 
            // groupBoxVasPc
            // 
            resources.ApplyResources(groupBoxVasPc, "groupBoxVasPc");
            groupBoxVasPc.Controls.Add(buttonPatchVasPc);
            groupBoxVasPc.Controls.Add(buttonRestoreVasPc);
            groupBoxVasPc.Name = "groupBoxVasPc";
            groupBoxVasPc.TabStop = false;
            // 
            // buttonPatchVasPc
            // 
            resources.ApplyResources(buttonPatchVasPc, "buttonPatchVasPc");
            buttonPatchVasPc.Name = "buttonPatchVasPc";
            buttonPatchVasPc.UseVisualStyleBackColor = true;
            buttonPatchVasPc.Click += buttonPatch_Click;
            // 
            // buttonRestoreVasPc
            // 
            resources.ApplyResources(buttonRestoreVasPc, "buttonRestoreVasPc");
            buttonRestoreVasPc.Name = "buttonRestoreVasPc";
            buttonRestoreVasPc.UseVisualStyleBackColor = true;
            buttonRestoreVasPc.Click += buttonRestore_Click;
            // 
            // groupBoxIstad
            // 
            resources.ApplyResources(groupBoxIstad, "groupBoxIstad");
            groupBoxIstad.Controls.Add(textBoxIstaLocation);
            groupBoxIstad.Controls.Add(buttonDirIstad);
            groupBoxIstad.Controls.Add(buttonPatchIstad);
            groupBoxIstad.Controls.Add(buttonRestoreIstad);
            groupBoxIstad.Name = "groupBoxIstad";
            groupBoxIstad.TabStop = false;
            // 
            // textBoxIstaLocation
            // 
            resources.ApplyResources(textBoxIstaLocation, "textBoxIstaLocation");
            textBoxIstaLocation.Name = "textBoxIstaLocation";
            textBoxIstaLocation.ReadOnly = true;
            textBoxIstaLocation.TabStop = false;
            // 
            // buttonDirIstad
            // 
            resources.ApplyResources(buttonDirIstad, "buttonDirIstad");
            buttonDirIstad.Name = "buttonDirIstad";
            buttonDirIstad.UseVisualStyleBackColor = true;
            buttonDirIstad.Click += buttonDirIstad_Click;
            // 
            // buttonPatchIstad
            // 
            resources.ApplyResources(buttonPatchIstad, "buttonPatchIstad");
            buttonPatchIstad.Name = "buttonPatchIstad";
            buttonPatchIstad.UseVisualStyleBackColor = true;
            buttonPatchIstad.Click += buttonPatch_Click;
            // 
            // buttonRestoreIstad
            // 
            resources.ApplyResources(buttonRestoreIstad, "buttonRestoreIstad");
            buttonRestoreIstad.Name = "buttonRestoreIstad";
            buttonRestoreIstad.UseVisualStyleBackColor = true;
            buttonRestoreIstad.Click += buttonRestore_Click;
            // 
            // comboBoxLanguage
            // 
            resources.ApplyResources(comboBoxLanguage, "comboBoxLanguage");
            comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxLanguage.FormattingEnabled = true;
            comboBoxLanguage.Name = "comboBoxLanguage";
            comboBoxLanguage.SelectedIndexChanged += comboBoxLanguage_SelectedIndexChanged;
            // 
            // labelLanguage
            // 
            resources.ApplyResources(labelLanguage, "labelLanguage");
            labelLanguage.Name = "labelLanguage";
            // 
            // richTextBoxStatus
            // 
            resources.ApplyResources(richTextBoxStatus, "richTextBoxStatus");
            richTextBoxStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            richTextBoxStatus.Name = "richTextBoxStatus";
            richTextBoxStatus.ReadOnly = true;
            richTextBoxStatus.LinkClicked += richTextBoxStatus_LinkClicked;
            // 
            // checkBoxEnableBle
            // 
            resources.ApplyResources(checkBoxEnableBle, "checkBoxEnableBle");
            checkBoxEnableBle.Name = "checkBoxEnableBle";
            checkBoxEnableBle.UseVisualStyleBackColor = true;
            checkBoxEnableBle.CheckedChanged += checkBoxEnableBle_CheckedChanged;
            // 
            // FormMain
            // 
            AcceptButton = buttonClose;
            resources.ApplyResources(this, "$this");
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            Controls.Add(checkBoxEnableBle);
            Controls.Add(richTextBoxStatus);
            Controls.Add(labelLanguage);
            Controls.Add(comboBoxLanguage);
            Controls.Add(groupBoxIstad);
            Controls.Add(groupBoxVasPc);
            Controls.Add(groupBoxEdiabas);
            Controls.Add(labelWiFiPassword);
            Controls.Add(textBoxWifiPassword);
            Controls.Add(labelBtDevices);
            Controls.Add(labelBluetoothPin);
            Controls.Add(textBoxBluetoothPin);
            Controls.Add(buttonTest);
            Controls.Add(labelStatus);
            Controls.Add(buttonClose);
            Controls.Add(buttonSearch);
            Controls.Add(listViewDevices);
            MaximizeBox = false;
            Name = "FormMain";
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            FormClosing += FormMain_FormClosing;
            FormClosed += FormMain_FormClosed;
            Load += FormMain_Load;
            Shown += FormMain_Shown;
            Resize += FormMain_Resize;
            groupBoxEdiabas.ResumeLayout(false);
            groupBoxVasPc.ResumeLayout(false);
            groupBoxIstad.ResumeLayout(false);
            groupBoxIstad.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ListView listViewDevices;
        private System.Windows.Forms.ColumnHeader columnHeaderAddress;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.Button buttonSearch;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Button buttonTest;
        private System.Windows.Forms.Label labelBluetoothPin;
        private System.Windows.Forms.Label labelBtDevices;
        private System.Windows.Forms.Button buttonPatchEdiabas;
        private System.Windows.Forms.OpenFileDialog openFileDialogConfigFile;
        private System.Windows.Forms.Label labelWiFiPassword;
        private System.Windows.Forms.TextBox textBoxBluetoothPin;
        private System.Windows.Forms.TextBox textBoxWifiPassword;
        private System.Windows.Forms.Button buttonRestoreEdiabas;
        private System.Windows.Forms.GroupBox groupBoxEdiabas;
        private System.Windows.Forms.GroupBox groupBoxVasPc;
        private System.Windows.Forms.Button buttonPatchVasPc;
        private System.Windows.Forms.Button buttonRestoreVasPc;
        private System.Windows.Forms.GroupBox groupBoxIstad;
        private System.Windows.Forms.Button buttonPatchIstad;
        private System.Windows.Forms.Button buttonRestoreIstad;
        private System.Windows.Forms.Button buttonDirIstad;
        private System.Windows.Forms.ComboBox comboBoxLanguage;
        private System.Windows.Forms.Label labelLanguage;
        private System.Windows.Forms.TextBox textBoxIstaLocation;
        private System.Windows.Forms.RichTextBox richTextBoxStatus;
        private System.Windows.Forms.CheckBox checkBoxEnableBle;
    }
}


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
            this.listViewDevices = new System.Windows.Forms.ListView();
            this.columnHeaderAddress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonSearch = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.textBoxStatus = new System.Windows.Forms.TextBox();
            this.buttonTest = new System.Windows.Forms.Button();
            this.textBoxBluetoothPin = new System.Windows.Forms.TextBox();
            this.labelBluetoothPin = new System.Windows.Forms.Label();
            this.labelBtDevices = new System.Windows.Forms.Label();
            this.buttonPatchEdiabas = new System.Windows.Forms.Button();
            this.openFileDialogConfigFile = new System.Windows.Forms.OpenFileDialog();
            this.textBoxWifiPassword = new System.Windows.Forms.TextBox();
            this.labelWiFiPassword = new System.Windows.Forms.Label();
            this.buttonRestoreEdiabas = new System.Windows.Forms.Button();
            this.groupBoxEdiabas = new System.Windows.Forms.GroupBox();
            this.groupBoxVasPc = new System.Windows.Forms.GroupBox();
            this.buttonPatchVasPc = new System.Windows.Forms.Button();
            this.buttonRestoreVasPc = new System.Windows.Forms.Button();
            this.groupBoxIstad = new System.Windows.Forms.GroupBox();
            this.buttonDirIstad = new System.Windows.Forms.Button();
            this.buttonPatchIstad = new System.Windows.Forms.Button();
            this.buttonRestoreIstad = new System.Windows.Forms.Button();
            this.comboBoxLanguage = new System.Windows.Forms.ComboBox();
            this.labelLanguage = new System.Windows.Forms.Label();
            this.textBoxIstaLocation = new System.Windows.Forms.TextBox();
            this.groupBoxEdiabas.SuspendLayout();
            this.groupBoxVasPc.SuspendLayout();
            this.groupBoxIstad.SuspendLayout();
            this.SuspendLayout();
            // 
            // listViewDevices
            // 
            this.listViewDevices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderAddress,
            this.columnHeaderName});
            this.listViewDevices.FullRowSelect = true;
            this.listViewDevices.GridLines = true;
            this.listViewDevices.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewDevices.HideSelection = false;
            resources.ApplyResources(this.listViewDevices, "listViewDevices");
            this.listViewDevices.MultiSelect = false;
            this.listViewDevices.Name = "listViewDevices";
            this.listViewDevices.ShowGroups = false;
            this.listViewDevices.UseCompatibleStateImageBehavior = false;
            this.listViewDevices.View = System.Windows.Forms.View.Details;
            this.listViewDevices.ColumnWidthChanging += new System.Windows.Forms.ColumnWidthChangingEventHandler(this.listViewDevices_ColumnWidthChanging);
            this.listViewDevices.SelectedIndexChanged += new System.EventHandler(this.listViewDevices_SelectedIndexChanged);
            this.listViewDevices.DoubleClick += new System.EventHandler(this.listViewDevices_DoubleClick);
            // 
            // columnHeaderAddress
            // 
            resources.ApplyResources(this.columnHeaderAddress, "columnHeaderAddress");
            // 
            // columnHeaderName
            // 
            resources.ApplyResources(this.columnHeaderName, "columnHeaderName");
            // 
            // buttonSearch
            // 
            resources.ApplyResources(this.buttonSearch, "buttonSearch");
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.UseVisualStyleBackColor = true;
            this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources(this.buttonClose, "buttonClose");
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // labelStatus
            // 
            resources.ApplyResources(this.labelStatus, "labelStatus");
            this.labelStatus.Name = "labelStatus";
            // 
            // textBoxStatus
            // 
            resources.ApplyResources(this.textBoxStatus, "textBoxStatus");
            this.textBoxStatus.Name = "textBoxStatus";
            this.textBoxStatus.ReadOnly = true;
            this.textBoxStatus.TabStop = false;
            // 
            // buttonTest
            // 
            resources.ApplyResources(this.buttonTest, "buttonTest");
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.UseVisualStyleBackColor = true;
            this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
            // 
            // textBoxBluetoothPin
            // 
            resources.ApplyResources(this.textBoxBluetoothPin, "textBoxBluetoothPin");
            this.textBoxBluetoothPin.Name = "textBoxBluetoothPin";
            this.textBoxBluetoothPin.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxBluetoothPin_KeyPress);
            // 
            // labelBluetoothPin
            // 
            resources.ApplyResources(this.labelBluetoothPin, "labelBluetoothPin");
            this.labelBluetoothPin.Name = "labelBluetoothPin";
            // 
            // labelBtDevices
            // 
            resources.ApplyResources(this.labelBtDevices, "labelBtDevices");
            this.labelBtDevices.Name = "labelBtDevices";
            // 
            // buttonPatchEdiabas
            // 
            resources.ApplyResources(this.buttonPatchEdiabas, "buttonPatchEdiabas");
            this.buttonPatchEdiabas.Name = "buttonPatchEdiabas";
            this.buttonPatchEdiabas.UseVisualStyleBackColor = true;
            this.buttonPatchEdiabas.Click += new System.EventHandler(this.buttonPatch_Click);
            // 
            // openFileDialogConfigFile
            // 
            resources.ApplyResources(this.openFileDialogConfigFile, "openFileDialogConfigFile");
            // 
            // textBoxWifiPassword
            // 
            resources.ApplyResources(this.textBoxWifiPassword, "textBoxWifiPassword");
            this.textBoxWifiPassword.Name = "textBoxWifiPassword";
            // 
            // labelWiFiPassword
            // 
            resources.ApplyResources(this.labelWiFiPassword, "labelWiFiPassword");
            this.labelWiFiPassword.Name = "labelWiFiPassword";
            // 
            // buttonRestoreEdiabas
            // 
            resources.ApplyResources(this.buttonRestoreEdiabas, "buttonRestoreEdiabas");
            this.buttonRestoreEdiabas.Name = "buttonRestoreEdiabas";
            this.buttonRestoreEdiabas.UseVisualStyleBackColor = true;
            this.buttonRestoreEdiabas.Click += new System.EventHandler(this.buttonRestore_Click);
            // 
            // groupBoxEdiabas
            // 
            this.groupBoxEdiabas.Controls.Add(this.buttonPatchEdiabas);
            this.groupBoxEdiabas.Controls.Add(this.buttonRestoreEdiabas);
            resources.ApplyResources(this.groupBoxEdiabas, "groupBoxEdiabas");
            this.groupBoxEdiabas.Name = "groupBoxEdiabas";
            this.groupBoxEdiabas.TabStop = false;
            // 
            // groupBoxVasPc
            // 
            this.groupBoxVasPc.Controls.Add(this.buttonPatchVasPc);
            this.groupBoxVasPc.Controls.Add(this.buttonRestoreVasPc);
            resources.ApplyResources(this.groupBoxVasPc, "groupBoxVasPc");
            this.groupBoxVasPc.Name = "groupBoxVasPc";
            this.groupBoxVasPc.TabStop = false;
            // 
            // buttonPatchVasPc
            // 
            resources.ApplyResources(this.buttonPatchVasPc, "buttonPatchVasPc");
            this.buttonPatchVasPc.Name = "buttonPatchVasPc";
            this.buttonPatchVasPc.UseVisualStyleBackColor = true;
            this.buttonPatchVasPc.Click += new System.EventHandler(this.buttonPatch_Click);
            // 
            // buttonRestoreVasPc
            // 
            resources.ApplyResources(this.buttonRestoreVasPc, "buttonRestoreVasPc");
            this.buttonRestoreVasPc.Name = "buttonRestoreVasPc";
            this.buttonRestoreVasPc.UseVisualStyleBackColor = true;
            this.buttonRestoreVasPc.Click += new System.EventHandler(this.buttonRestore_Click);
            // 
            // groupBoxIstad
            // 
            this.groupBoxIstad.Controls.Add(this.textBoxIstaLocation);
            this.groupBoxIstad.Controls.Add(this.buttonDirIstad);
            this.groupBoxIstad.Controls.Add(this.buttonPatchIstad);
            this.groupBoxIstad.Controls.Add(this.buttonRestoreIstad);
            resources.ApplyResources(this.groupBoxIstad, "groupBoxIstad");
            this.groupBoxIstad.Name = "groupBoxIstad";
            this.groupBoxIstad.TabStop = false;
            // 
            // buttonDirIstad
            // 
            resources.ApplyResources(this.buttonDirIstad, "buttonDirIstad");
            this.buttonDirIstad.Name = "buttonDirIstad";
            this.buttonDirIstad.UseVisualStyleBackColor = true;
            this.buttonDirIstad.Click += new System.EventHandler(this.buttonDirIstad_Click);
            // 
            // buttonPatchIstad
            // 
            resources.ApplyResources(this.buttonPatchIstad, "buttonPatchIstad");
            this.buttonPatchIstad.Name = "buttonPatchIstad";
            this.buttonPatchIstad.UseVisualStyleBackColor = true;
            this.buttonPatchIstad.Click += new System.EventHandler(this.buttonPatch_Click);
            // 
            // buttonRestoreIstad
            // 
            resources.ApplyResources(this.buttonRestoreIstad, "buttonRestoreIstad");
            this.buttonRestoreIstad.Name = "buttonRestoreIstad";
            this.buttonRestoreIstad.UseVisualStyleBackColor = true;
            this.buttonRestoreIstad.Click += new System.EventHandler(this.buttonRestore_Click);
            // 
            // comboBoxLanguage
            // 
            this.comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguage.FormattingEnabled = true;
            resources.ApplyResources(this.comboBoxLanguage, "comboBoxLanguage");
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.SelectedIndexChanged += new System.EventHandler(this.comboBoxLanguage_SelectedIndexChanged);
            // 
            // labelLanguage
            // 
            resources.ApplyResources(this.labelLanguage, "labelLanguage");
            this.labelLanguage.Name = "labelLanguage";
            // 
            // textBoxIstaLocation
            // 
            resources.ApplyResources(this.textBoxIstaLocation, "textBoxIstaLocation");
            this.textBoxIstaLocation.Name = "textBoxIstaLocation";
            this.textBoxIstaLocation.ReadOnly = true;
            this.textBoxIstaLocation.TabStop = false;
            // 
            // FormMain
            // 
            this.AcceptButton = this.buttonClose;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.labelLanguage);
            this.Controls.Add(this.comboBoxLanguage);
            this.Controls.Add(this.groupBoxIstad);
            this.Controls.Add(this.groupBoxVasPc);
            this.Controls.Add(this.groupBoxEdiabas);
            this.Controls.Add(this.labelWiFiPassword);
            this.Controls.Add(this.textBoxWifiPassword);
            this.Controls.Add(this.labelBtDevices);
            this.Controls.Add(this.labelBluetoothPin);
            this.Controls.Add(this.textBoxBluetoothPin);
            this.Controls.Add(this.buttonTest);
            this.Controls.Add(this.textBoxStatus);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonSearch);
            this.Controls.Add(this.listViewDevices);
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMain_FormClosed);
            this.Shown += new System.EventHandler(this.FormMain_Shown);
            this.groupBoxEdiabas.ResumeLayout(false);
            this.groupBoxVasPc.ResumeLayout(false);
            this.groupBoxIstad.ResumeLayout(false);
            this.groupBoxIstad.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listViewDevices;
        private System.Windows.Forms.ColumnHeader columnHeaderAddress;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.Button buttonSearch;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.TextBox textBoxStatus;
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
    }
}


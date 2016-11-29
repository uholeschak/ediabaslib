namespace BluetoothDeviceSelector
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
            this.checkBoxAutoMode = new System.Windows.Forms.CheckBox();
            this.buttonUpdateConfigFile = new System.Windows.Forms.Button();
            this.openFileDialogConfigFile = new System.Windows.Forms.OpenFileDialog();
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
            // checkBoxAutoMode
            // 
            resources.ApplyResources(this.checkBoxAutoMode, "checkBoxAutoMode");
            this.checkBoxAutoMode.Name = "checkBoxAutoMode";
            this.checkBoxAutoMode.UseVisualStyleBackColor = true;
            // 
            // buttonUpdateConfigFile
            // 
            resources.ApplyResources(this.buttonUpdateConfigFile, "buttonUpdateConfigFile");
            this.buttonUpdateConfigFile.Name = "buttonUpdateConfigFile";
            this.buttonUpdateConfigFile.UseVisualStyleBackColor = true;
            this.buttonUpdateConfigFile.Click += new System.EventHandler(this.buttonUpdateConfigFile_Click);
            // 
            // openFileDialogConfigFile
            // 
            resources.ApplyResources(this.openFileDialogConfigFile, "openFileDialogConfigFile");
            // 
            // FormMain
            // 
            this.AcceptButton = this.buttonClose;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonUpdateConfigFile);
            this.Controls.Add(this.checkBoxAutoMode);
            this.Controls.Add(this.labelBtDevices);
            this.Controls.Add(this.labelBluetoothPin);
            this.Controls.Add(this.textBoxBluetoothPin);
            this.Controls.Add(this.buttonTest);
            this.Controls.Add(this.textBoxStatus);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonSearch);
            this.Controls.Add(this.listViewDevices);
            this.Name = "FormMain";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMain_FormClosed);
            this.Shown += new System.EventHandler(this.FormMain_Shown);
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
        private System.Windows.Forms.TextBox textBoxBluetoothPin;
        private System.Windows.Forms.Label labelBluetoothPin;
        private System.Windows.Forms.Label labelBtDevices;
        private System.Windows.Forms.CheckBox checkBoxAutoMode;
        private System.Windows.Forms.Button buttonUpdateConfigFile;
        private System.Windows.Forms.OpenFileDialog openFileDialogConfigFile;
    }
}


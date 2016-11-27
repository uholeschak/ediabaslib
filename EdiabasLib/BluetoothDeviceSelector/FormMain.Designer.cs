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
            this.listViewDevices.Location = new System.Drawing.Point(12, 25);
            this.listViewDevices.MultiSelect = false;
            this.listViewDevices.Name = "listViewDevices";
            this.listViewDevices.ShowGroups = false;
            this.listViewDevices.Size = new System.Drawing.Size(525, 232);
            this.listViewDevices.TabIndex = 1;
            this.listViewDevices.UseCompatibleStateImageBehavior = false;
            this.listViewDevices.View = System.Windows.Forms.View.Details;
            this.listViewDevices.ColumnWidthChanging += new System.Windows.Forms.ColumnWidthChangingEventHandler(this.listViewDevices_ColumnWidthChanging);
            this.listViewDevices.SelectedIndexChanged += new System.EventHandler(this.listViewDevices_SelectedIndexChanged);
            this.listViewDevices.DoubleClick += new System.EventHandler(this.listViewDevices_DoubleClick);
            // 
            // columnHeaderAddress
            // 
            this.columnHeaderAddress.Text = "Address";
            this.columnHeaderAddress.Width = 193;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Name";
            this.columnHeaderName.Width = 322;
            // 
            // buttonSearch
            // 
            this.buttonSearch.Location = new System.Drawing.Point(12, 402);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(75, 23);
            this.buttonSearch.TabIndex = 5;
            this.buttonSearch.Text = "Search";
            this.buttonSearch.UseVisualStyleBackColor = true;
            this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonClose.Location = new System.Drawing.Point(462, 402);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 0;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(12, 260);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(40, 13);
            this.labelStatus.TabIndex = 3;
            this.labelStatus.Text = "Status:";
            // 
            // textBoxStatus
            // 
            this.textBoxStatus.Location = new System.Drawing.Point(12, 276);
            this.textBoxStatus.Multiline = true;
            this.textBoxStatus.Name = "textBoxStatus";
            this.textBoxStatus.ReadOnly = true;
            this.textBoxStatus.Size = new System.Drawing.Size(525, 81);
            this.textBoxStatus.TabIndex = 2;
            this.textBoxStatus.TabStop = false;
            // 
            // buttonTest
            // 
            this.buttonTest.Location = new System.Drawing.Point(93, 402);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(120, 23);
            this.buttonTest.TabIndex = 6;
            this.buttonTest.Text = "Test Connection";
            this.buttonTest.UseVisualStyleBackColor = true;
            this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
            // 
            // textBoxBluetoothPin
            // 
            this.textBoxBluetoothPin.Location = new System.Drawing.Point(12, 375);
            this.textBoxBluetoothPin.MaxLength = 16;
            this.textBoxBluetoothPin.Name = "textBoxBluetoothPin";
            this.textBoxBluetoothPin.Size = new System.Drawing.Size(156, 20);
            this.textBoxBluetoothPin.TabIndex = 3;
            this.textBoxBluetoothPin.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxBluetoothPin_KeyPress);
            // 
            // labelBluetoothPin
            // 
            this.labelBluetoothPin.AutoSize = true;
            this.labelBluetoothPin.Location = new System.Drawing.Point(9, 360);
            this.labelBluetoothPin.Name = "labelBluetoothPin";
            this.labelBluetoothPin.Size = new System.Drawing.Size(76, 13);
            this.labelBluetoothPin.TabIndex = 7;
            this.labelBluetoothPin.Text = "Bluetooth PIN:";
            // 
            // labelBtDevices
            // 
            this.labelBtDevices.AutoSize = true;
            this.labelBtDevices.Location = new System.Drawing.Point(12, 9);
            this.labelBtDevices.Name = "labelBtDevices";
            this.labelBtDevices.Size = new System.Drawing.Size(124, 13);
            this.labelBtDevices.TabIndex = 8;
            this.labelBtDevices.Text = "Bluetooth/Wi-Fi devices:";
            // 
            // checkBoxAutoMode
            // 
            this.checkBoxAutoMode.AutoSize = true;
            this.checkBoxAutoMode.Location = new System.Drawing.Point(174, 378);
            this.checkBoxAutoMode.Name = "checkBoxAutoMode";
            this.checkBoxAutoMode.Size = new System.Drawing.Size(187, 17);
            this.checkBoxAutoMode.TabIndex = 4;
            this.checkBoxAutoMode.Text = "Switch adapter to automatic mode";
            this.checkBoxAutoMode.UseVisualStyleBackColor = true;
            // 
            // buttonUpdateConfigFile
            // 
            this.buttonUpdateConfigFile.Location = new System.Drawing.Point(219, 402);
            this.buttonUpdateConfigFile.Name = "buttonUpdateConfigFile";
            this.buttonUpdateConfigFile.Size = new System.Drawing.Size(120, 23);
            this.buttonUpdateConfigFile.TabIndex = 9;
            this.buttonUpdateConfigFile.Text = "Update Config File";
            this.buttonUpdateConfigFile.UseVisualStyleBackColor = true;
            this.buttonUpdateConfigFile.Click += new System.EventHandler(this.buttonUpdateConfigFile_Click);
            // 
            // openFileDialogConfigFile
            // 
            this.openFileDialogConfigFile.Filter = "EdiabasLib.config File|EdiabasLib.config| All Files|*.*";
            // 
            // FormMain
            // 
            this.AcceptButton = this.buttonClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(549, 437);
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
            this.Text = "Deep OBD Bluetooth Device Selector";
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


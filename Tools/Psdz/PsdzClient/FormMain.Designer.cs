
namespace PsdzClient
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.textBoxIstaFolder = new System.Windows.Forms.TextBox();
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonAbort = new System.Windows.Forms.Button();
            this.folderBrowserDialogIsta = new System.Windows.Forms.FolderBrowserDialog();
            this.buttonIstaFolder = new System.Windows.Forms.Button();
            this.buttonStartHost = new System.Windows.Forms.Button();
            this.buttonStopHost = new System.Windows.Forms.Button();
            this.timerUpdate = new System.Windows.Forms.Timer(this.components);
            this.textBoxStatus = new System.Windows.Forms.TextBox();
            this.ipAddressControlVehicleIp = new IPAddressControlLib.IPAddressControl();
            this.labelVehicleIp = new System.Windows.Forms.Label();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.buttonDisconnect = new System.Windows.Forms.Button();
            this.buttonModILevel = new System.Windows.Forms.Button();
            this.labelIstaFolder = new System.Windows.Forms.Label();
            this.buttonModFa = new System.Windows.Forms.Button();
            this.buttonExecuteTal = new System.Windows.Forms.Button();
            this.progressBarEvent = new System.Windows.Forms.ProgressBar();
            this.labelProgressEvent = new System.Windows.Forms.Label();
            this.checkBoxIcom = new System.Windows.Forms.CheckBox();
            this.buttonVehicleSearch = new System.Windows.Forms.Button();
            this.buttonCreateOptions = new System.Windows.Forms.Button();
            this.checkedListBoxOptions = new System.Windows.Forms.CheckedListBox();
            this.SuspendLayout();
            // 
            // textBoxIstaFolder
            // 
            this.textBoxIstaFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxIstaFolder.Location = new System.Drawing.Point(93, 12);
            this.textBoxIstaFolder.Name = "textBoxIstaFolder";
            this.textBoxIstaFolder.Size = new System.Drawing.Size(673, 20);
            this.textBoxIstaFolder.TabIndex = 3;
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonClose.Location = new System.Drawing.Point(723, 604);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 0;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonAbort
            // 
            this.buttonAbort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAbort.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonAbort.Location = new System.Drawing.Point(642, 604);
            this.buttonAbort.Name = "buttonAbort";
            this.buttonAbort.Size = new System.Drawing.Size(75, 23);
            this.buttonAbort.TabIndex = 1;
            this.buttonAbort.Text = "Abort";
            this.buttonAbort.UseVisualStyleBackColor = true;
            this.buttonAbort.Click += new System.EventHandler(this.buttonAbort_Click);
            // 
            // buttonIstaFolder
            // 
            this.buttonIstaFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonIstaFolder.Location = new System.Drawing.Point(772, 10);
            this.buttonIstaFolder.Name = "buttonIstaFolder";
            this.buttonIstaFolder.Size = new System.Drawing.Size(30, 23);
            this.buttonIstaFolder.TabIndex = 4;
            this.buttonIstaFolder.Text = "...";
            this.buttonIstaFolder.UseVisualStyleBackColor = true;
            this.buttonIstaFolder.Click += new System.EventHandler(this.buttonIstaFolder_Click);
            // 
            // buttonStartHost
            // 
            this.buttonStartHost.Location = new System.Drawing.Point(12, 64);
            this.buttonStartHost.Name = "buttonStartHost";
            this.buttonStartHost.Size = new System.Drawing.Size(75, 23);
            this.buttonStartHost.TabIndex = 9;
            this.buttonStartHost.Text = "Start Host";
            this.buttonStartHost.UseVisualStyleBackColor = true;
            this.buttonStartHost.Click += new System.EventHandler(this.buttonStartHost_Click);
            // 
            // buttonStopHost
            // 
            this.buttonStopHost.Location = new System.Drawing.Point(93, 64);
            this.buttonStopHost.Name = "buttonStopHost";
            this.buttonStopHost.Size = new System.Drawing.Size(75, 23);
            this.buttonStopHost.TabIndex = 10;
            this.buttonStopHost.Text = "Stop Host";
            this.buttonStopHost.UseVisualStyleBackColor = true;
            this.buttonStopHost.Click += new System.EventHandler(this.buttonStopHost_Click);
            // 
            // timerUpdate
            // 
            this.timerUpdate.Tick += new System.EventHandler(this.timerUpdate_Tick);
            // 
            // textBoxStatus
            // 
            this.textBoxStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxStatus.Location = new System.Drawing.Point(12, 261);
            this.textBoxStatus.Multiline = true;
            this.textBoxStatus.Name = "textBoxStatus";
            this.textBoxStatus.ReadOnly = true;
            this.textBoxStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxStatus.Size = new System.Drawing.Size(790, 337);
            this.textBoxStatus.TabIndex = 18;
            // 
            // ipAddressControlVehicleIp
            // 
            this.ipAddressControlVehicleIp.AllowInternalTab = false;
            this.ipAddressControlVehicleIp.AutoHeight = true;
            this.ipAddressControlVehicleIp.BackColor = System.Drawing.SystemColors.Window;
            this.ipAddressControlVehicleIp.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ipAddressControlVehicleIp.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.ipAddressControlVehicleIp.Location = new System.Drawing.Point(93, 38);
            this.ipAddressControlVehicleIp.MinimumSize = new System.Drawing.Size(87, 20);
            this.ipAddressControlVehicleIp.Name = "ipAddressControlVehicleIp";
            this.ipAddressControlVehicleIp.ReadOnly = false;
            this.ipAddressControlVehicleIp.Size = new System.Drawing.Size(87, 20);
            this.ipAddressControlVehicleIp.TabIndex = 6;
            this.ipAddressControlVehicleIp.Text = "...";
            // 
            // labelVehicleIp
            // 
            this.labelVehicleIp.AutoSize = true;
            this.labelVehicleIp.Location = new System.Drawing.Point(12, 41);
            this.labelVehicleIp.Name = "labelVehicleIp";
            this.labelVehicleIp.Size = new System.Drawing.Size(57, 13);
            this.labelVehicleIp.TabIndex = 5;
            this.labelVehicleIp.Text = "Vehicle Ip:";
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(174, 64);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(75, 23);
            this.buttonConnect.TabIndex = 11;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // buttonDisconnect
            // 
            this.buttonDisconnect.Location = new System.Drawing.Point(255, 64);
            this.buttonDisconnect.Name = "buttonDisconnect";
            this.buttonDisconnect.Size = new System.Drawing.Size(75, 23);
            this.buttonDisconnect.TabIndex = 12;
            this.buttonDisconnect.Text = "Disconnect";
            this.buttonDisconnect.UseVisualStyleBackColor = true;
            this.buttonDisconnect.Click += new System.EventHandler(this.buttonDisconnect_Click);
            // 
            // buttonModILevel
            // 
            this.buttonModILevel.Location = new System.Drawing.Point(417, 64);
            this.buttonModILevel.Name = "buttonModILevel";
            this.buttonModILevel.Size = new System.Drawing.Size(75, 23);
            this.buttonModILevel.TabIndex = 14;
            this.buttonModILevel.Text = "Mod. ILevel";
            this.buttonModILevel.UseVisualStyleBackColor = true;
            this.buttonModILevel.Click += new System.EventHandler(this.buttonFunc_Click);
            // 
            // labelIstaFolder
            // 
            this.labelIstaFolder.AutoSize = true;
            this.labelIstaFolder.Location = new System.Drawing.Point(12, 15);
            this.labelIstaFolder.Name = "labelIstaFolder";
            this.labelIstaFolder.Size = new System.Drawing.Size(59, 13);
            this.labelIstaFolder.TabIndex = 2;
            this.labelIstaFolder.Text = "Ista Folder:";
            // 
            // buttonModFa
            // 
            this.buttonModFa.Location = new System.Drawing.Point(498, 64);
            this.buttonModFa.Name = "buttonModFa";
            this.buttonModFa.Size = new System.Drawing.Size(75, 23);
            this.buttonModFa.TabIndex = 15;
            this.buttonModFa.Text = "Mod. Fa";
            this.buttonModFa.UseVisualStyleBackColor = true;
            this.buttonModFa.Click += new System.EventHandler(this.buttonFunc_Click);
            // 
            // buttonExecuteTal
            // 
            this.buttonExecuteTal.Location = new System.Drawing.Point(579, 64);
            this.buttonExecuteTal.Name = "buttonExecuteTal";
            this.buttonExecuteTal.Size = new System.Drawing.Size(75, 23);
            this.buttonExecuteTal.TabIndex = 16;
            this.buttonExecuteTal.Text = "Execute Tal";
            this.buttonExecuteTal.UseVisualStyleBackColor = true;
            this.buttonExecuteTal.Click += new System.EventHandler(this.buttonFunc_Click);
            // 
            // progressBarEvent
            // 
            this.progressBarEvent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.progressBarEvent.Location = new System.Drawing.Point(12, 604);
            this.progressBarEvent.Name = "progressBarEvent";
            this.progressBarEvent.Size = new System.Drawing.Size(140, 23);
            this.progressBarEvent.TabIndex = 19;
            // 
            // labelProgressEvent
            // 
            this.labelProgressEvent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelProgressEvent.AutoSize = true;
            this.labelProgressEvent.Location = new System.Drawing.Point(158, 609);
            this.labelProgressEvent.Name = "labelProgressEvent";
            this.labelProgressEvent.Size = new System.Drawing.Size(21, 13);
            this.labelProgressEvent.TabIndex = 20;
            this.labelProgressEvent.Text = "0%";
            // 
            // checkBoxIcom
            // 
            this.checkBoxIcom.AutoSize = true;
            this.checkBoxIcom.Location = new System.Drawing.Point(186, 40);
            this.checkBoxIcom.Name = "checkBoxIcom";
            this.checkBoxIcom.Size = new System.Drawing.Size(53, 17);
            this.checkBoxIcom.TabIndex = 7;
            this.checkBoxIcom.Text = "ICOM";
            this.checkBoxIcom.UseVisualStyleBackColor = true;
            // 
            // buttonVehicleSearch
            // 
            this.buttonVehicleSearch.Location = new System.Drawing.Point(255, 36);
            this.buttonVehicleSearch.Name = "buttonVehicleSearch";
            this.buttonVehicleSearch.Size = new System.Drawing.Size(75, 23);
            this.buttonVehicleSearch.TabIndex = 8;
            this.buttonVehicleSearch.Text = "Search";
            this.buttonVehicleSearch.UseVisualStyleBackColor = true;
            this.buttonVehicleSearch.Click += new System.EventHandler(this.buttonVehicleSearch_Click);
            // 
            // buttonCreateOptions
            // 
            this.buttonCreateOptions.Location = new System.Drawing.Point(336, 64);
            this.buttonCreateOptions.Name = "buttonCreateOptions";
            this.buttonCreateOptions.Size = new System.Drawing.Size(75, 23);
            this.buttonCreateOptions.TabIndex = 13;
            this.buttonCreateOptions.Text = "List Options";
            this.buttonCreateOptions.UseVisualStyleBackColor = true;
            this.buttonCreateOptions.Click += new System.EventHandler(this.buttonFunc_Click);
            // 
            // checkedListBoxOptions
            // 
            this.checkedListBoxOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedListBoxOptions.CheckOnClick = true;
            this.checkedListBoxOptions.Location = new System.Drawing.Point(12, 93);
            this.checkedListBoxOptions.Name = "checkedListBoxOptions";
            this.checkedListBoxOptions.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.checkedListBoxOptions.Size = new System.Drawing.Size(790, 154);
            this.checkedListBoxOptions.TabIndex = 17;
            this.checkedListBoxOptions.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBoxOptions_ItemCheck);
            // 
            // FormMain
            // 
            this.AcceptButton = this.buttonClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonAbort;
            this.ClientSize = new System.Drawing.Size(810, 639);
            this.Controls.Add(this.checkedListBoxOptions);
            this.Controls.Add(this.buttonCreateOptions);
            this.Controls.Add(this.buttonVehicleSearch);
            this.Controls.Add(this.checkBoxIcom);
            this.Controls.Add(this.labelProgressEvent);
            this.Controls.Add(this.progressBarEvent);
            this.Controls.Add(this.buttonExecuteTal);
            this.Controls.Add(this.buttonModFa);
            this.Controls.Add(this.labelIstaFolder);
            this.Controls.Add(this.buttonModILevel);
            this.Controls.Add(this.buttonDisconnect);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.labelVehicleIp);
            this.Controls.Add(this.ipAddressControlVehicleIp);
            this.Controls.Add(this.textBoxStatus);
            this.Controls.Add(this.buttonStopHost);
            this.Controls.Add(this.buttonStartHost);
            this.Controls.Add(this.buttonIstaFolder);
            this.Controls.Add(this.buttonAbort);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.textBoxIstaFolder);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.Text = "PsdzClient";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMain_FormClosed);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxIstaFolder;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonAbort;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogIsta;
        private System.Windows.Forms.Button buttonIstaFolder;
        private System.Windows.Forms.Button buttonStartHost;
        private System.Windows.Forms.Button buttonStopHost;
        private System.Windows.Forms.Timer timerUpdate;
        private System.Windows.Forms.TextBox textBoxStatus;
        private IPAddressControlLib.IPAddressControl ipAddressControlVehicleIp;
        private System.Windows.Forms.Label labelVehicleIp;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Button buttonDisconnect;
        private System.Windows.Forms.Button buttonModILevel;
        private System.Windows.Forms.Label labelIstaFolder;
        private System.Windows.Forms.Button buttonModFa;
        private System.Windows.Forms.Button buttonExecuteTal;
        private System.Windows.Forms.ProgressBar progressBarEvent;
        private System.Windows.Forms.Label labelProgressEvent;
        private System.Windows.Forms.CheckBox checkBoxIcom;
        private System.Windows.Forms.Button buttonVehicleSearch;
        private System.Windows.Forms.Button buttonCreateOptions;
        private System.Windows.Forms.CheckedListBox checkedListBoxOptions;
    }
}


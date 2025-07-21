
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
            this.buttonStopHost = new System.Windows.Forms.Button();
            this.timerUpdate = new System.Windows.Forms.Timer(this.components);
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
            this.comboBoxOptionType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labelLanguage = new System.Windows.Forms.Label();
            this.comboBoxLanguage = new System.Windows.Forms.ComboBox();
            this.buttonInternalTest = new System.Windows.Forms.Button();
            this.openFileDialogTest = new System.Windows.Forms.OpenFileDialog();
            this.checkBoxGenServiceModules = new System.Windows.Forms.CheckBox();
            this.buttonDecryptFile = new System.Windows.Forms.Button();
            this.openFileDialogDecrypt = new System.Windows.Forms.OpenFileDialog();
            this.textBoxStatus = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // textBoxIstaFolder
            // 
            this.textBoxIstaFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxIstaFolder.Location = new System.Drawing.Point(93, 12);
            this.textBoxIstaFolder.Name = "textBoxIstaFolder";
            this.textBoxIstaFolder.Size = new System.Drawing.Size(647, 20);
            this.textBoxIstaFolder.TabIndex = 1;
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonClose.Location = new System.Drawing.Point(697, 626);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 26;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonAbort
            // 
            this.buttonAbort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAbort.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonAbort.Location = new System.Drawing.Point(616, 626);
            this.buttonAbort.Name = "buttonAbort";
            this.buttonAbort.Size = new System.Drawing.Size(75, 23);
            this.buttonAbort.TabIndex = 25;
            this.buttonAbort.Text = "Abort";
            this.buttonAbort.UseVisualStyleBackColor = true;
            this.buttonAbort.Click += new System.EventHandler(this.buttonAbort_Click);
            // 
            // folderBrowserDialogIsta
            // 
            this.folderBrowserDialogIsta.RootFolder = System.Environment.SpecialFolder.MyComputer;
            this.folderBrowserDialogIsta.ShowNewFolderButton = false;
            // 
            // buttonIstaFolder
            // 
            this.buttonIstaFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonIstaFolder.Location = new System.Drawing.Point(746, 10);
            this.buttonIstaFolder.Name = "buttonIstaFolder";
            this.buttonIstaFolder.Size = new System.Drawing.Size(30, 23);
            this.buttonIstaFolder.TabIndex = 2;
            this.buttonIstaFolder.Text = "...";
            this.buttonIstaFolder.UseVisualStyleBackColor = true;
            this.buttonIstaFolder.Click += new System.EventHandler(this.buttonIstaFolder_Click);
            // 
            // buttonStopHost
            // 
            this.buttonStopHost.Location = new System.Drawing.Point(12, 97);
            this.buttonStopHost.Name = "buttonStopHost";
            this.buttonStopHost.Size = new System.Drawing.Size(75, 23);
            this.buttonStopHost.TabIndex = 12;
            this.buttonStopHost.Text = "Stop Host";
            this.buttonStopHost.UseVisualStyleBackColor = true;
            this.buttonStopHost.Click += new System.EventHandler(this.buttonStopHost_Click);
            // 
            // timerUpdate
            // 
            this.timerUpdate.Tick += new System.EventHandler(this.timerUpdate_Tick);
            // 
            // ipAddressControlVehicleIp
            // 
            this.ipAddressControlVehicleIp.AllowInternalTab = false;
            this.ipAddressControlVehicleIp.AutoHeight = true;
            this.ipAddressControlVehicleIp.BackColor = System.Drawing.SystemColors.Window;
            this.ipAddressControlVehicleIp.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ipAddressControlVehicleIp.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.ipAddressControlVehicleIp.Location = new System.Drawing.Point(93, 68);
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
            this.labelVehicleIp.Location = new System.Drawing.Point(12, 73);
            this.labelVehicleIp.Name = "labelVehicleIp";
            this.labelVehicleIp.Size = new System.Drawing.Size(57, 13);
            this.labelVehicleIp.TabIndex = 5;
            this.labelVehicleIp.Text = "Vehicle Ip:";
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(93, 97);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(75, 23);
            this.buttonConnect.TabIndex = 13;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // buttonDisconnect
            // 
            this.buttonDisconnect.Location = new System.Drawing.Point(174, 97);
            this.buttonDisconnect.Name = "buttonDisconnect";
            this.buttonDisconnect.Size = new System.Drawing.Size(75, 23);
            this.buttonDisconnect.TabIndex = 14;
            this.buttonDisconnect.Text = "Disconnect";
            this.buttonDisconnect.UseVisualStyleBackColor = true;
            this.buttonDisconnect.Click += new System.EventHandler(this.buttonDisconnect_Click);
            // 
            // buttonModILevel
            // 
            this.buttonModILevel.Location = new System.Drawing.Point(336, 97);
            this.buttonModILevel.Name = "buttonModILevel";
            this.buttonModILevel.Size = new System.Drawing.Size(75, 23);
            this.buttonModILevel.TabIndex = 16;
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
            this.labelIstaFolder.TabIndex = 0;
            this.labelIstaFolder.Text = "Ista Folder:";
            // 
            // buttonModFa
            // 
            this.buttonModFa.Location = new System.Drawing.Point(417, 97);
            this.buttonModFa.Name = "buttonModFa";
            this.buttonModFa.Size = new System.Drawing.Size(75, 23);
            this.buttonModFa.TabIndex = 17;
            this.buttonModFa.Text = "Gen.Tal";
            this.buttonModFa.UseVisualStyleBackColor = true;
            this.buttonModFa.Click += new System.EventHandler(this.buttonFunc_Click);
            // 
            // buttonExecuteTal
            // 
            this.buttonExecuteTal.Location = new System.Drawing.Point(498, 97);
            this.buttonExecuteTal.Name = "buttonExecuteTal";
            this.buttonExecuteTal.Size = new System.Drawing.Size(75, 23);
            this.buttonExecuteTal.TabIndex = 18;
            this.buttonExecuteTal.Text = "Execute Tal";
            this.buttonExecuteTal.UseVisualStyleBackColor = true;
            this.buttonExecuteTal.Click += new System.EventHandler(this.buttonFunc_Click);
            // 
            // progressBarEvent
            // 
            this.progressBarEvent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.progressBarEvent.Location = new System.Drawing.Point(12, 626);
            this.progressBarEvent.Name = "progressBarEvent";
            this.progressBarEvent.Size = new System.Drawing.Size(140, 23);
            this.progressBarEvent.TabIndex = 23;
            // 
            // labelProgressEvent
            // 
            this.labelProgressEvent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelProgressEvent.AutoSize = true;
            this.labelProgressEvent.Location = new System.Drawing.Point(158, 631);
            this.labelProgressEvent.Name = "labelProgressEvent";
            this.labelProgressEvent.Size = new System.Drawing.Size(21, 13);
            this.labelProgressEvent.TabIndex = 24;
            this.labelProgressEvent.Text = "0%";
            // 
            // checkBoxIcom
            // 
            this.checkBoxIcom.AutoSize = true;
            this.checkBoxIcom.Location = new System.Drawing.Point(186, 70);
            this.checkBoxIcom.Name = "checkBoxIcom";
            this.checkBoxIcom.Size = new System.Drawing.Size(53, 17);
            this.checkBoxIcom.TabIndex = 7;
            this.checkBoxIcom.Text = "ICOM";
            this.checkBoxIcom.UseVisualStyleBackColor = true;
            // 
            // buttonVehicleSearch
            // 
            this.buttonVehicleSearch.Location = new System.Drawing.Point(255, 66);
            this.buttonVehicleSearch.Name = "buttonVehicleSearch";
            this.buttonVehicleSearch.Size = new System.Drawing.Size(75, 23);
            this.buttonVehicleSearch.TabIndex = 8;
            this.buttonVehicleSearch.Text = "Search";
            this.buttonVehicleSearch.UseVisualStyleBackColor = true;
            this.buttonVehicleSearch.Click += new System.EventHandler(this.buttonVehicleSearch_Click);
            // 
            // buttonCreateOptions
            // 
            this.buttonCreateOptions.Location = new System.Drawing.Point(255, 97);
            this.buttonCreateOptions.Name = "buttonCreateOptions";
            this.buttonCreateOptions.Size = new System.Drawing.Size(75, 23);
            this.buttonCreateOptions.TabIndex = 15;
            this.buttonCreateOptions.Text = "Create Opt.";
            this.buttonCreateOptions.UseVisualStyleBackColor = true;
            this.buttonCreateOptions.Click += new System.EventHandler(this.buttonFunc_Click);
            // 
            // checkedListBoxOptions
            // 
            this.checkedListBoxOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedListBoxOptions.CheckOnClick = true;
            this.checkedListBoxOptions.Location = new System.Drawing.Point(12, 153);
            this.checkedListBoxOptions.Name = "checkedListBoxOptions";
            this.checkedListBoxOptions.Size = new System.Drawing.Size(764, 154);
            this.checkedListBoxOptions.TabIndex = 21;
            this.checkedListBoxOptions.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBoxOptions_ItemCheck);
            // 
            // comboBoxOptionType
            // 
            this.comboBoxOptionType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxOptionType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOptionType.FormattingEnabled = true;
            this.comboBoxOptionType.Location = new System.Drawing.Point(93, 126);
            this.comboBoxOptionType.Name = "comboBoxOptionType";
            this.comboBoxOptionType.Size = new System.Drawing.Size(683, 21);
            this.comboBoxOptionType.TabIndex = 20;
            this.comboBoxOptionType.SelectedIndexChanged += new System.EventHandler(this.comboBoxOptionType_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 129);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 19;
            this.label1.Text = "Operation:";
            // 
            // labelLanguage
            // 
            this.labelLanguage.AutoSize = true;
            this.labelLanguage.Location = new System.Drawing.Point(12, 44);
            this.labelLanguage.Name = "labelLanguage";
            this.labelLanguage.Size = new System.Drawing.Size(58, 13);
            this.labelLanguage.TabIndex = 3;
            this.labelLanguage.Text = "Language:";
            // 
            // comboBoxLanguage
            // 
            this.comboBoxLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguage.FormattingEnabled = true;
            this.comboBoxLanguage.Location = new System.Drawing.Point(93, 41);
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.Size = new System.Drawing.Size(683, 21);
            this.comboBoxLanguage.TabIndex = 4;
            this.comboBoxLanguage.SelectedIndexChanged += new System.EventHandler(this.comboBoxLanguage_SelectedIndexChanged);
            // 
            // buttonInternalTest
            // 
            this.buttonInternalTest.Location = new System.Drawing.Point(336, 66);
            this.buttonInternalTest.Name = "buttonInternalTest";
            this.buttonInternalTest.Size = new System.Drawing.Size(75, 23);
            this.buttonInternalTest.TabIndex = 9;
            this.buttonInternalTest.Text = "Int. Test";
            this.buttonInternalTest.UseVisualStyleBackColor = true;
            this.buttonInternalTest.Click += new System.EventHandler(this.buttonInternalTest_Click);
            // 
            // openFileDialogTest
            // 
            this.openFileDialogTest.DefaultExt = "*.xml";
            this.openFileDialogTest.Filter = "*.xml|*.xml|*.*|*.*";
            this.openFileDialogTest.ShowReadOnly = true;
            this.openFileDialogTest.SupportMultiDottedExtensions = true;
            // 
            // checkBoxGenServiceModules
            // 
            this.checkBoxGenServiceModules.AutoSize = true;
            this.checkBoxGenServiceModules.Location = new System.Drawing.Point(498, 70);
            this.checkBoxGenServiceModules.Name = "checkBoxGenServiceModules";
            this.checkBoxGenServiceModules.Size = new System.Drawing.Size(104, 17);
            this.checkBoxGenServiceModules.TabIndex = 11;
            this.checkBoxGenServiceModules.Text = "Service modules";
            this.checkBoxGenServiceModules.UseVisualStyleBackColor = true;
            // 
            // buttonDecryptFile
            // 
            this.buttonDecryptFile.Location = new System.Drawing.Point(417, 66);
            this.buttonDecryptFile.Name = "buttonDecryptFile";
            this.buttonDecryptFile.Size = new System.Drawing.Size(75, 23);
            this.buttonDecryptFile.TabIndex = 10;
            this.buttonDecryptFile.Text = "Decrypt";
            this.buttonDecryptFile.UseVisualStyleBackColor = true;
            this.buttonDecryptFile.Click += new System.EventHandler(this.buttonDecryptFile_Click);
            // 
            // openFileDialogDecrypt
            // 
            this.openFileDialogDecrypt.Filter = "*.*|*.*";
            this.openFileDialogDecrypt.ShowReadOnly = true;
            this.openFileDialogDecrypt.SupportMultiDottedExtensions = true;
            // 
            // textBoxStatus
            // 
            this.textBoxStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxStatus.Location = new System.Drawing.Point(12, 313);
            this.textBoxStatus.Name = "textBoxStatus";
            this.textBoxStatus.ReadOnly = true;
            this.textBoxStatus.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.textBoxStatus.Size = new System.Drawing.Size(760, 307);
            this.textBoxStatus.TabIndex = 22;
            this.textBoxStatus.Text = "";
            // 
            // FormMain
            // 
            this.AcceptButton = this.buttonClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonAbort;
            this.ClientSize = new System.Drawing.Size(784, 661);
            this.Controls.Add(this.textBoxStatus);
            this.Controls.Add(this.buttonDecryptFile);
            this.Controls.Add(this.checkBoxGenServiceModules);
            this.Controls.Add(this.buttonInternalTest);
            this.Controls.Add(this.comboBoxLanguage);
            this.Controls.Add(this.labelLanguage);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxOptionType);
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
            this.Controls.Add(this.buttonStopHost);
            this.Controls.Add(this.buttonIstaFolder);
            this.Controls.Add(this.buttonAbort);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.textBoxIstaFolder);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(800, 700);
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
        private System.Windows.Forms.Button buttonStopHost;
        private System.Windows.Forms.Timer timerUpdate;
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
        private System.Windows.Forms.ComboBox comboBoxOptionType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelLanguage;
        private System.Windows.Forms.ComboBox comboBoxLanguage;
        private System.Windows.Forms.Button buttonInternalTest;
        private System.Windows.Forms.OpenFileDialog openFileDialogTest;
        private System.Windows.Forms.CheckBox checkBoxGenServiceModules;
        private System.Windows.Forms.Button buttonDecryptFile;
        private System.Windows.Forms.OpenFileDialog openFileDialogDecrypt;
        private System.Windows.Forms.RichTextBox textBoxStatus;
    }
}


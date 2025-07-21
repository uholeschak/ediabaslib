
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
            this.saveFileDialogDecrypt = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // textBoxIstaFolder
            // 
            resources.ApplyResources(this.textBoxIstaFolder, "textBoxIstaFolder");
            this.textBoxIstaFolder.Name = "textBoxIstaFolder";
            // 
            // buttonClose
            // 
            resources.ApplyResources(this.buttonClose, "buttonClose");
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonAbort
            // 
            resources.ApplyResources(this.buttonAbort, "buttonAbort");
            this.buttonAbort.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonAbort.Name = "buttonAbort";
            this.buttonAbort.UseVisualStyleBackColor = true;
            this.buttonAbort.Click += new System.EventHandler(this.buttonAbort_Click);
            // 
            // folderBrowserDialogIsta
            // 
            resources.ApplyResources(this.folderBrowserDialogIsta, "folderBrowserDialogIsta");
            this.folderBrowserDialogIsta.RootFolder = System.Environment.SpecialFolder.MyComputer;
            this.folderBrowserDialogIsta.ShowNewFolderButton = false;
            // 
            // buttonIstaFolder
            // 
            resources.ApplyResources(this.buttonIstaFolder, "buttonIstaFolder");
            this.buttonIstaFolder.Name = "buttonIstaFolder";
            this.buttonIstaFolder.UseVisualStyleBackColor = true;
            this.buttonIstaFolder.Click += new System.EventHandler(this.buttonIstaFolder_Click);
            // 
            // buttonStopHost
            // 
            resources.ApplyResources(this.buttonStopHost, "buttonStopHost");
            this.buttonStopHost.Name = "buttonStopHost";
            this.buttonStopHost.UseVisualStyleBackColor = true;
            this.buttonStopHost.Click += new System.EventHandler(this.buttonStopHost_Click);
            // 
            // timerUpdate
            // 
            this.timerUpdate.Tick += new System.EventHandler(this.timerUpdate_Tick);
            // 
            // ipAddressControlVehicleIp
            // 
            resources.ApplyResources(this.ipAddressControlVehicleIp, "ipAddressControlVehicleIp");
            this.ipAddressControlVehicleIp.AllowInternalTab = false;
            this.ipAddressControlVehicleIp.AutoHeight = true;
            this.ipAddressControlVehicleIp.BackColor = System.Drawing.SystemColors.Window;
            this.ipAddressControlVehicleIp.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ipAddressControlVehicleIp.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.ipAddressControlVehicleIp.Name = "ipAddressControlVehicleIp";
            this.ipAddressControlVehicleIp.ReadOnly = false;
            // 
            // labelVehicleIp
            // 
            resources.ApplyResources(this.labelVehicleIp, "labelVehicleIp");
            this.labelVehicleIp.Name = "labelVehicleIp";
            // 
            // buttonConnect
            // 
            resources.ApplyResources(this.buttonConnect, "buttonConnect");
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // buttonDisconnect
            // 
            resources.ApplyResources(this.buttonDisconnect, "buttonDisconnect");
            this.buttonDisconnect.Name = "buttonDisconnect";
            this.buttonDisconnect.UseVisualStyleBackColor = true;
            this.buttonDisconnect.Click += new System.EventHandler(this.buttonDisconnect_Click);
            // 
            // buttonModILevel
            // 
            resources.ApplyResources(this.buttonModILevel, "buttonModILevel");
            this.buttonModILevel.Name = "buttonModILevel";
            this.buttonModILevel.UseVisualStyleBackColor = true;
            this.buttonModILevel.Click += new System.EventHandler(this.buttonFunc_Click);
            // 
            // labelIstaFolder
            // 
            resources.ApplyResources(this.labelIstaFolder, "labelIstaFolder");
            this.labelIstaFolder.Name = "labelIstaFolder";
            // 
            // buttonModFa
            // 
            resources.ApplyResources(this.buttonModFa, "buttonModFa");
            this.buttonModFa.Name = "buttonModFa";
            this.buttonModFa.UseVisualStyleBackColor = true;
            this.buttonModFa.Click += new System.EventHandler(this.buttonFunc_Click);
            // 
            // buttonExecuteTal
            // 
            resources.ApplyResources(this.buttonExecuteTal, "buttonExecuteTal");
            this.buttonExecuteTal.Name = "buttonExecuteTal";
            this.buttonExecuteTal.UseVisualStyleBackColor = true;
            this.buttonExecuteTal.Click += new System.EventHandler(this.buttonFunc_Click);
            // 
            // progressBarEvent
            // 
            resources.ApplyResources(this.progressBarEvent, "progressBarEvent");
            this.progressBarEvent.Name = "progressBarEvent";
            // 
            // labelProgressEvent
            // 
            resources.ApplyResources(this.labelProgressEvent, "labelProgressEvent");
            this.labelProgressEvent.Name = "labelProgressEvent";
            // 
            // checkBoxIcom
            // 
            resources.ApplyResources(this.checkBoxIcom, "checkBoxIcom");
            this.checkBoxIcom.Name = "checkBoxIcom";
            this.checkBoxIcom.UseVisualStyleBackColor = true;
            // 
            // buttonVehicleSearch
            // 
            resources.ApplyResources(this.buttonVehicleSearch, "buttonVehicleSearch");
            this.buttonVehicleSearch.Name = "buttonVehicleSearch";
            this.buttonVehicleSearch.UseVisualStyleBackColor = true;
            this.buttonVehicleSearch.Click += new System.EventHandler(this.buttonVehicleSearch_Click);
            // 
            // buttonCreateOptions
            // 
            resources.ApplyResources(this.buttonCreateOptions, "buttonCreateOptions");
            this.buttonCreateOptions.Name = "buttonCreateOptions";
            this.buttonCreateOptions.UseVisualStyleBackColor = true;
            this.buttonCreateOptions.Click += new System.EventHandler(this.buttonFunc_Click);
            // 
            // checkedListBoxOptions
            // 
            resources.ApplyResources(this.checkedListBoxOptions, "checkedListBoxOptions");
            this.checkedListBoxOptions.CheckOnClick = true;
            this.checkedListBoxOptions.Name = "checkedListBoxOptions";
            this.checkedListBoxOptions.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBoxOptions_ItemCheck);
            // 
            // comboBoxOptionType
            // 
            resources.ApplyResources(this.comboBoxOptionType, "comboBoxOptionType");
            this.comboBoxOptionType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOptionType.FormattingEnabled = true;
            this.comboBoxOptionType.Name = "comboBoxOptionType";
            this.comboBoxOptionType.SelectedIndexChanged += new System.EventHandler(this.comboBoxOptionType_SelectedIndexChanged);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // labelLanguage
            // 
            resources.ApplyResources(this.labelLanguage, "labelLanguage");
            this.labelLanguage.Name = "labelLanguage";
            // 
            // comboBoxLanguage
            // 
            resources.ApplyResources(this.comboBoxLanguage, "comboBoxLanguage");
            this.comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguage.FormattingEnabled = true;
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.SelectedIndexChanged += new System.EventHandler(this.comboBoxLanguage_SelectedIndexChanged);
            // 
            // buttonInternalTest
            // 
            resources.ApplyResources(this.buttonInternalTest, "buttonInternalTest");
            this.buttonInternalTest.Name = "buttonInternalTest";
            this.buttonInternalTest.UseVisualStyleBackColor = true;
            this.buttonInternalTest.Click += new System.EventHandler(this.buttonInternalTest_Click);
            // 
            // openFileDialogTest
            // 
            this.openFileDialogTest.DefaultExt = "*.xml";
            resources.ApplyResources(this.openFileDialogTest, "openFileDialogTest");
            this.openFileDialogTest.ShowReadOnly = true;
            this.openFileDialogTest.SupportMultiDottedExtensions = true;
            // 
            // checkBoxGenServiceModules
            // 
            resources.ApplyResources(this.checkBoxGenServiceModules, "checkBoxGenServiceModules");
            this.checkBoxGenServiceModules.Name = "checkBoxGenServiceModules";
            this.checkBoxGenServiceModules.UseVisualStyleBackColor = true;
            // 
            // buttonDecryptFile
            // 
            resources.ApplyResources(this.buttonDecryptFile, "buttonDecryptFile");
            this.buttonDecryptFile.Name = "buttonDecryptFile";
            this.buttonDecryptFile.UseVisualStyleBackColor = true;
            this.buttonDecryptFile.Click += new System.EventHandler(this.buttonDecryptFile_Click);
            // 
            // openFileDialogDecrypt
            // 
            resources.ApplyResources(this.openFileDialogDecrypt, "openFileDialogDecrypt");
            this.openFileDialogDecrypt.ShowReadOnly = true;
            this.openFileDialogDecrypt.SupportMultiDottedExtensions = true;
            // 
            // textBoxStatus
            // 
            resources.ApplyResources(this.textBoxStatus, "textBoxStatus");
            this.textBoxStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxStatus.DetectUrls = false;
            this.textBoxStatus.Name = "textBoxStatus";
            this.textBoxStatus.ReadOnly = true;
            // 
            // saveFileDialogDecrypt
            // 
            resources.ApplyResources(this.saveFileDialogDecrypt, "saveFileDialogDecrypt");
            // 
            // FormMain
            // 
            this.AcceptButton = this.buttonClose;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonAbort;
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
            this.MaximizeBox = false;
            this.Name = "FormMain";
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
        private System.Windows.Forms.SaveFileDialog saveFileDialogDecrypt;
    }
}


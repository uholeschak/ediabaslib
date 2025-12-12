
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            textBoxIstaFolder = new System.Windows.Forms.TextBox();
            buttonClose = new System.Windows.Forms.Button();
            buttonAbort = new System.Windows.Forms.Button();
            folderBrowserDialogIsta = new System.Windows.Forms.FolderBrowserDialog();
            buttonIstaFolder = new System.Windows.Forms.Button();
            buttonStopHost = new System.Windows.Forms.Button();
            timerUpdate = new System.Windows.Forms.Timer(components);
            labelVehicleIp = new System.Windows.Forms.Label();
            buttonConnect = new System.Windows.Forms.Button();
            buttonDisconnect = new System.Windows.Forms.Button();
            buttonModILevel = new System.Windows.Forms.Button();
            labelIstaFolder = new System.Windows.Forms.Label();
            buttonModFa = new System.Windows.Forms.Button();
            buttonExecuteTal = new System.Windows.Forms.Button();
            progressBarEvent = new System.Windows.Forms.ProgressBar();
            labelProgressEvent = new System.Windows.Forms.Label();
            checkBoxIcom = new System.Windows.Forms.CheckBox();
            buttonVehicleSearch = new System.Windows.Forms.Button();
            buttonCreateOptions = new System.Windows.Forms.Button();
            checkedListBoxOptions = new System.Windows.Forms.CheckedListBox();
            comboBoxOptionType = new System.Windows.Forms.ComboBox();
            label1 = new System.Windows.Forms.Label();
            labelLanguage = new System.Windows.Forms.Label();
            comboBoxLanguage = new System.Windows.Forms.ComboBox();
            buttonInternalTest = new System.Windows.Forms.Button();
            openFileDialogTest = new System.Windows.Forms.OpenFileDialog();
            checkBoxGenServiceModules = new System.Windows.Forms.CheckBox();
            buttonDecryptFile = new System.Windows.Forms.Button();
            openFileDialogDecrypt = new System.Windows.Forms.OpenFileDialog();
            textBoxStatus = new System.Windows.Forms.RichTextBox();
            saveFileDialogDecrypt = new System.Windows.Forms.SaveFileDialog();
            ipAddressControlVehicleIp = new PsdzClient.Controls.IpAddressControl();
            SuspendLayout();
            // 
            // textBoxIstaFolder
            // 
            resources.ApplyResources(textBoxIstaFolder, "textBoxIstaFolder");
            textBoxIstaFolder.Name = "textBoxIstaFolder";
            // 
            // buttonClose
            // 
            resources.ApplyResources(buttonClose, "buttonClose");
            buttonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonClose.Name = "buttonClose";
            buttonClose.UseVisualStyleBackColor = true;
            buttonClose.Click += buttonClose_Click;
            // 
            // buttonAbort
            // 
            resources.ApplyResources(buttonAbort, "buttonAbort");
            buttonAbort.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonAbort.Name = "buttonAbort";
            buttonAbort.UseVisualStyleBackColor = true;
            buttonAbort.Click += buttonAbort_Click;
            // 
            // folderBrowserDialogIsta
            // 
            folderBrowserDialogIsta.RootFolder = System.Environment.SpecialFolder.MyComputer;
            folderBrowserDialogIsta.ShowNewFolderButton = false;
            // 
            // buttonIstaFolder
            // 
            resources.ApplyResources(buttonIstaFolder, "buttonIstaFolder");
            buttonIstaFolder.Name = "buttonIstaFolder";
            buttonIstaFolder.UseVisualStyleBackColor = true;
            buttonIstaFolder.Click += buttonIstaFolder_Click;
            // 
            // buttonStopHost
            // 
            resources.ApplyResources(buttonStopHost, "buttonStopHost");
            buttonStopHost.Name = "buttonStopHost";
            buttonStopHost.UseVisualStyleBackColor = true;
            buttonStopHost.Click += buttonStopHost_Click;
            // 
            // timerUpdate
            // 
            timerUpdate.Tick += timerUpdate_Tick;
            // 
            // labelVehicleIp
            // 
            resources.ApplyResources(labelVehicleIp, "labelVehicleIp");
            labelVehicleIp.Name = "labelVehicleIp";
            // 
            // buttonConnect
            // 
            resources.ApplyResources(buttonConnect, "buttonConnect");
            buttonConnect.Name = "buttonConnect";
            buttonConnect.UseVisualStyleBackColor = true;
            buttonConnect.Click += buttonConnect_Click;
            // 
            // buttonDisconnect
            // 
            resources.ApplyResources(buttonDisconnect, "buttonDisconnect");
            buttonDisconnect.Name = "buttonDisconnect";
            buttonDisconnect.UseVisualStyleBackColor = true;
            buttonDisconnect.Click += buttonDisconnect_Click;
            // 
            // buttonModILevel
            // 
            resources.ApplyResources(buttonModILevel, "buttonModILevel");
            buttonModILevel.Name = "buttonModILevel";
            buttonModILevel.UseVisualStyleBackColor = true;
            buttonModILevel.Click += buttonFunc_Click;
            // 
            // labelIstaFolder
            // 
            resources.ApplyResources(labelIstaFolder, "labelIstaFolder");
            labelIstaFolder.Name = "labelIstaFolder";
            // 
            // buttonModFa
            // 
            resources.ApplyResources(buttonModFa, "buttonModFa");
            buttonModFa.Name = "buttonModFa";
            buttonModFa.UseVisualStyleBackColor = true;
            buttonModFa.Click += buttonFunc_Click;
            // 
            // buttonExecuteTal
            // 
            resources.ApplyResources(buttonExecuteTal, "buttonExecuteTal");
            buttonExecuteTal.Name = "buttonExecuteTal";
            buttonExecuteTal.UseVisualStyleBackColor = true;
            buttonExecuteTal.Click += buttonFunc_Click;
            // 
            // progressBarEvent
            // 
            resources.ApplyResources(progressBarEvent, "progressBarEvent");
            progressBarEvent.Name = "progressBarEvent";
            // 
            // labelProgressEvent
            // 
            resources.ApplyResources(labelProgressEvent, "labelProgressEvent");
            labelProgressEvent.Name = "labelProgressEvent";
            // 
            // checkBoxIcom
            // 
            resources.ApplyResources(checkBoxIcom, "checkBoxIcom");
            checkBoxIcom.Name = "checkBoxIcom";
            checkBoxIcom.UseVisualStyleBackColor = true;
            // 
            // buttonVehicleSearch
            // 
            resources.ApplyResources(buttonVehicleSearch, "buttonVehicleSearch");
            buttonVehicleSearch.Name = "buttonVehicleSearch";
            buttonVehicleSearch.UseVisualStyleBackColor = true;
            buttonVehicleSearch.Click += buttonVehicleSearch_Click;
            // 
            // buttonCreateOptions
            // 
            resources.ApplyResources(buttonCreateOptions, "buttonCreateOptions");
            buttonCreateOptions.Name = "buttonCreateOptions";
            buttonCreateOptions.UseVisualStyleBackColor = true;
            buttonCreateOptions.Click += buttonFunc_Click;
            // 
            // checkedListBoxOptions
            // 
            resources.ApplyResources(checkedListBoxOptions, "checkedListBoxOptions");
            checkedListBoxOptions.CheckOnClick = true;
            checkedListBoxOptions.Name = "checkedListBoxOptions";
            checkedListBoxOptions.ItemCheck += checkedListBoxOptions_ItemCheck;
            // 
            // comboBoxOptionType
            // 
            resources.ApplyResources(comboBoxOptionType, "comboBoxOptionType");
            comboBoxOptionType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxOptionType.FormattingEnabled = true;
            comboBoxOptionType.Name = "comboBoxOptionType";
            comboBoxOptionType.SelectedIndexChanged += comboBoxOptionType_SelectedIndexChanged;
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            label1.Name = "label1";
            // 
            // labelLanguage
            // 
            resources.ApplyResources(labelLanguage, "labelLanguage");
            labelLanguage.Name = "labelLanguage";
            // 
            // comboBoxLanguage
            // 
            resources.ApplyResources(comboBoxLanguage, "comboBoxLanguage");
            comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxLanguage.FormattingEnabled = true;
            comboBoxLanguage.Name = "comboBoxLanguage";
            comboBoxLanguage.SelectedIndexChanged += comboBoxLanguage_SelectedIndexChanged;
            // 
            // buttonInternalTest
            // 
            resources.ApplyResources(buttonInternalTest, "buttonInternalTest");
            buttonInternalTest.Name = "buttonInternalTest";
            buttonInternalTest.UseVisualStyleBackColor = true;
            buttonInternalTest.Click += buttonInternalTest_Click;
            // 
            // openFileDialogTest
            // 
            openFileDialogTest.DefaultExt = "*.xml";
            resources.ApplyResources(openFileDialogTest, "openFileDialogTest");
            openFileDialogTest.ShowReadOnly = true;
            openFileDialogTest.SupportMultiDottedExtensions = true;
            // 
            // checkBoxGenServiceModules
            // 
            resources.ApplyResources(checkBoxGenServiceModules, "checkBoxGenServiceModules");
            checkBoxGenServiceModules.Name = "checkBoxGenServiceModules";
            checkBoxGenServiceModules.UseVisualStyleBackColor = true;
            // 
            // buttonDecryptFile
            // 
            resources.ApplyResources(buttonDecryptFile, "buttonDecryptFile");
            buttonDecryptFile.Name = "buttonDecryptFile";
            buttonDecryptFile.UseVisualStyleBackColor = true;
            buttonDecryptFile.Click += buttonDecryptFile_Click;
            // 
            // openFileDialogDecrypt
            // 
            resources.ApplyResources(openFileDialogDecrypt, "openFileDialogDecrypt");
            openFileDialogDecrypt.ShowReadOnly = true;
            openFileDialogDecrypt.SupportMultiDottedExtensions = true;
            // 
            // textBoxStatus
            // 
            resources.ApplyResources(textBoxStatus, "textBoxStatus");
            textBoxStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            textBoxStatus.DetectUrls = false;
            textBoxStatus.Name = "textBoxStatus";
            textBoxStatus.ReadOnly = true;
            // 
            // saveFileDialogDecrypt
            // 
            resources.ApplyResources(saveFileDialogDecrypt, "saveFileDialogDecrypt");
            // 
            // ipAddressControlVehicleIp
            // 
            resources.ApplyResources(ipAddressControlVehicleIp, "ipAddressControlVehicleIp");
            ipAddressControlVehicleIp.Name = "ipAddressControlVehicleIp";
            // 
            // FormMain
            // 
            AcceptButton = buttonClose;
            resources.ApplyResources(this, "$this");
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonAbort;
            Controls.Add(ipAddressControlVehicleIp);
            Controls.Add(textBoxStatus);
            Controls.Add(buttonDecryptFile);
            Controls.Add(checkBoxGenServiceModules);
            Controls.Add(buttonInternalTest);
            Controls.Add(comboBoxLanguage);
            Controls.Add(labelLanguage);
            Controls.Add(label1);
            Controls.Add(comboBoxOptionType);
            Controls.Add(checkedListBoxOptions);
            Controls.Add(buttonCreateOptions);
            Controls.Add(buttonVehicleSearch);
            Controls.Add(checkBoxIcom);
            Controls.Add(labelProgressEvent);
            Controls.Add(progressBarEvent);
            Controls.Add(buttonExecuteTal);
            Controls.Add(buttonModFa);
            Controls.Add(labelIstaFolder);
            Controls.Add(buttonModILevel);
            Controls.Add(buttonDisconnect);
            Controls.Add(buttonConnect);
            Controls.Add(labelVehicleIp);
            Controls.Add(buttonStopHost);
            Controls.Add(buttonIstaFolder);
            Controls.Add(buttonAbort);
            Controls.Add(buttonClose);
            Controls.Add(textBoxIstaFolder);
            MaximizeBox = false;
            Name = "FormMain";
            FormClosing += FormMain_FormClosing;
            FormClosed += FormMain_FormClosed;
            Load += FormMain_Load;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxIstaFolder;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonAbort;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogIsta;
        private System.Windows.Forms.Button buttonIstaFolder;
        private System.Windows.Forms.Button buttonStopHost;
        private System.Windows.Forms.Timer timerUpdate;
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
        private Controls.IpAddressControl ipAddressControlVehicleIp;
    }
}


namespace S29CertGenerator
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            openCertFileDialog = new System.Windows.Forms.OpenFileDialog();
            buttonSelectCaKeyFile = new System.Windows.Forms.Button();
            textBoxCaCeyFile = new System.Windows.Forms.TextBox();
            buttonClose = new System.Windows.Forms.Button();
            buttonSelectJsonRequestFolder = new System.Windows.Forms.Button();
            textBoxJsonRequestFolder = new System.Windows.Forms.TextBox();
            buttonSelectCertOutputFolder = new System.Windows.Forms.Button();
            textBoxCertOutputFolder = new System.Windows.Forms.TextBox();
            buttonInstall = new System.Windows.Forms.Button();
            richTextBoxStatus = new System.Windows.Forms.RichTextBox();
            buttonSelectJsonResponseFolder = new System.Windows.Forms.Button();
            textBoxJsonResponseFolder = new System.Windows.Forms.TextBox();
            buttonSelectSecurityFolder = new System.Windows.Forms.Button();
            textBoxSecurityFolder = new System.Windows.Forms.TextBox();
            buttonSelectIstaKeyFile = new System.Windows.Forms.Button();
            textBoxIstaKeyFile = new System.Windows.Forms.TextBox();
            openIstaKeyFileDialog = new System.Windows.Forms.OpenFileDialog();
            checkBoxForceCreate = new System.Windows.Forms.CheckBox();
            textBoxCaCertsFile = new System.Windows.Forms.TextBox();
            buttonSelectCaCertsFile = new System.Windows.Forms.Button();
            textBoxTrustStoreFolder = new System.Windows.Forms.TextBox();
            buttonSelectTrustStoreFolder = new System.Windows.Forms.Button();
            buttonUninstall = new System.Windows.Forms.Button();
            openCaCertsFileDialog = new System.Windows.Forms.OpenFileDialog();
            buttonResetSettings = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // folderBrowserDialog
            // 
            folderBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
            folderBrowserDialog.ShowNewFolderButton = false;
            // 
            // openCertFileDialog
            // 
            openCertFileDialog.DefaultExt = "pfx";
            openCertFileDialog.Filter = "Key|*.pfx|All files|*.*";
            openCertFileDialog.Title = "Select server cert";
            // 
            // buttonSelectCaKeyFile
            // 
            buttonSelectCaKeyFile.Location = new System.Drawing.Point(12, 12);
            buttonSelectCaKeyFile.Name = "buttonSelectCaKeyFile";
            buttonSelectCaKeyFile.Size = new System.Drawing.Size(179, 23);
            buttonSelectCaKeyFile.TabIndex = 0;
            buttonSelectCaKeyFile.Text = "Select CA Key";
            buttonSelectCaKeyFile.UseVisualStyleBackColor = true;
            buttonSelectCaKeyFile.Click += buttonSelectCaKeyFile_Click;
            // 
            // textBoxCaCeyFile
            // 
            textBoxCaCeyFile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxCaCeyFile.Location = new System.Drawing.Point(197, 12);
            textBoxCaCeyFile.Name = "textBoxCaCeyFile";
            textBoxCaCeyFile.ReadOnly = true;
            textBoxCaCeyFile.Size = new System.Drawing.Size(488, 23);
            textBoxCaCeyFile.TabIndex = 1;
            // 
            // buttonClose
            // 
            buttonClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonClose.Location = new System.Drawing.Point(610, 484);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new System.Drawing.Size(75, 23);
            buttonClose.TabIndex = 21;
            buttonClose.Text = "Close";
            buttonClose.UseVisualStyleBackColor = true;
            buttonClose.Click += buttonClose_Click;
            // 
            // buttonSelectJsonRequestFolder
            // 
            buttonSelectJsonRequestFolder.Location = new System.Drawing.Point(12, 128);
            buttonSelectJsonRequestFolder.Name = "buttonSelectJsonRequestFolder";
            buttonSelectJsonRequestFolder.Size = new System.Drawing.Size(179, 23);
            buttonSelectJsonRequestFolder.TabIndex = 8;
            buttonSelectJsonRequestFolder.Text = "Select JSON Request Dir";
            buttonSelectJsonRequestFolder.UseVisualStyleBackColor = true;
            buttonSelectJsonRequestFolder.Click += buttonSelectJsonRequestFolder_Click;
            // 
            // textBoxJsonRequestFolder
            // 
            textBoxJsonRequestFolder.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxJsonRequestFolder.Location = new System.Drawing.Point(197, 128);
            textBoxJsonRequestFolder.Name = "textBoxJsonRequestFolder";
            textBoxJsonRequestFolder.ReadOnly = true;
            textBoxJsonRequestFolder.Size = new System.Drawing.Size(488, 23);
            textBoxJsonRequestFolder.TabIndex = 9;
            // 
            // buttonSelectCertOutputFolder
            // 
            buttonSelectCertOutputFolder.Location = new System.Drawing.Point(12, 186);
            buttonSelectCertOutputFolder.Name = "buttonSelectCertOutputFolder";
            buttonSelectCertOutputFolder.Size = new System.Drawing.Size(179, 23);
            buttonSelectCertOutputFolder.TabIndex = 12;
            buttonSelectCertOutputFolder.Text = "Select Cert Output Dir";
            buttonSelectCertOutputFolder.UseVisualStyleBackColor = true;
            buttonSelectCertOutputFolder.Click += buttonSelectCertOutputFolder_Click;
            // 
            // textBoxCertOutputFolder
            // 
            textBoxCertOutputFolder.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxCertOutputFolder.Location = new System.Drawing.Point(197, 186);
            textBoxCertOutputFolder.Name = "textBoxCertOutputFolder";
            textBoxCertOutputFolder.ReadOnly = true;
            textBoxCertOutputFolder.Size = new System.Drawing.Size(488, 23);
            textBoxCertOutputFolder.TabIndex = 13;
            // 
            // buttonInstall
            // 
            buttonInstall.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonInstall.Location = new System.Drawing.Point(448, 484);
            buttonInstall.Name = "buttonInstall";
            buttonInstall.Size = new System.Drawing.Size(75, 23);
            buttonInstall.TabIndex = 19;
            buttonInstall.Text = "Install";
            buttonInstall.UseVisualStyleBackColor = true;
            buttonInstall.Click += buttonInstall_Click;
            // 
            // richTextBoxStatus
            // 
            richTextBoxStatus.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            richTextBoxStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            richTextBoxStatus.Location = new System.Drawing.Point(12, 244);
            richTextBoxStatus.Name = "richTextBoxStatus";
            richTextBoxStatus.ReadOnly = true;
            richTextBoxStatus.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            richTextBoxStatus.Size = new System.Drawing.Size(673, 204);
            richTextBoxStatus.TabIndex = 16;
            richTextBoxStatus.Text = "";
            // 
            // buttonSelectJsonResponseFolder
            // 
            buttonSelectJsonResponseFolder.Location = new System.Drawing.Point(12, 157);
            buttonSelectJsonResponseFolder.Name = "buttonSelectJsonResponseFolder";
            buttonSelectJsonResponseFolder.Size = new System.Drawing.Size(179, 23);
            buttonSelectJsonResponseFolder.TabIndex = 10;
            buttonSelectJsonResponseFolder.Text = "Select JSON Response Dir";
            buttonSelectJsonResponseFolder.UseVisualStyleBackColor = true;
            buttonSelectJsonResponseFolder.Click += buttonSelectJsonResponseFolder_Click;
            // 
            // textBoxJsonResponseFolder
            // 
            textBoxJsonResponseFolder.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxJsonResponseFolder.Location = new System.Drawing.Point(197, 157);
            textBoxJsonResponseFolder.Name = "textBoxJsonResponseFolder";
            textBoxJsonResponseFolder.ReadOnly = true;
            textBoxJsonResponseFolder.Size = new System.Drawing.Size(488, 23);
            textBoxJsonResponseFolder.TabIndex = 11;
            // 
            // buttonSelectSecurityFolder
            // 
            buttonSelectSecurityFolder.Location = new System.Drawing.Point(12, 99);
            buttonSelectSecurityFolder.Name = "buttonSelectSecurityFolder";
            buttonSelectSecurityFolder.Size = new System.Drawing.Size(179, 23);
            buttonSelectSecurityFolder.TabIndex = 6;
            buttonSelectSecurityFolder.Text = "Select EDIABAS Sec Dir";
            buttonSelectSecurityFolder.UseVisualStyleBackColor = true;
            buttonSelectSecurityFolder.Click += buttonSelectSecurityFolder_Click;
            // 
            // textBoxSecurityFolder
            // 
            textBoxSecurityFolder.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxSecurityFolder.Location = new System.Drawing.Point(197, 99);
            textBoxSecurityFolder.Name = "textBoxSecurityFolder";
            textBoxSecurityFolder.ReadOnly = true;
            textBoxSecurityFolder.Size = new System.Drawing.Size(488, 23);
            textBoxSecurityFolder.TabIndex = 7;
            // 
            // buttonSelectIstaKeyFile
            // 
            buttonSelectIstaKeyFile.Location = new System.Drawing.Point(12, 41);
            buttonSelectIstaKeyFile.Name = "buttonSelectIstaKeyFile";
            buttonSelectIstaKeyFile.Size = new System.Drawing.Size(179, 23);
            buttonSelectIstaKeyFile.TabIndex = 2;
            buttonSelectIstaKeyFile.Text = "Select ISTA Key file";
            buttonSelectIstaKeyFile.UseVisualStyleBackColor = true;
            buttonSelectIstaKeyFile.Click += buttonSelectIstaKeyFile_Click;
            // 
            // textBoxIstaKeyFile
            // 
            textBoxIstaKeyFile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxIstaKeyFile.Location = new System.Drawing.Point(197, 41);
            textBoxIstaKeyFile.Name = "textBoxIstaKeyFile";
            textBoxIstaKeyFile.ReadOnly = true;
            textBoxIstaKeyFile.Size = new System.Drawing.Size(488, 23);
            textBoxIstaKeyFile.TabIndex = 3;
            // 
            // openIstaKeyFileDialog
            // 
            openIstaKeyFileDialog.DefaultExt = "pfx";
            openIstaKeyFileDialog.Filter = "Key|*.pfx|All files|*.*";
            openIstaKeyFileDialog.Title = "Select ISTA key file";
            // 
            // checkBoxForceCreate
            // 
            checkBoxForceCreate.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            checkBoxForceCreate.AutoSize = true;
            checkBoxForceCreate.Location = new System.Drawing.Point(12, 487);
            checkBoxForceCreate.Name = "checkBoxForceCreate";
            checkBoxForceCreate.Size = new System.Drawing.Size(113, 19);
            checkBoxForceCreate.TabIndex = 18;
            checkBoxForceCreate.Text = "Force create cert";
            checkBoxForceCreate.UseVisualStyleBackColor = true;
            // 
            // textBoxCaCertsFile
            // 
            textBoxCaCertsFile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxCaCertsFile.Location = new System.Drawing.Point(197, 70);
            textBoxCaCertsFile.Name = "textBoxCaCertsFile";
            textBoxCaCertsFile.ReadOnly = true;
            textBoxCaCertsFile.Size = new System.Drawing.Size(488, 23);
            textBoxCaCertsFile.TabIndex = 5;
            // 
            // buttonSelectCaCertsFile
            // 
            buttonSelectCaCertsFile.Location = new System.Drawing.Point(12, 70);
            buttonSelectCaCertsFile.Name = "buttonSelectCaCertsFile";
            buttonSelectCaCertsFile.Size = new System.Drawing.Size(179, 23);
            buttonSelectCaCertsFile.TabIndex = 4;
            buttonSelectCaCertsFile.Text = "Select PSdZ CaCerts file";
            buttonSelectCaCertsFile.UseVisualStyleBackColor = true;
            buttonSelectCaCertsFile.Click += buttonSelectCaCertsFile_Click;
            // 
            // textBoxTrustStoreFolder
            // 
            textBoxTrustStoreFolder.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxTrustStoreFolder.Location = new System.Drawing.Point(197, 215);
            textBoxTrustStoreFolder.Name = "textBoxTrustStoreFolder";
            textBoxTrustStoreFolder.ReadOnly = true;
            textBoxTrustStoreFolder.Size = new System.Drawing.Size(488, 23);
            textBoxTrustStoreFolder.TabIndex = 15;
            // 
            // buttonSelectTrustStoreFolder
            // 
            buttonSelectTrustStoreFolder.Location = new System.Drawing.Point(12, 215);
            buttonSelectTrustStoreFolder.Name = "buttonSelectTrustStoreFolder";
            buttonSelectTrustStoreFolder.Size = new System.Drawing.Size(179, 23);
            buttonSelectTrustStoreFolder.TabIndex = 14;
            buttonSelectTrustStoreFolder.Text = "Select Truststore Dir";
            buttonSelectTrustStoreFolder.UseVisualStyleBackColor = true;
            buttonSelectTrustStoreFolder.Click += buttonSelectTrustStoreFolder_Click;
            // 
            // buttonUninstall
            // 
            buttonUninstall.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonUninstall.Location = new System.Drawing.Point(529, 484);
            buttonUninstall.Name = "buttonUninstall";
            buttonUninstall.Size = new System.Drawing.Size(75, 23);
            buttonUninstall.TabIndex = 20;
            buttonUninstall.Text = "Uninstall";
            buttonUninstall.UseVisualStyleBackColor = true;
            buttonUninstall.Click += buttonUninstall_Click;
            // 
            // openCaCertsFileDialog
            // 
            openCaCertsFileDialog.Filter = "CaCerts|cacerts|All files|*.*";
            openCaCertsFileDialog.Title = "select CaCerts file";
            // 
            // buttonResetSettings
            // 
            buttonResetSettings.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonResetSettings.Location = new System.Drawing.Point(529, 454);
            buttonResetSettings.Name = "buttonResetSettings";
            buttonResetSettings.Size = new System.Drawing.Size(156, 23);
            buttonResetSettings.TabIndex = 17;
            buttonResetSettings.Text = "Reset Settings";
            buttonResetSettings.UseVisualStyleBackColor = true;
            buttonResetSettings.Click += buttonResetSettings_Click;
            // 
            // MainForm
            // 
            AcceptButton = buttonInstall;
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            CancelButton = buttonClose;
            ClientSize = new System.Drawing.Size(697, 519);
            Controls.Add(buttonResetSettings);
            Controls.Add(buttonUninstall);
            Controls.Add(buttonSelectTrustStoreFolder);
            Controls.Add(textBoxTrustStoreFolder);
            Controls.Add(buttonSelectCaCertsFile);
            Controls.Add(textBoxCaCertsFile);
            Controls.Add(checkBoxForceCreate);
            Controls.Add(textBoxIstaKeyFile);
            Controls.Add(buttonSelectIstaKeyFile);
            Controls.Add(textBoxSecurityFolder);
            Controls.Add(buttonSelectSecurityFolder);
            Controls.Add(textBoxJsonResponseFolder);
            Controls.Add(buttonSelectJsonResponseFolder);
            Controls.Add(richTextBoxStatus);
            Controls.Add(buttonInstall);
            Controls.Add(textBoxCertOutputFolder);
            Controls.Add(buttonSelectCertOutputFolder);
            Controls.Add(textBoxJsonRequestFolder);
            Controls.Add(buttonSelectJsonRequestFolder);
            Controls.Add(buttonClose);
            Controls.Add(textBoxCaCeyFile);
            Controls.Add(buttonSelectCaKeyFile);
            MaximizeBox = false;
            MinimumSize = new System.Drawing.Size(600, 400);
            Name = "MainForm";
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            Text = "S29CertGenerator";
            FormClosing += MainForm_FormClosing;
            FormClosed += MainForm_FormClosed;
            Load += MainForm_Load;
            Shown += MainForm_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.OpenFileDialog openCertFileDialog;
        private System.Windows.Forms.Button buttonSelectCaKeyFile;
        private System.Windows.Forms.TextBox textBoxCaCeyFile;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonSelectJsonRequestFolder;
        private System.Windows.Forms.TextBox textBoxJsonRequestFolder;
        private System.Windows.Forms.Button buttonSelectCertOutputFolder;
        private System.Windows.Forms.TextBox textBoxCertOutputFolder;
        private System.Windows.Forms.Button buttonInstall;
        private System.Windows.Forms.RichTextBox richTextBoxStatus;
        private System.Windows.Forms.Button buttonSelectJsonResponseFolder;
        private System.Windows.Forms.TextBox textBoxJsonResponseFolder;
        private System.Windows.Forms.Button buttonSelectSecurityFolder;
        private System.Windows.Forms.TextBox textBoxSecurityFolder;
        private System.Windows.Forms.Button buttonSelectIstaKeyFile;
        private System.Windows.Forms.TextBox textBoxIstaKeyFile;
        private System.Windows.Forms.OpenFileDialog openIstaKeyFileDialog;
        private System.Windows.Forms.CheckBox checkBoxForceCreate;
        private System.Windows.Forms.TextBox textBoxCaCertsFile;
        private System.Windows.Forms.Button buttonSelectCaCertsFile;
        private System.Windows.Forms.TextBox textBoxTrustStoreFolder;
        private System.Windows.Forms.Button buttonSelectTrustStoreFolder;
        private System.Windows.Forms.Button buttonUninstall;
        private System.Windows.Forms.OpenFileDialog openCaCertsFileDialog;
        private System.Windows.Forms.Button buttonResetSettings;
    }
}

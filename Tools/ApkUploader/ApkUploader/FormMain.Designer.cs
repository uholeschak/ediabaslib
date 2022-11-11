namespace ApkUploader
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
            this.textBoxStatus = new System.Windows.Forms.TextBox();
            this.buttonListBundles = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonAbort = new System.Windows.Forms.Button();
            this.buttonUploadBundle = new System.Windows.Forms.Button();
            this.openFileDialogBundle = new System.Windows.Forms.OpenFileDialog();
            this.buttonListTracks = new System.Windows.Forms.Button();
            this.buttonSelectBundleFile = new System.Windows.Forms.Button();
            this.labelApkFile = new System.Windows.Forms.Label();
            this.textBoxBundleFile = new System.Windows.Forms.TextBox();
            this.labelObbFile = new System.Windows.Forms.Label();
            this.textBoxObbFile = new System.Windows.Forms.TextBox();
            this.buttonSelectObbFile = new System.Windows.Forms.Button();
            this.openFileDialogObb = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDialogResource = new System.Windows.Forms.FolderBrowserDialog();
            this.labelRescourceFolder = new System.Windows.Forms.Label();
            this.textBoxResourceFolder = new System.Windows.Forms.TextBox();
            this.buttonSelectResourceFolder = new System.Windows.Forms.Button();
            this.buttonChangeTrack = new System.Windows.Forms.Button();
            this.comboBoxTrackUnassign = new System.Windows.Forms.ComboBox();
            this.comboBoxTrackAssign = new System.Windows.Forms.ComboBox();
            this.labelTrackUnassign = new System.Windows.Forms.Label();
            this.labelTrackAssign = new System.Windows.Forms.Label();
            this.buttonAssignTrack = new System.Windows.Forms.Button();
            this.textBoxVersion = new System.Windows.Forms.TextBox();
            this.labelVersion = new System.Windows.Forms.Label();
            this.buttonUpdateChanges = new System.Windows.Forms.Button();
            this.checkBoxUpdateName = new System.Windows.Forms.CheckBox();
            this.buttonSetAppInfo = new System.Windows.Forms.Button();
            this.labelSerialFileName = new System.Windows.Forms.Label();
            this.textBoxSerialFileName = new System.Windows.Forms.TextBox();
            this.buttonSelectSerialFile = new System.Windows.Forms.Button();
            this.openFileDialogSerial = new System.Windows.Forms.OpenFileDialog();
            this.buttonUploadSerials = new System.Windows.Forms.Button();
            this.comboBoxSerialOem = new System.Windows.Forms.ComboBox();
            this.labelSerialOem = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBoxStatus
            // 
            this.textBoxStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxStatus.Location = new System.Drawing.Point(12, 226);
            this.textBoxStatus.Multiline = true;
            this.textBoxStatus.Name = "textBoxStatus";
            this.textBoxStatus.ReadOnly = true;
            this.textBoxStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxStatus.Size = new System.Drawing.Size(1060, 326);
            this.textBoxStatus.TabIndex = 22;
            this.textBoxStatus.TabStop = false;
            // 
            // buttonListBundles
            // 
            this.buttonListBundles.Location = new System.Drawing.Point(12, 12);
            this.buttonListBundles.Name = "buttonListBundles";
            this.buttonListBundles.Size = new System.Drawing.Size(115, 23);
            this.buttonListBundles.TabIndex = 2;
            this.buttonListBundles.Text = "List Bundles/Apks";
            this.buttonListBundles.UseVisualStyleBackColor = true;
            this.buttonListBundles.Click += new System.EventHandler(this.buttonListBundles_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Location = new System.Drawing.Point(997, 558);
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
            this.buttonAbort.Location = new System.Drawing.Point(916, 558);
            this.buttonAbort.Name = "buttonAbort";
            this.buttonAbort.Size = new System.Drawing.Size(75, 23);
            this.buttonAbort.TabIndex = 1;
            this.buttonAbort.Text = "Abort";
            this.buttonAbort.UseVisualStyleBackColor = true;
            this.buttonAbort.Click += new System.EventHandler(this.buttonAbort_Click);
            // 
            // buttonUploadBundle
            // 
            this.buttonUploadBundle.Location = new System.Drawing.Point(254, 41);
            this.buttonUploadBundle.Name = "buttonUploadBundle";
            this.buttonUploadBundle.Size = new System.Drawing.Size(115, 23);
            this.buttonUploadBundle.TabIndex = 7;
            this.buttonUploadBundle.Text = "Upload Bundle/Apk";
            this.buttonUploadBundle.UseVisualStyleBackColor = true;
            this.buttonUploadBundle.Click += new System.EventHandler(this.buttonUploadBundle_Click);
            // 
            // openFileDialogBundle
            // 
            this.openFileDialogBundle.DefaultExt = "*.aab";
            this.openFileDialogBundle.Filter = "Bundle/Apk Files|*.aab;*.apk";
            // 
            // buttonListTracks
            // 
            this.buttonListTracks.Location = new System.Drawing.Point(133, 12);
            this.buttonListTracks.Name = "buttonListTracks";
            this.buttonListTracks.Size = new System.Drawing.Size(115, 23);
            this.buttonListTracks.TabIndex = 3;
            this.buttonListTracks.Text = "List Tracks";
            this.buttonListTracks.UseVisualStyleBackColor = true;
            this.buttonListTracks.Click += new System.EventHandler(this.buttonListTracks_Click);
            // 
            // buttonSelectBundleFile
            // 
            this.buttonSelectBundleFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectBundleFile.Location = new System.Drawing.Point(1042, 81);
            this.buttonSelectBundleFile.Name = "buttonSelectBundleFile";
            this.buttonSelectBundleFile.Size = new System.Drawing.Size(30, 23);
            this.buttonSelectBundleFile.TabIndex = 12;
            this.buttonSelectBundleFile.Text = "...";
            this.buttonSelectBundleFile.UseVisualStyleBackColor = true;
            this.buttonSelectBundleFile.Click += new System.EventHandler(this.buttonSelectBundleFile_Click);
            // 
            // labelApkFile
            // 
            this.labelApkFile.AutoSize = true;
            this.labelApkFile.Location = new System.Drawing.Point(12, 67);
            this.labelApkFile.Name = "labelApkFile";
            this.labelApkFile.Size = new System.Drawing.Size(112, 13);
            this.labelApkFile.TabIndex = 10;
            this.labelApkFile.Text = "Bundle/Apk file name:";
            // 
            // textBoxBundleFile
            // 
            this.textBoxBundleFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxBundleFile.Location = new System.Drawing.Point(12, 83);
            this.textBoxBundleFile.Name = "textBoxBundleFile";
            this.textBoxBundleFile.Size = new System.Drawing.Size(1024, 20);
            this.textBoxBundleFile.TabIndex = 11;
            // 
            // labelObbFile
            // 
            this.labelObbFile.AutoSize = true;
            this.labelObbFile.Location = new System.Drawing.Point(12, 106);
            this.labelObbFile.Name = "labelObbFile";
            this.labelObbFile.Size = new System.Drawing.Size(203, 13);
            this.labelObbFile.TabIndex = 13;
            this.labelObbFile.Text = "Obb file name (use * to keep last version):";
            // 
            // textBoxObbFile
            // 
            this.textBoxObbFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxObbFile.Location = new System.Drawing.Point(12, 122);
            this.textBoxObbFile.Name = "textBoxObbFile";
            this.textBoxObbFile.Size = new System.Drawing.Size(1024, 20);
            this.textBoxObbFile.TabIndex = 14;
            // 
            // buttonSelectObbFile
            // 
            this.buttonSelectObbFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectObbFile.Location = new System.Drawing.Point(1042, 120);
            this.buttonSelectObbFile.Name = "buttonSelectObbFile";
            this.buttonSelectObbFile.Size = new System.Drawing.Size(30, 23);
            this.buttonSelectObbFile.TabIndex = 15;
            this.buttonSelectObbFile.Text = "...";
            this.buttonSelectObbFile.UseVisualStyleBackColor = true;
            this.buttonSelectObbFile.Click += new System.EventHandler(this.buttonSelectObbFile_Click);
            // 
            // openFileDialogObb
            // 
            this.openFileDialogObb.DefaultExt = "*.obb";
            this.openFileDialogObb.Filter = "Obb Files|*.obb";
            // 
            // folderBrowserDialogResource
            // 
            this.folderBrowserDialogResource.ShowNewFolderButton = false;
            // 
            // labelRescourceFolder
            // 
            this.labelRescourceFolder.AutoSize = true;
            this.labelRescourceFolder.Location = new System.Drawing.Point(12, 145);
            this.labelRescourceFolder.Name = "labelRescourceFolder";
            this.labelRescourceFolder.Size = new System.Drawing.Size(85, 13);
            this.labelRescourceFolder.TabIndex = 16;
            this.labelRescourceFolder.Text = "Resource folder:";
            // 
            // textBoxResourceFolder
            // 
            this.textBoxResourceFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxResourceFolder.Location = new System.Drawing.Point(12, 161);
            this.textBoxResourceFolder.Name = "textBoxResourceFolder";
            this.textBoxResourceFolder.Size = new System.Drawing.Size(1024, 20);
            this.textBoxResourceFolder.TabIndex = 17;
            // 
            // buttonSelectResourceFolder
            // 
            this.buttonSelectResourceFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectResourceFolder.Location = new System.Drawing.Point(1042, 159);
            this.buttonSelectResourceFolder.Name = "buttonSelectResourceFolder";
            this.buttonSelectResourceFolder.Size = new System.Drawing.Size(30, 23);
            this.buttonSelectResourceFolder.TabIndex = 18;
            this.buttonSelectResourceFolder.Text = "...";
            this.buttonSelectResourceFolder.UseVisualStyleBackColor = true;
            this.buttonSelectResourceFolder.Click += new System.EventHandler(this.buttonSelectResourceFolder_Click);
            // 
            // buttonChangeTrack
            // 
            this.buttonChangeTrack.Location = new System.Drawing.Point(12, 41);
            this.buttonChangeTrack.Name = "buttonChangeTrack";
            this.buttonChangeTrack.Size = new System.Drawing.Size(115, 23);
            this.buttonChangeTrack.TabIndex = 5;
            this.buttonChangeTrack.Text = "Change Track";
            this.buttonChangeTrack.UseVisualStyleBackColor = true;
            this.buttonChangeTrack.Click += new System.EventHandler(this.buttonChangeTrack_Click);
            // 
            // comboBoxTrackUnassign
            // 
            this.comboBoxTrackUnassign.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxTrackUnassign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTrackUnassign.FormattingEnabled = true;
            this.comboBoxTrackUnassign.Location = new System.Drawing.Point(936, 43);
            this.comboBoxTrackUnassign.Name = "comboBoxTrackUnassign";
            this.comboBoxTrackUnassign.Size = new System.Drawing.Size(100, 21);
            this.comboBoxTrackUnassign.TabIndex = 29;
            // 
            // comboBoxTrackAssign
            // 
            this.comboBoxTrackAssign.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxTrackAssign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTrackAssign.FormattingEnabled = true;
            this.comboBoxTrackAssign.Location = new System.Drawing.Point(936, 14);
            this.comboBoxTrackAssign.Name = "comboBoxTrackAssign";
            this.comboBoxTrackAssign.Size = new System.Drawing.Size(100, 21);
            this.comboBoxTrackAssign.TabIndex = 26;
            // 
            // labelTrackUnassign
            // 
            this.labelTrackUnassign.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTrackUnassign.AutoSize = true;
            this.labelTrackUnassign.Location = new System.Drawing.Point(847, 46);
            this.labelTrackUnassign.Name = "labelTrackUnassign";
            this.labelTrackUnassign.Size = new System.Drawing.Size(83, 13);
            this.labelTrackUnassign.TabIndex = 28;
            this.labelTrackUnassign.Text = "Track unassign:";
            // 
            // labelTrackAssign
            // 
            this.labelTrackAssign.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTrackAssign.AutoSize = true;
            this.labelTrackAssign.Location = new System.Drawing.Point(859, 17);
            this.labelTrackAssign.Name = "labelTrackAssign";
            this.labelTrackAssign.Size = new System.Drawing.Size(71, 13);
            this.labelTrackAssign.TabIndex = 25;
            this.labelTrackAssign.Text = "Track assign:";
            // 
            // buttonAssignTrack
            // 
            this.buttonAssignTrack.Location = new System.Drawing.Point(133, 41);
            this.buttonAssignTrack.Name = "buttonAssignTrack";
            this.buttonAssignTrack.Size = new System.Drawing.Size(115, 23);
            this.buttonAssignTrack.TabIndex = 6;
            this.buttonAssignTrack.Text = "Assign Track";
            this.buttonAssignTrack.UseVisualStyleBackColor = true;
            this.buttonAssignTrack.Click += new System.EventHandler(this.buttonAssignTrack_Click);
            // 
            // textBoxVersion
            // 
            this.textBoxVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxVersion.Location = new System.Drawing.Point(776, 14);
            this.textBoxVersion.Name = "textBoxVersion";
            this.textBoxVersion.Size = new System.Drawing.Size(49, 20);
            this.textBoxVersion.TabIndex = 24;
            // 
            // labelVersion
            // 
            this.labelVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelVersion.AutoSize = true;
            this.labelVersion.Location = new System.Drawing.Point(692, 17);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(78, 13);
            this.labelVersion.TabIndex = 23;
            this.labelVersion.Text = "Version assign:";
            // 
            // buttonUpdateChanges
            // 
            this.buttonUpdateChanges.Location = new System.Drawing.Point(254, 12);
            this.buttonUpdateChanges.Name = "buttonUpdateChanges";
            this.buttonUpdateChanges.Size = new System.Drawing.Size(115, 23);
            this.buttonUpdateChanges.TabIndex = 4;
            this.buttonUpdateChanges.Text = "Update Translations";
            this.buttonUpdateChanges.UseVisualStyleBackColor = true;
            this.buttonUpdateChanges.Click += new System.EventHandler(this.buttonUpdateChanges_Click);
            // 
            // checkBoxUpdateName
            // 
            this.checkBoxUpdateName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxUpdateName.Location = new System.Drawing.Point(695, 40);
            this.checkBoxUpdateName.Name = "checkBoxUpdateName";
            this.checkBoxUpdateName.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.checkBoxUpdateName.Size = new System.Drawing.Size(130, 24);
            this.checkBoxUpdateName.TabIndex = 27;
            this.checkBoxUpdateName.Text = "Update name";
            this.checkBoxUpdateName.UseVisualStyleBackColor = true;
            // 
            // buttonSetAppInfo
            // 
            this.buttonSetAppInfo.Location = new System.Drawing.Point(375, 41);
            this.buttonSetAppInfo.Name = "buttonSetAppInfo";
            this.buttonSetAppInfo.Size = new System.Drawing.Size(115, 23);
            this.buttonSetAppInfo.TabIndex = 8;
            this.buttonSetAppInfo.Text = "Set App Info";
            this.buttonSetAppInfo.UseVisualStyleBackColor = true;
            this.buttonSetAppInfo.Click += new System.EventHandler(this.buttonSetAppInfo_Click);
            // 
            // labelSerialFileName
            // 
            this.labelSerialFileName.AutoSize = true;
            this.labelSerialFileName.Location = new System.Drawing.Point(12, 184);
            this.labelSerialFileName.Name = "labelSerialFileName";
            this.labelSerialFileName.Size = new System.Drawing.Size(81, 13);
            this.labelSerialFileName.TabIndex = 19;
            this.labelSerialFileName.Text = "Serial file name:";
            // 
            // textBoxSerialFileName
            // 
            this.textBoxSerialFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSerialFileName.Location = new System.Drawing.Point(12, 200);
            this.textBoxSerialFileName.Name = "textBoxSerialFileName";
            this.textBoxSerialFileName.Size = new System.Drawing.Size(1024, 20);
            this.textBoxSerialFileName.TabIndex = 20;
            // 
            // buttonSelectSerialFile
            // 
            this.buttonSelectSerialFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectSerialFile.Location = new System.Drawing.Point(1042, 197);
            this.buttonSelectSerialFile.Name = "buttonSelectSerialFile";
            this.buttonSelectSerialFile.Size = new System.Drawing.Size(30, 23);
            this.buttonSelectSerialFile.TabIndex = 21;
            this.buttonSelectSerialFile.Text = "...";
            this.buttonSelectSerialFile.UseVisualStyleBackColor = true;
            this.buttonSelectSerialFile.Click += new System.EventHandler(this.buttonSelectSerialFile_Click);
            // 
            // openFileDialogSerial
            // 
            this.openFileDialogSerial.DefaultExt = "*.num";
            this.openFileDialogSerial.Filter = "Serial Number Files|*.num";
            // 
            // buttonUploadSerials
            // 
            this.buttonUploadSerials.Location = new System.Drawing.Point(496, 41);
            this.buttonUploadSerials.Name = "buttonUploadSerials";
            this.buttonUploadSerials.Size = new System.Drawing.Size(115, 23);
            this.buttonUploadSerials.TabIndex = 9;
            this.buttonUploadSerials.Text = "Upload Serials";
            this.buttonUploadSerials.UseVisualStyleBackColor = true;
            this.buttonUploadSerials.Click += new System.EventHandler(this.buttonUploadSerials_Click);
            // 
            // comboBoxSerialOem
            // 
            this.comboBoxSerialOem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSerialOem.FormattingEnabled = true;
            this.comboBoxSerialOem.Location = new System.Drawing.Point(496, 14);
            this.comboBoxSerialOem.Name = "comboBoxSerialOem";
            this.comboBoxSerialOem.Size = new System.Drawing.Size(115, 21);
            this.comboBoxSerialOem.TabIndex = 30;
            // 
            // labelSerialOem
            // 
            this.labelSerialOem.AutoSize = true;
            this.labelSerialOem.Location = new System.Drawing.Point(427, 17);
            this.labelSerialOem.Name = "labelSerialOem";
            this.labelSerialOem.Size = new System.Drawing.Size(63, 13);
            this.labelSerialOem.TabIndex = 31;
            this.labelSerialOem.Text = "Serial OEM:";
            // 
            // FormMain
            // 
            this.AcceptButton = this.buttonClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonAbort;
            this.ClientSize = new System.Drawing.Size(1084, 593);
            this.Controls.Add(this.labelSerialOem);
            this.Controls.Add(this.comboBoxSerialOem);
            this.Controls.Add(this.buttonUploadSerials);
            this.Controls.Add(this.buttonSelectSerialFile);
            this.Controls.Add(this.textBoxSerialFileName);
            this.Controls.Add(this.labelSerialFileName);
            this.Controls.Add(this.buttonSetAppInfo);
            this.Controls.Add(this.checkBoxUpdateName);
            this.Controls.Add(this.buttonUpdateChanges);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.textBoxVersion);
            this.Controls.Add(this.buttonAssignTrack);
            this.Controls.Add(this.labelTrackAssign);
            this.Controls.Add(this.labelTrackUnassign);
            this.Controls.Add(this.comboBoxTrackAssign);
            this.Controls.Add(this.comboBoxTrackUnassign);
            this.Controls.Add(this.buttonChangeTrack);
            this.Controls.Add(this.buttonSelectResourceFolder);
            this.Controls.Add(this.textBoxResourceFolder);
            this.Controls.Add(this.labelRescourceFolder);
            this.Controls.Add(this.buttonSelectObbFile);
            this.Controls.Add(this.textBoxObbFile);
            this.Controls.Add(this.labelObbFile);
            this.Controls.Add(this.textBoxBundleFile);
            this.Controls.Add(this.labelApkFile);
            this.Controls.Add(this.buttonSelectBundleFile);
            this.Controls.Add(this.buttonListTracks);
            this.Controls.Add(this.buttonUploadBundle);
            this.Controls.Add(this.buttonAbort);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonListBundles);
            this.Controls.Add(this.textBoxStatus);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1100, 600);
            this.Name = "FormMain";
            this.Text = "Bundle/Apk Uploader";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMain_FormClosed);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.Shown += new System.EventHandler(this.FormMain_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxStatus;
        private System.Windows.Forms.Button buttonListBundles;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonAbort;
        private System.Windows.Forms.Button buttonUploadBundle;
        private System.Windows.Forms.OpenFileDialog openFileDialogBundle;
        private System.Windows.Forms.Button buttonListTracks;
        private System.Windows.Forms.Button buttonSelectBundleFile;
        private System.Windows.Forms.Label labelApkFile;
        private System.Windows.Forms.TextBox textBoxBundleFile;
        private System.Windows.Forms.Label labelObbFile;
        private System.Windows.Forms.TextBox textBoxObbFile;
        private System.Windows.Forms.Button buttonSelectObbFile;
        private System.Windows.Forms.OpenFileDialog openFileDialogObb;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogResource;
        private System.Windows.Forms.Label labelRescourceFolder;
        private System.Windows.Forms.TextBox textBoxResourceFolder;
        private System.Windows.Forms.Button buttonSelectResourceFolder;
        private System.Windows.Forms.Button buttonChangeTrack;
        private System.Windows.Forms.ComboBox comboBoxTrackUnassign;
        private System.Windows.Forms.ComboBox comboBoxTrackAssign;
        private System.Windows.Forms.Label labelTrackUnassign;
        private System.Windows.Forms.Label labelTrackAssign;
        private System.Windows.Forms.Button buttonAssignTrack;
        private System.Windows.Forms.TextBox textBoxVersion;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Button buttonUpdateChanges;
        private System.Windows.Forms.CheckBox checkBoxUpdateName;
        private System.Windows.Forms.Button buttonSetAppInfo;
        private System.Windows.Forms.Label labelSerialFileName;
        private System.Windows.Forms.TextBox textBoxSerialFileName;
        private System.Windows.Forms.Button buttonSelectSerialFile;
        private System.Windows.Forms.OpenFileDialog openFileDialogSerial;
        private System.Windows.Forms.Button buttonUploadSerials;
        private System.Windows.Forms.ComboBox comboBoxSerialOem;
        private System.Windows.Forms.Label labelSerialOem;
    }
}


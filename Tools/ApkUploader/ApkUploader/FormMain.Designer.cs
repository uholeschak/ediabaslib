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
            textBoxStatus = new System.Windows.Forms.TextBox();
            buttonListBundles = new System.Windows.Forms.Button();
            buttonClose = new System.Windows.Forms.Button();
            buttonAbort = new System.Windows.Forms.Button();
            buttonUploadBundle = new System.Windows.Forms.Button();
            openFileDialogBundle = new System.Windows.Forms.OpenFileDialog();
            buttonListTracks = new System.Windows.Forms.Button();
            buttonSelectBundleFile = new System.Windows.Forms.Button();
            labelApkFile = new System.Windows.Forms.Label();
            textBoxBundleFile = new System.Windows.Forms.TextBox();
            labelObbFile = new System.Windows.Forms.Label();
            textBoxObbFile = new System.Windows.Forms.TextBox();
            buttonSelectObbFile = new System.Windows.Forms.Button();
            openFileDialogObb = new System.Windows.Forms.OpenFileDialog();
            folderBrowserDialogResource = new System.Windows.Forms.FolderBrowserDialog();
            labelRescourceFolder = new System.Windows.Forms.Label();
            textBoxResourceFolder = new System.Windows.Forms.TextBox();
            buttonSelectResourceFolder = new System.Windows.Forms.Button();
            buttonChangeTrack = new System.Windows.Forms.Button();
            comboBoxTrackUnassign = new System.Windows.Forms.ComboBox();
            comboBoxTrackAssign = new System.Windows.Forms.ComboBox();
            labelTrackUnassign = new System.Windows.Forms.Label();
            labelTrackAssign = new System.Windows.Forms.Label();
            buttonAssignTrack = new System.Windows.Forms.Button();
            textBoxVersion = new System.Windows.Forms.TextBox();
            labelVersion = new System.Windows.Forms.Label();
            buttonUpdateChanges = new System.Windows.Forms.Button();
            checkBoxUpdateName = new System.Windows.Forms.CheckBox();
            buttonSetAppInfo = new System.Windows.Forms.Button();
            labelSerialFileName = new System.Windows.Forms.Label();
            textBoxSerialFileName = new System.Windows.Forms.TextBox();
            buttonSelectSerialFile = new System.Windows.Forms.Button();
            openFileDialogSerial = new System.Windows.Forms.OpenFileDialog();
            buttonUploadSerials = new System.Windows.Forms.Button();
            comboBoxSerialOem = new System.Windows.Forms.ComboBox();
            labelSerialOem = new System.Windows.Forms.Label();
            buttonTriggerReview = new System.Windows.Forms.Button();
            textBoxUserFraction = new System.Windows.Forms.TextBox();
            labelUserFraction = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // textBoxStatus
            // 
            textBoxStatus.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxStatus.Location = new System.Drawing.Point(13, 254);
            textBoxStatus.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxStatus.Multiline = true;
            textBoxStatus.Name = "textBoxStatus";
            textBoxStatus.ReadOnly = true;
            textBoxStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            textBoxStatus.Size = new System.Drawing.Size(1363, 464);
            textBoxStatus.TabIndex = 23;
            textBoxStatus.TabStop = false;
            // 
            // buttonListBundles
            // 
            buttonListBundles.Location = new System.Drawing.Point(13, 12);
            buttonListBundles.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonListBundles.Name = "buttonListBundles";
            buttonListBundles.Size = new System.Drawing.Size(134, 27);
            buttonListBundles.TabIndex = 2;
            buttonListBundles.Text = "List Bundles/Apks";
            buttonListBundles.UseVisualStyleBackColor = true;
            buttonListBundles.Click += buttonListBundles_Click;
            // 
            // buttonClose
            // 
            buttonClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonClose.Location = new System.Drawing.Point(1288, 724);
            buttonClose.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new System.Drawing.Size(88, 27);
            buttonClose.TabIndex = 0;
            buttonClose.Text = "Close";
            buttonClose.UseVisualStyleBackColor = true;
            buttonClose.Click += buttonClose_Click;
            // 
            // buttonAbort
            // 
            buttonAbort.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonAbort.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonAbort.Location = new System.Drawing.Point(1192, 724);
            buttonAbort.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonAbort.Name = "buttonAbort";
            buttonAbort.Size = new System.Drawing.Size(88, 27);
            buttonAbort.TabIndex = 1;
            buttonAbort.Text = "Abort";
            buttonAbort.UseVisualStyleBackColor = true;
            buttonAbort.Click += buttonAbort_Click;
            // 
            // buttonUploadBundle
            // 
            buttonUploadBundle.Location = new System.Drawing.Point(297, 45);
            buttonUploadBundle.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonUploadBundle.Name = "buttonUploadBundle";
            buttonUploadBundle.Size = new System.Drawing.Size(134, 27);
            buttonUploadBundle.TabIndex = 8;
            buttonUploadBundle.Text = "Upload Bundle/Apk";
            buttonUploadBundle.UseVisualStyleBackColor = true;
            buttonUploadBundle.Click += buttonUploadBundle_Click;
            // 
            // openFileDialogBundle
            // 
            openFileDialogBundle.DefaultExt = "*.aab";
            openFileDialogBundle.Filter = "Bundle/Apk Files|*.aab;*.apk";
            // 
            // buttonListTracks
            // 
            buttonListTracks.Location = new System.Drawing.Point(155, 12);
            buttonListTracks.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonListTracks.Name = "buttonListTracks";
            buttonListTracks.Size = new System.Drawing.Size(134, 27);
            buttonListTracks.TabIndex = 3;
            buttonListTracks.Text = "List Tracks";
            buttonListTracks.UseVisualStyleBackColor = true;
            buttonListTracks.Click += buttonListTracks_Click;
            // 
            // buttonSelectBundleFile
            // 
            buttonSelectBundleFile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonSelectBundleFile.Location = new System.Drawing.Point(1341, 90);
            buttonSelectBundleFile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonSelectBundleFile.Name = "buttonSelectBundleFile";
            buttonSelectBundleFile.Size = new System.Drawing.Size(35, 27);
            buttonSelectBundleFile.TabIndex = 13;
            buttonSelectBundleFile.Text = "...";
            buttonSelectBundleFile.UseVisualStyleBackColor = true;
            buttonSelectBundleFile.Click += buttonSelectBundleFile_Click;
            // 
            // labelApkFile
            // 
            labelApkFile.AutoSize = true;
            labelApkFile.Location = new System.Drawing.Point(14, 75);
            labelApkFile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelApkFile.Name = "labelApkFile";
            labelApkFile.Size = new System.Drawing.Size(125, 15);
            labelApkFile.TabIndex = 11;
            labelApkFile.Text = "Bundle/Apk file name:";
            // 
            // textBoxBundleFile
            // 
            textBoxBundleFile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxBundleFile.Location = new System.Drawing.Point(13, 93);
            textBoxBundleFile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxBundleFile.Name = "textBoxBundleFile";
            textBoxBundleFile.Size = new System.Drawing.Size(1318, 23);
            textBoxBundleFile.TabIndex = 12;
            textBoxBundleFile.Leave += textBoxBundleFile_Leave;
            // 
            // labelObbFile
            // 
            labelObbFile.AutoSize = true;
            labelObbFile.Location = new System.Drawing.Point(14, 119);
            labelObbFile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelObbFile.Name = "labelObbFile";
            labelObbFile.Size = new System.Drawing.Size(226, 15);
            labelObbFile.TabIndex = 14;
            labelObbFile.Text = "Obb file name (use * to keep last version):";
            // 
            // textBoxObbFile
            // 
            textBoxObbFile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxObbFile.Location = new System.Drawing.Point(13, 137);
            textBoxObbFile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxObbFile.Name = "textBoxObbFile";
            textBoxObbFile.Size = new System.Drawing.Size(1318, 23);
            textBoxObbFile.TabIndex = 15;
            // 
            // buttonSelectObbFile
            // 
            buttonSelectObbFile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonSelectObbFile.Location = new System.Drawing.Point(1341, 134);
            buttonSelectObbFile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonSelectObbFile.Name = "buttonSelectObbFile";
            buttonSelectObbFile.Size = new System.Drawing.Size(35, 27);
            buttonSelectObbFile.TabIndex = 16;
            buttonSelectObbFile.Text = "...";
            buttonSelectObbFile.UseVisualStyleBackColor = true;
            buttonSelectObbFile.Click += buttonSelectObbFile_Click;
            // 
            // openFileDialogObb
            // 
            openFileDialogObb.DefaultExt = "*.obb";
            openFileDialogObb.Filter = "Obb Files|*.obb";
            // 
            // folderBrowserDialogResource
            // 
            folderBrowserDialogResource.ShowNewFolderButton = false;
            // 
            // labelRescourceFolder
            // 
            labelRescourceFolder.AutoSize = true;
            labelRescourceFolder.Location = new System.Drawing.Point(13, 163);
            labelRescourceFolder.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelRescourceFolder.Name = "labelRescourceFolder";
            labelRescourceFolder.Size = new System.Drawing.Size(92, 15);
            labelRescourceFolder.TabIndex = 17;
            labelRescourceFolder.Text = "Resource folder:";
            // 
            // textBoxResourceFolder
            // 
            textBoxResourceFolder.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxResourceFolder.Location = new System.Drawing.Point(13, 181);
            textBoxResourceFolder.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxResourceFolder.Name = "textBoxResourceFolder";
            textBoxResourceFolder.Size = new System.Drawing.Size(1318, 23);
            textBoxResourceFolder.TabIndex = 18;
            // 
            // buttonSelectResourceFolder
            // 
            buttonSelectResourceFolder.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonSelectResourceFolder.Location = new System.Drawing.Point(1341, 178);
            buttonSelectResourceFolder.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonSelectResourceFolder.Name = "buttonSelectResourceFolder";
            buttonSelectResourceFolder.Size = new System.Drawing.Size(35, 27);
            buttonSelectResourceFolder.TabIndex = 19;
            buttonSelectResourceFolder.Text = "...";
            buttonSelectResourceFolder.UseVisualStyleBackColor = true;
            buttonSelectResourceFolder.Click += buttonSelectResourceFolder_Click;
            // 
            // buttonChangeTrack
            // 
            buttonChangeTrack.Location = new System.Drawing.Point(13, 45);
            buttonChangeTrack.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonChangeTrack.Name = "buttonChangeTrack";
            buttonChangeTrack.Size = new System.Drawing.Size(134, 27);
            buttonChangeTrack.TabIndex = 6;
            buttonChangeTrack.Text = "Change Track";
            buttonChangeTrack.UseVisualStyleBackColor = true;
            buttonChangeTrack.Click += buttonChangeTrack_Click;
            // 
            // comboBoxTrackUnassign
            // 
            comboBoxTrackUnassign.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            comboBoxTrackUnassign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxTrackUnassign.FormattingEnabled = true;
            comboBoxTrackUnassign.Location = new System.Drawing.Point(1215, 48);
            comboBoxTrackUnassign.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            comboBoxTrackUnassign.Name = "comboBoxTrackUnassign";
            comboBoxTrackUnassign.Size = new System.Drawing.Size(116, 23);
            comboBoxTrackUnassign.TabIndex = 32;
            comboBoxTrackUnassign.SelectedIndexChanged += comboBoxTrackUnassign_SelectedIndexChanged;
            // 
            // comboBoxTrackAssign
            // 
            comboBoxTrackAssign.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            comboBoxTrackAssign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxTrackAssign.FormattingEnabled = true;
            comboBoxTrackAssign.Location = new System.Drawing.Point(1215, 15);
            comboBoxTrackAssign.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            comboBoxTrackAssign.Name = "comboBoxTrackAssign";
            comboBoxTrackAssign.Size = new System.Drawing.Size(116, 23);
            comboBoxTrackAssign.TabIndex = 29;
            comboBoxTrackAssign.SelectedIndexChanged += comboBoxTrackAssign_SelectedIndexChanged;
            // 
            // labelTrackUnassign
            // 
            labelTrackUnassign.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            labelTrackUnassign.AutoSize = true;
            labelTrackUnassign.Location = new System.Drawing.Point(1121, 51);
            labelTrackUnassign.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelTrackUnassign.Name = "labelTrackUnassign";
            labelTrackUnassign.Size = new System.Drawing.Size(88, 15);
            labelTrackUnassign.TabIndex = 31;
            labelTrackUnassign.Text = "Track unassign:";
            // 
            // labelTrackAssign
            // 
            labelTrackAssign.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            labelTrackAssign.AutoSize = true;
            labelTrackAssign.Location = new System.Drawing.Point(1135, 18);
            labelTrackAssign.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelTrackAssign.Name = "labelTrackAssign";
            labelTrackAssign.Size = new System.Drawing.Size(74, 15);
            labelTrackAssign.TabIndex = 28;
            labelTrackAssign.Text = "Track assign:";
            // 
            // buttonAssignTrack
            // 
            buttonAssignTrack.Location = new System.Drawing.Point(155, 45);
            buttonAssignTrack.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonAssignTrack.Name = "buttonAssignTrack";
            buttonAssignTrack.Size = new System.Drawing.Size(134, 27);
            buttonAssignTrack.TabIndex = 7;
            buttonAssignTrack.Text = "Assign Ver to Track";
            buttonAssignTrack.UseVisualStyleBackColor = true;
            buttonAssignTrack.Click += buttonAssignTrack_Click;
            // 
            // textBoxVersion
            // 
            textBoxVersion.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            textBoxVersion.Location = new System.Drawing.Point(1062, 15);
            textBoxVersion.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxVersion.Name = "textBoxVersion";
            textBoxVersion.Size = new System.Drawing.Size(56, 23);
            textBoxVersion.TabIndex = 27;
            // 
            // labelVersion
            // 
            labelVersion.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            labelVersion.AutoSize = true;
            labelVersion.Location = new System.Drawing.Point(970, 18);
            labelVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelVersion.Name = "labelVersion";
            labelVersion.Size = new System.Drawing.Size(84, 15);
            labelVersion.TabIndex = 26;
            labelVersion.Text = "Version assign:";
            // 
            // buttonUpdateChanges
            // 
            buttonUpdateChanges.Location = new System.Drawing.Point(297, 12);
            buttonUpdateChanges.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonUpdateChanges.Name = "buttonUpdateChanges";
            buttonUpdateChanges.Size = new System.Drawing.Size(134, 27);
            buttonUpdateChanges.TabIndex = 4;
            buttonUpdateChanges.Text = "Update Translations";
            buttonUpdateChanges.UseVisualStyleBackColor = true;
            buttonUpdateChanges.Click += buttonUpdateChanges_Click;
            // 
            // checkBoxUpdateName
            // 
            checkBoxUpdateName.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            checkBoxUpdateName.Location = new System.Drawing.Point(753, 45);
            checkBoxUpdateName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBoxUpdateName.Name = "checkBoxUpdateName";
            checkBoxUpdateName.RightToLeft = System.Windows.Forms.RightToLeft.No;
            checkBoxUpdateName.Size = new System.Drawing.Size(148, 28);
            checkBoxUpdateName.TabIndex = 30;
            checkBoxUpdateName.Text = "Update name";
            checkBoxUpdateName.UseVisualStyleBackColor = true;
            // 
            // buttonSetAppInfo
            // 
            buttonSetAppInfo.Location = new System.Drawing.Point(439, 45);
            buttonSetAppInfo.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonSetAppInfo.Name = "buttonSetAppInfo";
            buttonSetAppInfo.Size = new System.Drawing.Size(134, 27);
            buttonSetAppInfo.TabIndex = 9;
            buttonSetAppInfo.Text = "Set App Info";
            buttonSetAppInfo.UseVisualStyleBackColor = true;
            buttonSetAppInfo.Click += buttonSetAppInfo_Click;
            // 
            // labelSerialFileName
            // 
            labelSerialFileName.AutoSize = true;
            labelSerialFileName.Location = new System.Drawing.Point(13, 207);
            labelSerialFileName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelSerialFileName.Name = "labelSerialFileName";
            labelSerialFileName.Size = new System.Drawing.Size(90, 15);
            labelSerialFileName.TabIndex = 20;
            labelSerialFileName.Text = "Serial file name:";
            // 
            // textBoxSerialFileName
            // 
            textBoxSerialFileName.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxSerialFileName.Location = new System.Drawing.Point(13, 225);
            textBoxSerialFileName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxSerialFileName.Name = "textBoxSerialFileName";
            textBoxSerialFileName.Size = new System.Drawing.Size(1318, 23);
            textBoxSerialFileName.TabIndex = 21;
            // 
            // buttonSelectSerialFile
            // 
            buttonSelectSerialFile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonSelectSerialFile.Location = new System.Drawing.Point(1341, 222);
            buttonSelectSerialFile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonSelectSerialFile.Name = "buttonSelectSerialFile";
            buttonSelectSerialFile.Size = new System.Drawing.Size(35, 27);
            buttonSelectSerialFile.TabIndex = 22;
            buttonSelectSerialFile.Text = "...";
            buttonSelectSerialFile.UseVisualStyleBackColor = true;
            buttonSelectSerialFile.Click += buttonSelectSerialFile_Click;
            // 
            // openFileDialogSerial
            // 
            openFileDialogSerial.DefaultExt = "*.num";
            openFileDialogSerial.Filter = "Serial Number Files|*.num";
            // 
            // buttonUploadSerials
            // 
            buttonUploadSerials.Location = new System.Drawing.Point(581, 45);
            buttonUploadSerials.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonUploadSerials.Name = "buttonUploadSerials";
            buttonUploadSerials.Size = new System.Drawing.Size(134, 27);
            buttonUploadSerials.TabIndex = 10;
            buttonUploadSerials.Text = "Upload Serials";
            buttonUploadSerials.UseVisualStyleBackColor = true;
            buttonUploadSerials.Click += buttonUploadSerials_Click;
            // 
            // comboBoxSerialOem
            // 
            comboBoxSerialOem.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            comboBoxSerialOem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxSerialOem.FormattingEnabled = true;
            comboBoxSerialOem.Location = new System.Drawing.Point(984, 48);
            comboBoxSerialOem.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            comboBoxSerialOem.Name = "comboBoxSerialOem";
            comboBoxSerialOem.Size = new System.Drawing.Size(134, 23);
            comboBoxSerialOem.TabIndex = 25;
            // 
            // labelSerialOem
            // 
            labelSerialOem.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            labelSerialOem.AutoSize = true;
            labelSerialOem.Location = new System.Drawing.Point(909, 51);
            labelSerialOem.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelSerialOem.Name = "labelSerialOem";
            labelSerialOem.Size = new System.Drawing.Size(67, 15);
            labelSerialOem.TabIndex = 24;
            labelSerialOem.Text = "Serial OEM:";
            // 
            // buttonTriggerReview
            // 
            buttonTriggerReview.Location = new System.Drawing.Point(439, 12);
            buttonTriggerReview.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonTriggerReview.Name = "buttonTriggerReview";
            buttonTriggerReview.Size = new System.Drawing.Size(134, 27);
            buttonTriggerReview.TabIndex = 5;
            buttonTriggerReview.Text = "Trigger Review";
            buttonTriggerReview.UseVisualStyleBackColor = true;
            buttonTriggerReview.Click += buttonTriggerReview_Click;
            // 
            // textBoxUserFraction
            // 
            textBoxUserFraction.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            textBoxUserFraction.Location = new System.Drawing.Point(906, 15);
            textBoxUserFraction.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxUserFraction.Name = "textBoxUserFraction";
            textBoxUserFraction.Size = new System.Drawing.Size(56, 23);
            textBoxUserFraction.TabIndex = 33;
            // 
            // labelUserFraction
            // 
            labelUserFraction.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            labelUserFraction.AutoSize = true;
            labelUserFraction.Location = new System.Drawing.Point(790, 18);
            labelUserFraction.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelUserFraction.Name = "labelUserFraction";
            labelUserFraction.Size = new System.Drawing.Size(100, 15);
            labelUserFraction.TabIndex = 34;
            labelUserFraction.Text = "User Fraction [%]:";
            // 
            // FormMain
            // 
            AcceptButton = buttonClose;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonAbort;
            ClientSize = new System.Drawing.Size(1389, 763);
            Controls.Add(labelUserFraction);
            Controls.Add(textBoxUserFraction);
            Controls.Add(buttonTriggerReview);
            Controls.Add(labelSerialOem);
            Controls.Add(comboBoxSerialOem);
            Controls.Add(buttonUploadSerials);
            Controls.Add(buttonSelectSerialFile);
            Controls.Add(textBoxSerialFileName);
            Controls.Add(labelSerialFileName);
            Controls.Add(buttonSetAppInfo);
            Controls.Add(checkBoxUpdateName);
            Controls.Add(buttonUpdateChanges);
            Controls.Add(labelVersion);
            Controls.Add(textBoxVersion);
            Controls.Add(buttonAssignTrack);
            Controls.Add(labelTrackAssign);
            Controls.Add(labelTrackUnassign);
            Controls.Add(comboBoxTrackAssign);
            Controls.Add(comboBoxTrackUnassign);
            Controls.Add(buttonChangeTrack);
            Controls.Add(buttonSelectResourceFolder);
            Controls.Add(textBoxResourceFolder);
            Controls.Add(labelRescourceFolder);
            Controls.Add(buttonSelectObbFile);
            Controls.Add(textBoxObbFile);
            Controls.Add(labelObbFile);
            Controls.Add(textBoxBundleFile);
            Controls.Add(labelApkFile);
            Controls.Add(buttonSelectBundleFile);
            Controls.Add(buttonListTracks);
            Controls.Add(buttonUploadBundle);
            Controls.Add(buttonAbort);
            Controls.Add(buttonClose);
            Controls.Add(buttonListBundles);
            Controls.Add(textBoxStatus);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimumSize = new System.Drawing.Size(1400, 802);
            Name = "FormMain";
            Text = "Bundle/Apk Uploader";
            FormClosing += FormMain_FormClosing;
            FormClosed += FormMain_FormClosed;
            Load += FormMain_Load;
            Shown += FormMain_Shown;
            ResumeLayout(false);
            PerformLayout();
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
        private System.Windows.Forms.Button buttonTriggerReview;
        private System.Windows.Forms.TextBox textBoxUserFraction;
        private System.Windows.Forms.Label labelUserFraction;
    }
}


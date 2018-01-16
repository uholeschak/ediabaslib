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
            this.buttonListApks = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonAbort = new System.Windows.Forms.Button();
            this.buttonUploadApk = new System.Windows.Forms.Button();
            this.openFileDialogApk = new System.Windows.Forms.OpenFileDialog();
            this.buttonListTracks = new System.Windows.Forms.Button();
            this.checkBoxAlpha = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // textBoxStatus
            // 
            this.textBoxStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxStatus.Location = new System.Drawing.Point(12, 41);
            this.textBoxStatus.Multiline = true;
            this.textBoxStatus.Name = "textBoxStatus";
            this.textBoxStatus.ReadOnly = true;
            this.textBoxStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxStatus.Size = new System.Drawing.Size(500, 284);
            this.textBoxStatus.TabIndex = 0;
            this.textBoxStatus.TabStop = false;
            // 
            // buttonListApks
            // 
            this.buttonListApks.Location = new System.Drawing.Point(12, 12);
            this.buttonListApks.Name = "buttonListApks";
            this.buttonListApks.Size = new System.Drawing.Size(115, 23);
            this.buttonListApks.TabIndex = 3;
            this.buttonListApks.Text = "List Apks";
            this.buttonListApks.UseVisualStyleBackColor = true;
            this.buttonListApks.Click += new System.EventHandler(this.buttonListApks_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Location = new System.Drawing.Point(437, 331);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 1;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonAbort
            // 
            this.buttonAbort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAbort.Location = new System.Drawing.Point(356, 331);
            this.buttonAbort.Name = "buttonAbort";
            this.buttonAbort.Size = new System.Drawing.Size(75, 23);
            this.buttonAbort.TabIndex = 2;
            this.buttonAbort.Text = "Abort";
            this.buttonAbort.UseVisualStyleBackColor = true;
            this.buttonAbort.Click += new System.EventHandler(this.buttonAbort_Click);
            // 
            // buttonUploadApk
            // 
            this.buttonUploadApk.Location = new System.Drawing.Point(254, 12);
            this.buttonUploadApk.Name = "buttonUploadApk";
            this.buttonUploadApk.Size = new System.Drawing.Size(115, 23);
            this.buttonUploadApk.TabIndex = 5;
            this.buttonUploadApk.Text = "Upload Apk";
            this.buttonUploadApk.UseVisualStyleBackColor = true;
            this.buttonUploadApk.Click += new System.EventHandler(this.buttonUploadApk_Click);
            // 
            // openFileDialogApk
            // 
            this.openFileDialogApk.Filter = "Apk Files|*.apk";
            // 
            // buttonListTracks
            // 
            this.buttonListTracks.Location = new System.Drawing.Point(133, 12);
            this.buttonListTracks.Name = "buttonListTracks";
            this.buttonListTracks.Size = new System.Drawing.Size(115, 23);
            this.buttonListTracks.TabIndex = 4;
            this.buttonListTracks.Text = "List Tracks";
            this.buttonListTracks.UseVisualStyleBackColor = true;
            this.buttonListTracks.Click += new System.EventHandler(this.buttonListTracks_Click);
            // 
            // checkBoxAlpha
            // 
            this.checkBoxAlpha.AutoSize = true;
            this.checkBoxAlpha.Location = new System.Drawing.Point(375, 16);
            this.checkBoxAlpha.Name = "checkBoxAlpha";
            this.checkBoxAlpha.Size = new System.Drawing.Size(53, 17);
            this.checkBoxAlpha.TabIndex = 6;
            this.checkBoxAlpha.Text = "Alpha";
            this.checkBoxAlpha.UseVisualStyleBackColor = true;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(524, 366);
            this.Controls.Add(this.checkBoxAlpha);
            this.Controls.Add(this.buttonListTracks);
            this.Controls.Add(this.buttonUploadApk);
            this.Controls.Add(this.buttonAbort);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonListApks);
            this.Controls.Add(this.textBoxStatus);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.Text = "Apk Uploader";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMain_FormClosed);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.Shown += new System.EventHandler(this.FormMain_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxStatus;
        private System.Windows.Forms.Button buttonListApks;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonAbort;
        private System.Windows.Forms.Button buttonUploadApk;
        private System.Windows.Forms.OpenFileDialog openFileDialogApk;
        private System.Windows.Forms.Button buttonListTracks;
        private System.Windows.Forms.CheckBox checkBoxAlpha;
    }
}


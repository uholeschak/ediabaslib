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
            buttonExecute = new System.Windows.Forms.Button();
            richTextBoxStatus = new System.Windows.Forms.RichTextBox();
            buttonSelectJsonResponseFolder = new System.Windows.Forms.Button();
            textBoxJsonResponseFolder = new System.Windows.Forms.TextBox();
            buttonSelectS29Folder = new System.Windows.Forms.Button();
            textBoxS29Folder = new System.Windows.Forms.TextBox();
            buttonSelectIstaKeyFile = new System.Windows.Forms.Button();
            textBoxIstaKeyFile = new System.Windows.Forms.TextBox();
            openIstaKeyFileDialog = new System.Windows.Forms.OpenFileDialog();
            checkBoxForceCreate = new System.Windows.Forms.CheckBox();
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
            buttonClose.Location = new System.Drawing.Point(610, 374);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new System.Drawing.Size(75, 23);
            buttonClose.TabIndex = 15;
            buttonClose.Text = "Close";
            buttonClose.UseVisualStyleBackColor = true;
            buttonClose.Click += buttonClose_Click;
            // 
            // buttonSelectJsonRequestFolder
            // 
            buttonSelectJsonRequestFolder.Location = new System.Drawing.Point(12, 99);
            buttonSelectJsonRequestFolder.Name = "buttonSelectJsonRequestFolder";
            buttonSelectJsonRequestFolder.Size = new System.Drawing.Size(179, 23);
            buttonSelectJsonRequestFolder.TabIndex = 6;
            buttonSelectJsonRequestFolder.Text = "Select JSON Request Dir";
            buttonSelectJsonRequestFolder.UseVisualStyleBackColor = true;
            buttonSelectJsonRequestFolder.Click += buttonSelectJsonRequestFolder_Click;
            // 
            // textBoxJsonRequestFolder
            // 
            textBoxJsonRequestFolder.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxJsonRequestFolder.Location = new System.Drawing.Point(197, 99);
            textBoxJsonRequestFolder.Name = "textBoxJsonRequestFolder";
            textBoxJsonRequestFolder.ReadOnly = true;
            textBoxJsonRequestFolder.Size = new System.Drawing.Size(488, 23);
            textBoxJsonRequestFolder.TabIndex = 7;
            // 
            // buttonSelectCertOutputFolder
            // 
            buttonSelectCertOutputFolder.Location = new System.Drawing.Point(12, 157);
            buttonSelectCertOutputFolder.Name = "buttonSelectCertOutputFolder";
            buttonSelectCertOutputFolder.Size = new System.Drawing.Size(179, 23);
            buttonSelectCertOutputFolder.TabIndex = 10;
            buttonSelectCertOutputFolder.Text = "Select Cert Output Dir";
            buttonSelectCertOutputFolder.UseVisualStyleBackColor = true;
            buttonSelectCertOutputFolder.Click += buttonSelectCertOutputFolder_Click;
            // 
            // textBoxCertOutputFolder
            // 
            textBoxCertOutputFolder.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxCertOutputFolder.Location = new System.Drawing.Point(197, 157);
            textBoxCertOutputFolder.Name = "textBoxCertOutputFolder";
            textBoxCertOutputFolder.ReadOnly = true;
            textBoxCertOutputFolder.Size = new System.Drawing.Size(488, 23);
            textBoxCertOutputFolder.TabIndex = 11;
            // 
            // buttonExecute
            // 
            buttonExecute.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonExecute.Location = new System.Drawing.Point(529, 374);
            buttonExecute.Name = "buttonExecute";
            buttonExecute.Size = new System.Drawing.Size(75, 23);
            buttonExecute.TabIndex = 14;
            buttonExecute.Text = "Execute";
            buttonExecute.UseVisualStyleBackColor = true;
            buttonExecute.Click += buttonExecute_Click;
            // 
            // richTextBoxStatus
            // 
            richTextBoxStatus.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            richTextBoxStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            richTextBoxStatus.Location = new System.Drawing.Point(12, 186);
            richTextBoxStatus.Name = "richTextBoxStatus";
            richTextBoxStatus.ReadOnly = true;
            richTextBoxStatus.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            richTextBoxStatus.Size = new System.Drawing.Size(673, 182);
            richTextBoxStatus.TabIndex = 12;
            richTextBoxStatus.Text = "";
            // 
            // buttonSelectJsonResponseFolder
            // 
            buttonSelectJsonResponseFolder.Location = new System.Drawing.Point(12, 128);
            buttonSelectJsonResponseFolder.Name = "buttonSelectJsonResponseFolder";
            buttonSelectJsonResponseFolder.Size = new System.Drawing.Size(179, 23);
            buttonSelectJsonResponseFolder.TabIndex = 8;
            buttonSelectJsonResponseFolder.Text = "Select JSON Response Dir";
            buttonSelectJsonResponseFolder.UseVisualStyleBackColor = true;
            buttonSelectJsonResponseFolder.Click += buttonSelectJsonResponseFolder_Click;
            // 
            // textBoxJsonResponseFolder
            // 
            textBoxJsonResponseFolder.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxJsonResponseFolder.Location = new System.Drawing.Point(197, 128);
            textBoxJsonResponseFolder.Name = "textBoxJsonResponseFolder";
            textBoxJsonResponseFolder.ReadOnly = true;
            textBoxJsonResponseFolder.Size = new System.Drawing.Size(488, 23);
            textBoxJsonResponseFolder.TabIndex = 9;
            // 
            // buttonSelectS29Folder
            // 
            buttonSelectS29Folder.Location = new System.Drawing.Point(12, 70);
            buttonSelectS29Folder.Name = "buttonSelectS29Folder";
            buttonSelectS29Folder.Size = new System.Drawing.Size(179, 23);
            buttonSelectS29Folder.TabIndex = 4;
            buttonSelectS29Folder.Text = "Select S29 Dir";
            buttonSelectS29Folder.UseVisualStyleBackColor = true;
            buttonSelectS29Folder.Click += buttonSelectS29Folder_Click;
            // 
            // textBoxS29Folder
            // 
            textBoxS29Folder.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxS29Folder.Location = new System.Drawing.Point(197, 70);
            textBoxS29Folder.Name = "textBoxS29Folder";
            textBoxS29Folder.ReadOnly = true;
            textBoxS29Folder.Size = new System.Drawing.Size(488, 23);
            textBoxS29Folder.TabIndex = 5;
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
            checkBoxForceCreate.AutoSize = true;
            checkBoxForceCreate.Location = new System.Drawing.Point(12, 374);
            checkBoxForceCreate.Name = "checkBoxForceCreate";
            checkBoxForceCreate.Size = new System.Drawing.Size(113, 19);
            checkBoxForceCreate.TabIndex = 13;
            checkBoxForceCreate.Text = "Force create cert";
            checkBoxForceCreate.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            AcceptButton = buttonExecute;
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            CancelButton = buttonClose;
            ClientSize = new System.Drawing.Size(697, 409);
            Controls.Add(checkBoxForceCreate);
            Controls.Add(textBoxIstaKeyFile);
            Controls.Add(buttonSelectIstaKeyFile);
            Controls.Add(textBoxS29Folder);
            Controls.Add(buttonSelectS29Folder);
            Controls.Add(textBoxJsonResponseFolder);
            Controls.Add(buttonSelectJsonResponseFolder);
            Controls.Add(richTextBoxStatus);
            Controls.Add(buttonExecute);
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
        private System.Windows.Forms.Button buttonExecute;
        private System.Windows.Forms.RichTextBox richTextBoxStatus;
        private System.Windows.Forms.Button buttonSelectJsonResponseFolder;
        private System.Windows.Forms.TextBox textBoxJsonResponseFolder;
        private System.Windows.Forms.Button buttonSelectS29Folder;
        private System.Windows.Forms.TextBox textBoxS29Folder;
        private System.Windows.Forms.Button buttonSelectIstaKeyFile;
        private System.Windows.Forms.TextBox textBoxIstaKeyFile;
        private System.Windows.Forms.OpenFileDialog openIstaKeyFileDialog;
        private System.Windows.Forms.CheckBox checkBoxForceCreate;
    }
}

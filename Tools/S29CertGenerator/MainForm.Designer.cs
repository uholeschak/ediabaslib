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
            SuspendLayout();
            // 
            // folderBrowserDialog
            // 
            folderBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
            folderBrowserDialog.ShowNewFolderButton = false;
            // 
            // openCertFileDialog
            // 
            openCertFileDialog.DefaultExt = "key";
            openCertFileDialog.Filter = "Cert|*.key|All files|*.*";
            openCertFileDialog.Title = "Select server cert";
            // 
            // buttonSelectCaKeyFile
            // 
            buttonSelectCaKeyFile.Location = new System.Drawing.Point(12, 12);
            buttonSelectCaKeyFile.Name = "buttonSelectCaKeyFile";
            buttonSelectCaKeyFile.Size = new System.Drawing.Size(147, 23);
            buttonSelectCaKeyFile.TabIndex = 0;
            buttonSelectCaKeyFile.Text = "Select CA Key";
            buttonSelectCaKeyFile.UseVisualStyleBackColor = true;
            buttonSelectCaKeyFile.Click += buttonSelectCaKeyFile_Click;
            // 
            // textBoxCaCeyFile
            // 
            textBoxCaCeyFile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxCaCeyFile.Location = new System.Drawing.Point(165, 12);
            textBoxCaCeyFile.Name = "textBoxCaCeyFile";
            textBoxCaCeyFile.ReadOnly = true;
            textBoxCaCeyFile.Size = new System.Drawing.Size(623, 23);
            textBoxCaCeyFile.TabIndex = 1;
            // 
            // buttonClose
            // 
            buttonClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonClose.Location = new System.Drawing.Point(713, 415);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new System.Drawing.Size(75, 23);
            buttonClose.TabIndex = 2;
            buttonClose.Text = "Close";
            buttonClose.UseVisualStyleBackColor = true;
            buttonClose.Click += buttonClose_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonClose;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(buttonClose);
            Controls.Add(textBoxCaCeyFile);
            Controls.Add(buttonSelectCaKeyFile);
            Name = "MainForm";
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
    }
}

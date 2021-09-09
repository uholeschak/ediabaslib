
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
            this.SuspendLayout();
            // 
            // textBoxIstaFolder
            // 
            this.textBoxIstaFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxIstaFolder.Location = new System.Drawing.Point(12, 12);
            this.textBoxIstaFolder.Name = "textBoxIstaFolder";
            this.textBoxIstaFolder.Size = new System.Drawing.Size(754, 20);
            this.textBoxIstaFolder.TabIndex = 0;
            // 
            // buttonClose
            // 
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonClose.Location = new System.Drawing.Point(723, 415);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 1;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonAbort
            // 
            this.buttonAbort.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonAbort.Location = new System.Drawing.Point(642, 415);
            this.buttonAbort.Name = "buttonAbort";
            this.buttonAbort.Size = new System.Drawing.Size(75, 23);
            this.buttonAbort.TabIndex = 2;
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
            this.buttonIstaFolder.TabIndex = 3;
            this.buttonIstaFolder.Text = "...";
            this.buttonIstaFolder.UseVisualStyleBackColor = true;
            this.buttonIstaFolder.Click += new System.EventHandler(this.buttonIstaFolder_Click);
            // 
            // buttonStartHost
            // 
            this.buttonStartHost.Location = new System.Drawing.Point(12, 38);
            this.buttonStartHost.Name = "buttonStartHost";
            this.buttonStartHost.Size = new System.Drawing.Size(75, 23);
            this.buttonStartHost.TabIndex = 4;
            this.buttonStartHost.Text = "Start Host";
            this.buttonStartHost.UseVisualStyleBackColor = true;
            this.buttonStartHost.Click += new System.EventHandler(this.buttonStartHost_Click);
            // 
            // buttonStopHost
            // 
            this.buttonStopHost.Location = new System.Drawing.Point(93, 38);
            this.buttonStopHost.Name = "buttonStopHost";
            this.buttonStopHost.Size = new System.Drawing.Size(75, 23);
            this.buttonStopHost.TabIndex = 5;
            this.buttonStopHost.Text = "Stop Host";
            this.buttonStopHost.UseVisualStyleBackColor = true;
            this.buttonStopHost.Click += new System.EventHandler(this.buttonStopHost_Click);
            // 
            // timerUpdate
            // 
            this.timerUpdate.Tick += new System.EventHandler(this.timerUpdate_Tick);
            // 
            // FormMain
            // 
            this.AcceptButton = this.buttonClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonAbort;
            this.ClientSize = new System.Drawing.Size(810, 450);
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
    }
}


namespace OfflineKeyGenerator
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.buttonClose = new System.Windows.Forms.Button();
            this.labelObbKey = new System.Windows.Forms.Label();
            this.textBoxObbKey = new System.Windows.Forms.TextBox();
            this.labelAppId = new System.Windows.Forms.Label();
            this.textBoxAppId = new System.Windows.Forms.TextBox();
            this.labelOfflineKey = new System.Windows.Forms.Label();
            this.textBoxOfflineKey = new System.Windows.Forms.TextBox();
            this.buttonCalculate = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Location = new System.Drawing.Point(325, 129);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 1;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // labelObbKey
            // 
            this.labelObbKey.AutoSize = true;
            this.labelObbKey.Location = new System.Drawing.Point(9, 9);
            this.labelObbKey.Name = "labelObbKey";
            this.labelObbKey.Size = new System.Drawing.Size(53, 13);
            this.labelObbKey.TabIndex = 1;
            this.labelObbKey.Text = "OBB Key:";
            // 
            // textBoxObbKey
            // 
            this.textBoxObbKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxObbKey.Location = new System.Drawing.Point(12, 25);
            this.textBoxObbKey.Name = "textBoxObbKey";
            this.textBoxObbKey.Size = new System.Drawing.Size(388, 20);
            this.textBoxObbKey.TabIndex = 2;
            // 
            // labelAppId
            // 
            this.labelAppId.AutoSize = true;
            this.labelAppId.Location = new System.Drawing.Point(9, 48);
            this.labelAppId.Name = "labelAppId";
            this.labelAppId.Size = new System.Drawing.Size(43, 13);
            this.labelAppId.TabIndex = 3;
            this.labelAppId.Text = "App ID:";
            // 
            // textBoxAppId
            // 
            this.textBoxAppId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxAppId.Location = new System.Drawing.Point(12, 64);
            this.textBoxAppId.Name = "textBoxAppId";
            this.textBoxAppId.Size = new System.Drawing.Size(388, 20);
            this.textBoxAppId.TabIndex = 4;
            // 
            // labelOfflineKey
            // 
            this.labelOfflineKey.AutoSize = true;
            this.labelOfflineKey.Location = new System.Drawing.Point(12, 87);
            this.labelOfflineKey.Name = "labelOfflineKey";
            this.labelOfflineKey.Size = new System.Drawing.Size(61, 13);
            this.labelOfflineKey.TabIndex = 5;
            this.labelOfflineKey.Text = "Offline Key:";
            // 
            // textBoxOfflineKey
            // 
            this.textBoxOfflineKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOfflineKey.Location = new System.Drawing.Point(12, 103);
            this.textBoxOfflineKey.Name = "textBoxOfflineKey";
            this.textBoxOfflineKey.ReadOnly = true;
            this.textBoxOfflineKey.Size = new System.Drawing.Size(388, 20);
            this.textBoxOfflineKey.TabIndex = 6;
            // 
            // buttonCalculate
            // 
            this.buttonCalculate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCalculate.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCalculate.Location = new System.Drawing.Point(244, 129);
            this.buttonCalculate.Name = "buttonCalculate";
            this.buttonCalculate.Size = new System.Drawing.Size(75, 23);
            this.buttonCalculate.TabIndex = 0;
            this.buttonCalculate.Text = "Calculate";
            this.buttonCalculate.UseVisualStyleBackColor = true;
            this.buttonCalculate.Click += new System.EventHandler(this.buttonCalculate_Click);
            // 
            // MainForm
            // 
            this.AcceptButton = this.buttonCalculate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(412, 164);
            this.Controls.Add(this.buttonCalculate);
            this.Controls.Add(this.textBoxOfflineKey);
            this.Controls.Add(this.labelOfflineKey);
            this.Controls.Add(this.textBoxAppId);
            this.Controls.Add(this.labelAppId);
            this.Controls.Add(this.textBoxObbKey);
            this.Controls.Add(this.labelObbKey);
            this.Controls.Add(this.buttonClose);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Offline Key Generator";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Label labelObbKey;
        private System.Windows.Forms.TextBox textBoxObbKey;
        private System.Windows.Forms.Label labelAppId;
        private System.Windows.Forms.TextBox textBoxAppId;
        private System.Windows.Forms.Label labelOfflineKey;
        private System.Windows.Forms.TextBox textBoxOfflineKey;
        private System.Windows.Forms.Button buttonCalculate;
    }
}


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
            buttonClose = new System.Windows.Forms.Button();
            labelObbKey = new System.Windows.Forms.Label();
            textBoxObbKey = new System.Windows.Forms.TextBox();
            labelAppId = new System.Windows.Forms.Label();
            textBoxAppId = new System.Windows.Forms.TextBox();
            labelOfflineKey = new System.Windows.Forms.Label();
            textBoxOfflineKey = new System.Windows.Forms.TextBox();
            buttonCalculate = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // buttonClose
            // 
            buttonClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonClose.Location = new System.Drawing.Point(380, 147);
            buttonClose.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new System.Drawing.Size(88, 27);
            buttonClose.TabIndex = 1;
            buttonClose.Text = "Close";
            buttonClose.UseVisualStyleBackColor = true;
            buttonClose.Click += buttonClose_Click;
            // 
            // labelObbKey
            // 
            labelObbKey.AutoSize = true;
            labelObbKey.Location = new System.Drawing.Point(10, 9);
            labelObbKey.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelObbKey.Name = "labelObbKey";
            labelObbKey.Size = new System.Drawing.Size(55, 15);
            labelObbKey.TabIndex = 1;
            labelObbKey.Text = "OBB Key:";
            // 
            // textBoxObbKey
            // 
            textBoxObbKey.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxObbKey.Location = new System.Drawing.Point(10, 27);
            textBoxObbKey.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxObbKey.Name = "textBoxObbKey";
            textBoxObbKey.Size = new System.Drawing.Size(458, 23);
            textBoxObbKey.TabIndex = 2;
            // 
            // labelAppId
            // 
            labelAppId.AutoSize = true;
            labelAppId.Location = new System.Drawing.Point(10, 53);
            labelAppId.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelAppId.Name = "labelAppId";
            labelAppId.Size = new System.Drawing.Size(46, 15);
            labelAppId.TabIndex = 3;
            labelAppId.Text = "App ID:";
            // 
            // textBoxAppId
            // 
            textBoxAppId.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxAppId.Location = new System.Drawing.Point(10, 71);
            textBoxAppId.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxAppId.Name = "textBoxAppId";
            textBoxAppId.Size = new System.Drawing.Size(458, 23);
            textBoxAppId.TabIndex = 4;
            // 
            // labelOfflineKey
            // 
            labelOfflineKey.AutoSize = true;
            labelOfflineKey.Location = new System.Drawing.Point(10, 97);
            labelOfflineKey.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelOfflineKey.Name = "labelOfflineKey";
            labelOfflineKey.Size = new System.Drawing.Size(68, 15);
            labelOfflineKey.TabIndex = 5;
            labelOfflineKey.Text = "Offline Key:";
            // 
            // textBoxOfflineKey
            // 
            textBoxOfflineKey.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxOfflineKey.Location = new System.Drawing.Point(10, 115);
            textBoxOfflineKey.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxOfflineKey.Name = "textBoxOfflineKey";
            textBoxOfflineKey.ReadOnly = true;
            textBoxOfflineKey.Size = new System.Drawing.Size(458, 23);
            textBoxOfflineKey.TabIndex = 6;
            // 
            // buttonCalculate
            // 
            buttonCalculate.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonCalculate.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCalculate.Location = new System.Drawing.Point(284, 147);
            buttonCalculate.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonCalculate.Name = "buttonCalculate";
            buttonCalculate.Size = new System.Drawing.Size(88, 27);
            buttonCalculate.TabIndex = 0;
            buttonCalculate.Text = "Calculate";
            buttonCalculate.UseVisualStyleBackColor = true;
            buttonCalculate.Click += buttonCalculate_Click;
            // 
            // MainForm
            // 
            AcceptButton = buttonCalculate;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonClose;
            ClientSize = new System.Drawing.Size(481, 186);
            Controls.Add(buttonCalculate);
            Controls.Add(textBoxOfflineKey);
            Controls.Add(labelOfflineKey);
            Controls.Add(textBoxAppId);
            Controls.Add(labelAppId);
            Controls.Add(textBoxObbKey);
            Controls.Add(labelObbKey);
            Controls.Add(buttonClose);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            Name = "MainForm";
            Text = "Offline Key Generator";
            FormClosed += MainForm_FormClosed;
            Load += MainForm_Load;
            ResumeLayout(false);
            PerformLayout();
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


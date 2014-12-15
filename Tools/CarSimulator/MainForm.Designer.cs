namespace CarSimulator
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
            this.components = new System.ComponentModel.Container();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.listPorts = new System.Windows.Forms.ListBox();
            this.timerUpdate = new System.Windows.Forms.Timer(this.components);
            this.checkBoxMoving = new System.Windows.Forms.CheckBox();
            this.checkBoxVariableValues = new System.Windows.Forms.CheckBox();
            this.radioButtonBmwFast = new System.Windows.Forms.RadioButton();
            this.groupBoxConcepts = new System.Windows.Forms.GroupBox();
            this.radioButtonIso9141 = new System.Windows.Forms.RadioButton();
            this.radioButtonDs2 = new System.Windows.Forms.RadioButton();
            this.radioButtonKwp2000S = new System.Windows.Forms.RadioButton();
            this.listBoxResponseFiles = new System.Windows.Forms.ListBox();
            this.checkBoxIgnitionOk = new System.Windows.Forms.CheckBox();
            this.checkBoxAdsAdapter = new System.Windows.Forms.CheckBox();
            this.groupBoxConcepts.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(96, 12);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(75, 23);
            this.buttonConnect.TabIndex = 0;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // listPorts
            // 
            this.listPorts.FormattingEnabled = true;
            this.listPorts.Location = new System.Drawing.Point(12, 12);
            this.listPorts.Name = "listPorts";
            this.listPorts.Size = new System.Drawing.Size(78, 56);
            this.listPorts.TabIndex = 1;
            // 
            // timerUpdate
            // 
            this.timerUpdate.Interval = 500;
            this.timerUpdate.Tick += new System.EventHandler(this.timerUpdate_Tick);
            // 
            // checkBoxMoving
            // 
            this.checkBoxMoving.AutoSize = true;
            this.checkBoxMoving.Location = new System.Drawing.Point(97, 42);
            this.checkBoxMoving.Name = "checkBoxMoving";
            this.checkBoxMoving.Size = new System.Drawing.Size(59, 17);
            this.checkBoxMoving.TabIndex = 2;
            this.checkBoxMoving.Text = "Driving";
            this.checkBoxMoving.UseVisualStyleBackColor = true;
            this.checkBoxMoving.CheckedChanged += new System.EventHandler(this.checkBoxMoving_CheckedChanged);
            // 
            // checkBoxVariableValues
            // 
            this.checkBoxVariableValues.AutoSize = true;
            this.checkBoxVariableValues.Location = new System.Drawing.Point(97, 65);
            this.checkBoxVariableValues.Name = "checkBoxVariableValues";
            this.checkBoxVariableValues.Size = new System.Drawing.Size(98, 17);
            this.checkBoxVariableValues.TabIndex = 3;
            this.checkBoxVariableValues.Text = "Variable values";
            this.checkBoxVariableValues.UseVisualStyleBackColor = true;
            // 
            // radioButtonBmwFast
            // 
            this.radioButtonBmwFast.AutoSize = true;
            this.radioButtonBmwFast.Checked = true;
            this.radioButtonBmwFast.Location = new System.Drawing.Point(6, 19);
            this.radioButtonBmwFast.Name = "radioButtonBmwFast";
            this.radioButtonBmwFast.Size = new System.Drawing.Size(75, 17);
            this.radioButtonBmwFast.TabIndex = 5;
            this.radioButtonBmwFast.TabStop = true;
            this.radioButtonBmwFast.Text = "BMW Fast";
            this.radioButtonBmwFast.UseVisualStyleBackColor = true;
            // 
            // groupBoxConcepts
            // 
            this.groupBoxConcepts.Controls.Add(this.radioButtonIso9141);
            this.groupBoxConcepts.Controls.Add(this.radioButtonDs2);
            this.groupBoxConcepts.Controls.Add(this.radioButtonKwp2000S);
            this.groupBoxConcepts.Controls.Add(this.radioButtonBmwFast);
            this.groupBoxConcepts.Location = new System.Drawing.Point(224, 88);
            this.groupBoxConcepts.Name = "groupBoxConcepts";
            this.groupBoxConcepts.Size = new System.Drawing.Size(207, 121);
            this.groupBoxConcepts.TabIndex = 6;
            this.groupBoxConcepts.TabStop = false;
            this.groupBoxConcepts.Text = "Concepts";
            // 
            // radioButtonIso9141
            // 
            this.radioButtonIso9141.AutoSize = true;
            this.radioButtonIso9141.Location = new System.Drawing.Point(6, 88);
            this.radioButtonIso9141.Name = "radioButtonIso9141";
            this.radioButtonIso9141.Size = new System.Drawing.Size(70, 17);
            this.radioButtonIso9141.TabIndex = 8;
            this.radioButtonIso9141.TabStop = true;
            this.radioButtonIso9141.Text = "ISO 9141";
            this.radioButtonIso9141.UseVisualStyleBackColor = true;
            // 
            // radioButtonDs2
            // 
            this.radioButtonDs2.AutoSize = true;
            this.radioButtonDs2.Location = new System.Drawing.Point(6, 65);
            this.radioButtonDs2.Name = "radioButtonDs2";
            this.radioButtonDs2.Size = new System.Drawing.Size(46, 17);
            this.radioButtonDs2.TabIndex = 7;
            this.radioButtonDs2.TabStop = true;
            this.radioButtonDs2.Text = "DS2";
            this.radioButtonDs2.UseVisualStyleBackColor = true;
            // 
            // radioButtonKwp2000S
            // 
            this.radioButtonKwp2000S.AutoSize = true;
            this.radioButtonKwp2000S.Location = new System.Drawing.Point(6, 42);
            this.radioButtonKwp2000S.Name = "radioButtonKwp2000S";
            this.radioButtonKwp2000S.Size = new System.Drawing.Size(78, 17);
            this.radioButtonKwp2000S.TabIndex = 6;
            this.radioButtonKwp2000S.TabStop = true;
            this.radioButtonKwp2000S.Text = "KWP2000*";
            this.radioButtonKwp2000S.UseVisualStyleBackColor = true;
            // 
            // listBoxResponseFiles
            // 
            this.listBoxResponseFiles.FormattingEnabled = true;
            this.listBoxResponseFiles.Location = new System.Drawing.Point(12, 88);
            this.listBoxResponseFiles.Name = "listBoxResponseFiles";
            this.listBoxResponseFiles.Size = new System.Drawing.Size(206, 121);
            this.listBoxResponseFiles.Sorted = true;
            this.listBoxResponseFiles.TabIndex = 7;
            // 
            // checkBoxIgnitionOk
            // 
            this.checkBoxIgnitionOk.AutoSize = true;
            this.checkBoxIgnitionOk.Checked = true;
            this.checkBoxIgnitionOk.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxIgnitionOk.Location = new System.Drawing.Point(224, 42);
            this.checkBoxIgnitionOk.Name = "checkBoxIgnitionOk";
            this.checkBoxIgnitionOk.Size = new System.Drawing.Size(78, 17);
            this.checkBoxIgnitionOk.TabIndex = 8;
            this.checkBoxIgnitionOk.Text = "Ignition OK";
            this.checkBoxIgnitionOk.UseVisualStyleBackColor = true;
            this.checkBoxIgnitionOk.CheckedChanged += new System.EventHandler(this.checkBoxIgnitionOk_CheckedChanged);
            // 
            // checkBoxAdsAdapter
            // 
            this.checkBoxAdsAdapter.AutoSize = true;
            this.checkBoxAdsAdapter.Location = new System.Drawing.Point(224, 65);
            this.checkBoxAdsAdapter.Name = "checkBoxAdsAdapter";
            this.checkBoxAdsAdapter.Size = new System.Drawing.Size(87, 17);
            this.checkBoxAdsAdapter.TabIndex = 9;
            this.checkBoxAdsAdapter.Text = "ADS adapter";
            this.checkBoxAdsAdapter.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(443, 225);
            this.Controls.Add(this.checkBoxAdsAdapter);
            this.Controls.Add(this.checkBoxIgnitionOk);
            this.Controls.Add(this.listBoxResponseFiles);
            this.Controls.Add(this.groupBoxConcepts);
            this.Controls.Add(this.checkBoxVariableValues);
            this.Controls.Add(this.checkBoxMoving);
            this.Controls.Add(this.listPorts);
            this.Controls.Add(this.buttonConnect);
            this.Name = "MainForm";
            this.Text = "Car Simulator";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.groupBoxConcepts.ResumeLayout(false);
            this.groupBoxConcepts.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.ListBox listPorts;
        private System.Windows.Forms.Timer timerUpdate;
        private System.Windows.Forms.CheckBox checkBoxMoving;
        private System.Windows.Forms.CheckBox checkBoxVariableValues;
        private System.Windows.Forms.RadioButton radioButtonBmwFast;
        private System.Windows.Forms.GroupBox groupBoxConcepts;
        private System.Windows.Forms.RadioButton radioButtonKwp2000S;
        private System.Windows.Forms.RadioButton radioButtonDs2;
        private System.Windows.Forms.ListBox listBoxResponseFiles;
        private System.Windows.Forms.CheckBox checkBoxIgnitionOk;
        private System.Windows.Forms.RadioButton radioButtonIso9141;
        private System.Windows.Forms.CheckBox checkBoxAdsAdapter;

    }
}


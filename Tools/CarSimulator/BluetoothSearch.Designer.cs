namespace CarSimulator
{
    partial class BluetoothSearch
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            listViewDevices = new System.Windows.Forms.ListView();
            columnHeaderAddress = new System.Windows.Forms.ColumnHeader();
            columnHeaderName = new System.Windows.Forms.ColumnHeader();
            columnHeaderBtType = new System.Windows.Forms.ColumnHeader();
            buttonCancel = new System.Windows.Forms.Button();
            buttonOk = new System.Windows.Forms.Button();
            textBoxStatus = new System.Windows.Forms.TextBox();
            buttonSearch = new System.Windows.Forms.Button();
            checkBoxEnableAutoconnect = new System.Windows.Forms.CheckBox();
            labelSearchTypes = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // listViewDevices
            // 
            listViewDevices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderAddress, columnHeaderName, columnHeaderBtType });
            listViewDevices.FullRowSelect = true;
            listViewDevices.GridLines = true;
            listViewDevices.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            listViewDevices.Location = new System.Drawing.Point(13, 27);
            listViewDevices.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            listViewDevices.MultiSelect = false;
            listViewDevices.Name = "listViewDevices";
            listViewDevices.ShowGroups = false;
            listViewDevices.Size = new System.Drawing.Size(594, 299);
            listViewDevices.TabIndex = 1;
            listViewDevices.UseCompatibleStateImageBehavior = false;
            listViewDevices.View = System.Windows.Forms.View.Details;
            listViewDevices.ColumnWidthChanging += listViewDevices_ColumnWidthChanging;
            listViewDevices.SelectedIndexChanged += listViewDevices_SelectedIndexChanged;
            listViewDevices.DoubleClick += listViewDevices_DoubleClick;
            // 
            // columnHeaderAddress
            // 
            columnHeaderAddress.Text = "Address";
            columnHeaderAddress.Width = 217;
            // 
            // columnHeaderName
            // 
            columnHeaderName.Text = "Name";
            columnHeaderName.Width = 320;
            // 
            // columnHeaderBtType
            // 
            columnHeaderBtType.Text = "Type";
            columnHeaderBtType.Width = 50;
            // 
            // buttonCancel
            // 
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Location = new System.Drawing.Point(519, 441);
            buttonCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(88, 27);
            buttonCancel.TabIndex = 4;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // buttonOk
            // 
            buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonOk.Location = new System.Drawing.Point(423, 441);
            buttonOk.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonOk.Name = "buttonOk";
            buttonOk.Size = new System.Drawing.Size(88, 27);
            buttonOk.TabIndex = 5;
            buttonOk.Text = "OK";
            buttonOk.UseVisualStyleBackColor = true;
            // 
            // textBoxStatus
            // 
            textBoxStatus.Location = new System.Drawing.Point(13, 335);
            textBoxStatus.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxStatus.Multiline = true;
            textBoxStatus.Name = "textBoxStatus";
            textBoxStatus.ReadOnly = true;
            textBoxStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            textBoxStatus.Size = new System.Drawing.Size(594, 100);
            textBoxStatus.TabIndex = 2;
            // 
            // buttonSearch
            // 
            buttonSearch.Location = new System.Drawing.Point(327, 441);
            buttonSearch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonSearch.Name = "buttonSearch";
            buttonSearch.Size = new System.Drawing.Size(88, 27);
            buttonSearch.TabIndex = 6;
            buttonSearch.Text = "Search";
            buttonSearch.UseVisualStyleBackColor = true;
            buttonSearch.Click += buttonSearch_Click;
            // 
            // checkBoxEnableAutoconnect
            // 
            checkBoxEnableAutoconnect.AutoSize = true;
            checkBoxEnableAutoconnect.Location = new System.Drawing.Point(12, 446);
            checkBoxEnableAutoconnect.Name = "checkBoxEnableAutoconnect";
            checkBoxEnableAutoconnect.Size = new System.Drawing.Size(95, 19);
            checkBoxEnableAutoconnect.TabIndex = 3;
            checkBoxEnableAutoconnect.Text = "Autoconnect";
            checkBoxEnableAutoconnect.UseVisualStyleBackColor = true;
            // 
            // labelSearchTypes
            // 
            labelSearchTypes.AutoSize = true;
            labelSearchTypes.Location = new System.Drawing.Point(13, 9);
            labelSearchTypes.Name = "labelSearchTypes";
            labelSearchTypes.Size = new System.Drawing.Size(28, 15);
            labelSearchTypes.TabIndex = 0;
            labelSearchTypes.Text = "EDR";
            // 
            // BluetoothSearch
            // 
            AcceptButton = buttonOk;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new System.Drawing.Size(620, 480);
            Controls.Add(labelSearchTypes);
            Controls.Add(checkBoxEnableAutoconnect);
            Controls.Add(buttonSearch);
            Controls.Add(textBoxStatus);
            Controls.Add(buttonOk);
            Controls.Add(buttonCancel);
            Controls.Add(listViewDevices);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "BluetoothSearch";
            ShowInTaskbar = false;
            Text = "Search Bluetooth Devices";
            FormClosing += BluetoothSearch_FormClosing;
            FormClosed += BluetoothSearch_FormClosed;
            Shown += BluetoothSearch_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ListView listViewDevices;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.ColumnHeader columnHeaderAddress;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.TextBox textBoxStatus;
        private System.Windows.Forms.Button buttonSearch;
        private System.Windows.Forms.ColumnHeader columnHeaderBtType;
        private System.Windows.Forms.CheckBox checkBoxEnableAutoconnect;
        private System.Windows.Forms.Label labelSearchTypes;
    }
}
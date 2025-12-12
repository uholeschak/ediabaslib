namespace PsdzClient.Controls
{
    partial class IpAddressControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            textBox1 = new System.Windows.Forms.TextBox();
            textBox2 = new System.Windows.Forms.TextBox();
            textBox3 = new System.Windows.Forms.TextBox();
            textBox4 = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            panel = new System.Windows.Forms.FlowLayoutPanel();
            panel.SuspendLayout();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            textBox1.Location = new System.Drawing.Point(2, 2);
            textBox1.Margin = new System.Windows.Forms.Padding(0);
            textBox1.MaxLength = 3;
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(34, 16);
            textBox1.TabIndex = 0;
            textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            textBox1.KeyPress += ValidateNumericInput;
            // 
            // textBox2
            // 
            textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            textBox2.Location = new System.Drawing.Point(46, 2);
            textBox2.Margin = new System.Windows.Forms.Padding(0);
            textBox2.MaxLength = 3;
            textBox2.Name = "textBox2";
            textBox2.Size = new System.Drawing.Size(34, 16);
            textBox2.TabIndex = 1;
            textBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            textBox2.KeyPress += ValidateNumericInput;
            // 
            // textBox3
            // 
            textBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            textBox3.Location = new System.Drawing.Point(90, 2);
            textBox3.Margin = new System.Windows.Forms.Padding(0);
            textBox3.MaxLength = 3;
            textBox3.Name = "textBox3";
            textBox3.Size = new System.Drawing.Size(34, 16);
            textBox3.TabIndex = 2;
            textBox3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            textBox3.KeyPress += ValidateNumericInput;
            // 
            // textBox4
            // 
            textBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            textBox4.Location = new System.Drawing.Point(134, 2);
            textBox4.Margin = new System.Windows.Forms.Padding(0);
            textBox4.MaxLength = 3;
            textBox4.Name = "textBox4";
            textBox4.Size = new System.Drawing.Size(34, 16);
            textBox4.TabIndex = 3;
            textBox4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            textBox4.KeyPress += ValidateNumericInput;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(36, 2);
            label1.Margin = new System.Windows.Forms.Padding(0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(10, 15);
            label1.TabIndex = 4;
            label1.Text = ".";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(80, 2);
            label2.Margin = new System.Windows.Forms.Padding(0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(10, 15);
            label2.TabIndex = 5;
            label2.Text = ".";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(124, 2);
            label3.Margin = new System.Windows.Forms.Padding(0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(10, 15);
            label3.TabIndex = 6;
            label3.Text = ".";
            // 
            // panel
            // 
            panel.AutoSize = true;
            panel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            panel.BackColor = System.Drawing.SystemColors.Window;
            panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panel.Controls.Add(textBox1);
            panel.Controls.Add(label1);
            panel.Controls.Add(textBox2);
            panel.Controls.Add(label2);
            panel.Controls.Add(textBox3);
            panel.Controls.Add(label3);
            panel.Controls.Add(textBox4);
            panel.Dock = System.Windows.Forms.DockStyle.Fill;
            panel.Location = new System.Drawing.Point(0, 0);
            panel.Margin = new System.Windows.Forms.Padding(0);
            panel.Name = "panel";
            panel.Padding = new System.Windows.Forms.Padding(2);
            panel.Size = new System.Drawing.Size(172, 22);
            panel.TabIndex = 0;
            panel.WrapContents = false;
            // 
            // IpAddressControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(panel);
            Margin = new System.Windows.Forms.Padding(0);
            Name = "IpAddressControl";
            Size = new System.Drawing.Size(172, 22);
            panel.ResumeLayout(false);
            panel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.FlowLayoutPanel panel;
    }
}
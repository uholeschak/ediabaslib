using System;
using System.ComponentModel;
using System.Drawing;
using System.Net;
using System.Windows.Forms;

namespace PsdzClient.Controls
{
    public class IpAddressControl : UserControl
    {
        private TextBox textBox1;
        private TextBox textBox2;
        private TextBox textBox3;
        private TextBox textBox4;
        private Label label1;
        private Label label2;
        private Label label3;
        private FlowLayoutPanel panel;

        public IpAddressControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.panel = new System.Windows.Forms.FlowLayoutPanel();
            this.panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(0, 5);
            this.textBox1.Margin = new System.Windows.Forms.Padding(0);
            this.textBox1.MaxLength = 3;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(30, 20);
            this.textBox1.TabIndex = 0;
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ValidateNumericInput);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 5);
            this.label1.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(10, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = ".";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(40, 5);
            this.textBox2.Margin = new System.Windows.Forms.Padding(0);
            this.textBox2.MaxLength = 3;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(30, 20);
            this.textBox2.TabIndex = 1;
            this.textBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBox2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ValidateNumericInput);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(70, 5);
            this.label2.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(10, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = ".";
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(80, 5);
            this.textBox3.Margin = new System.Windows.Forms.Padding(0);
            this.textBox3.MaxLength = 3;
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(30, 20);
            this.textBox3.TabIndex = 2;
            this.textBox3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBox3.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ValidateNumericInput);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(110, 5);
            this.label3.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(10, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = ".";
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(120, 5);
            this.textBox4.Margin = new System.Windows.Forms.Padding(0);
            this.textBox4.MaxLength = 3;
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(30, 20);
            this.textBox4.TabIndex = 3;
            this.textBox4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBox4.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ValidateNumericInput);
            // 
            // panel
            // 
            this.panel.AutoSize = true;
            this.panel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel.Controls.Add(this.textBox1);
            this.panel.Controls.Add(this.label1);
            this.panel.Controls.Add(this.textBox2);
            this.panel.Controls.Add(this.label2);
            this.panel.Controls.Add(this.textBox3);
            this.panel.Controls.Add(this.label3);
            this.panel.Controls.Add(this.textBox4);
            this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.panel.Location = new System.Drawing.Point(0, 0);
            this.panel.Margin = new System.Windows.Forms.Padding(0);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(150, 25);
            this.panel.TabIndex = 0;
            this.panel.WrapContents = false;
            // 
            // IpAddressControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel);
            this.Name = "IpAddressControl";
            this.Size = new System.Drawing.Size(150, 25);
            this.panel.ResumeLayout(false);
            this.panel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string Text
        {
            get
            {
                if (textBox1 == null || textBox2 == null || textBox3 == null || textBox4 == null)
                {
                    return "0.0.0.0";
                }

                return $"{textBox1.Text}.{textBox2.Text}.{textBox3.Text}.{textBox4.Text}";
            }
            set
            {
                if (textBox1 == null || textBox2 == null || textBox3 == null || textBox4 == null)
                {
                    return;
                }

                if (string.IsNullOrEmpty(value))
                {
                    textBox1.Text = "0";
                    textBox2.Text = "0";
                    textBox3.Text = "0";
                    textBox4.Text = "0";
                    return;
                }

                if (IPAddress.TryParse(value, out IPAddress ip))
                {
                    string[] parts = value.Split('.');
                    if (parts.Length == 4)
                    {
                        textBox1.Text = parts[0];
                        textBox2.Text = parts[1];
                        textBox3.Text = parts[2];
                        textBox4.Text = parts[3];
                    }
                }
            }
        }

        private void ValidateNumericInput(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}
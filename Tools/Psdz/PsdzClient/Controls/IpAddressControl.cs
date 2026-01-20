using System;
using System.ComponentModel;
using System.Drawing;
using System.Net;
using System.Windows.Forms;

namespace PsdzClient.Controls
{
    public partial class IpAddressControl : UserControl
    {
        private const string DefaultIpAddress = "0.0.0.0";

        public IpAddressControl()
        {
            InitializeComponent();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string Text
        {
            get
            {
                if (textBox1 == null || textBox2 == null || textBox3 == null || textBox4 == null)
                {
                    return DefaultIpAddress;
                }

                return $"{textBox1.Text}.{textBox2.Text}.{textBox3.Text}.{textBox4.Text}";
            }
            set
            {
                if (textBox1 == null || textBox2 == null || textBox3 == null || textBox4 == null)
                {
                    return;
                }

                string text = value;
                if (string.IsNullOrEmpty(text))
                {
                    text = DefaultIpAddress;
                }

                if (IPAddress.TryParse(text, out IPAddress ip) &&
                    ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    byte[] addrBytes = ip.GetAddressBytes();
                    if (addrBytes.Length == 4)
                    {
                        textBox1.Text = addrBytes[0].ToString();
                        textBox2.Text = addrBytes[1].ToString();
                        textBox3.Text = addrBytes[2].ToString();
                        textBox4.Text = addrBytes[3].ToString();
                    }
                }
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            UpdateBackgroundColor();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            UpdateBackgroundColor();
        }

        private void UpdateBackgroundColor()
        {
            Color backColor = Enabled ? SystemColors.Window : SystemColors.Control;

            if (panel != null)
            {
                panel.BackColor = backColor;
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
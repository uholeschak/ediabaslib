using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OfflineKeyGenerator
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            textBoxObbKey.Text = Properties.Settings.Default.ObbKey;
            textBoxAppId.Text = Properties.Settings.Default.AppId;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.ObbKey = textBoxObbKey.Text;
                Properties.Settings.Default.AppId = textBoxAppId.Text;
                Properties.Settings.Default.Save();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void buttonCalculate_Click(object sender, EventArgs e)
        {
            textBoxOfflineKey.Text = EncryptObbOfflineKey(textBoxObbKey.Text, textBoxAppId.Text) ?? string.Empty;
        }

        private string EncryptObbOfflineKey(string obbKey, string appId)
        {
            try
            {
                if (string.IsNullOrEmpty(obbKey) || string.IsNullOrEmpty(appId))
                {
                    return null;
                }

                using (AesCryptoServiceProvider crypto = new AesCryptoServiceProvider())
                {
                    crypto.Mode = CipherMode.CBC;
                    crypto.Padding = PaddingMode.PKCS7;
                    crypto.KeySize = 256;

                    using (SHA256Managed sha256 = new SHA256Managed())
                    {
                        crypto.Key = sha256.ComputeHash(Encoding.ASCII.GetBytes(appId));
                    }
                    using (MD5 md5 = MD5.Create())
                    {
                        crypto.IV = md5.ComputeHash(Encoding.ASCII.GetBytes(appId));
                    }

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, crypto.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(obbKey);
                            }
                            byte[] dataEncrypt = msEncrypt.ToArray();
                            return Convert.ToBase64String(dataEncrypt);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}

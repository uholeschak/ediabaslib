using System;
using System.Globalization;
using System.IO;
using System.Security;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace PsdzClient.Utility
{
    [PreserveSource(Hint = "Changed to public", AccessModified = true)]
    public class Encryption
    {
        private static string _clientId = string.Empty;
        private static string _volumeSNr = string.Empty;
        private const string logEncryptionPublicKey = "<RSAKeyValue><Modulus>o0DHJwtLBqYxDLkp7fqN9fhubcWACo2GVfz3qPUJxljUPT4xfZ0QUaFzLpf2YCeOqHGN9093V6dIYtNrukrnLZJtIiZ8kVdBSd3jlJ42QEBjW87XklMez5UKJmjzebs+2NDlaNNcEmhvli2l7GRSbkokqWUuN6SzrS6jIpO8MUk=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        private const string ICSLogEncryptionKeyName = "ICSLogEncryption";
        [PreserveSource(Hint = "Log removed", OriginalHash = "11ED03B5190B41C56AB5C54BC1DCE759")]
        public static string Encrypt(string toEncrypt, bool? isSensitive = false)
        {
            if (isSensitive.HasValue && isSensitive == true)
            {
            // [IGNORE] Logger.Instance()?.LogEncrypted(ICSEventId.ICSNone, "Encryption.Encrypt - string to encrypt", toEncrypt, EventKind.Technical, LogLevel.Info);
            }
            else
            {
            // [IGNORE] Logger.Instance()?.Log(ICSEventId.ICSNone, "Encryption.Encrypt - string to encrypt", toEncrypt, EventKind.Technical, LogLevel.Info);
            }

            if (string.IsNullOrEmpty(toEncrypt))
            {
                return string.Empty;
            }

            Aes aesManaged = null;
            MemoryStream memoryStream = null;
            try
            {
                aesManaged = InializeAesProvider();
                memoryStream = new MemoryStream();
                byte[] bytes = Encoding.UTF8.GetBytes(toEncrypt);
                CryptoStream cryptoStream = new CryptoStream(memoryStream, aesManaged.CreateEncryptor(), CryptoStreamMode.Write);
                cryptoStream.Write(bytes, 0, bytes.Length);
                cryptoStream.FlushFinalBlock();
                return Convert.ToBase64String(memoryStream.ToArray());
            }
            catch (NotSupportedException)
            {
            // [IGNORE] Logger.Instance()?.Log(ICSEventId.ICS0145, "Encryption.Encrypt", ex.ToString(), EventKind.Technical, LogLevel.Error, ex);
            }
            catch (EncoderFallbackException)
            {
            // [IGNORE] Logger.Instance()?.Log(ICSEventId.ICS0139, "Encryption.Encrypt", ex2.ToString(), EventKind.Technical, LogLevel.Error, ex2);
            }
            catch (CryptographicException)
            {
            // [IGNORE] Logger.Instance()?.Log(ICSEventId.ICS0010, "Encryption.Encrypt", ex3.ToString(), EventKind.Technical, LogLevel.Error, ex3);
            }
            catch (Exception ex4)when (ex4 is ArgumentOutOfRangeException || ex4 is ArgumentException)
            {
            // [IGNORE] Logger.Instance()?.Log(ICSEventId.ICS0123, "Encryption.Encrypt", ex4.ToString(), EventKind.Technical, LogLevel.Error, ex4);
            }
            finally
            {
                memoryStream?.Dispose();
                aesManaged?.Dispose();
            }

            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public static string EncryptSensitveContent(string toEncrypt)
        {
            return toEncrypt;
        }

        [PreserveSource(Hint = "Log removed", OriginalHash = "8FBDD1B56316B1B6866356649637F496")]
        public static string Decrypt(string toDecrypt)
        {
            // [IGNORE] Logger.Instance()?.Log(ICSEventId.ICSNone, "Encryption.Decrypt - string to decrypt", toDecrypt, EventKind.Technical, LogLevel.Info);
            if (string.IsNullOrEmpty(toDecrypt))
            {
                return string.Empty;
            }

            try
            {
                using (Aes aesManaged = InializeAesProvider())
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        CryptoStream cryptoStream = new CryptoStream(memoryStream, aesManaged.CreateDecryptor(), CryptoStreamMode.Write);
                        byte[] array = Convert.FromBase64String(toDecrypt);
                        cryptoStream.Write(array, 0, array.Length);
                        cryptoStream.FlushFinalBlock();
                        return Encoding.UTF8.GetString(memoryStream.ToArray());
                    }
                }
            }
            catch (NotSupportedException)
            {
            // [IGNORE] Logger.Instance()?.Log(ICSEventId.ICS0145, "Encryption.Decrypt", ex.ToString(), EventKind.Technical, LogLevel.Error, ex);
            }
            catch (EncoderFallbackException)
            {
            // [IGNORE] Logger.Instance()?.Log(ICSEventId.ICS0139, "Encryption.Decrypt", ex2.ToString(), EventKind.Technical, LogLevel.Error, ex2);
            }
            catch (CryptographicException)
            {
                // [IGNORE] Logger.Instance()?.Log(ICSEventId.ICS0010, "Encryption.Decrypt", ex3.ToString(), EventKind.Technical, LogLevel.Error, ex3);
                throw;
            }
            catch (DecoderFallbackException)
            {
            // [IGNORE] Logger.Instance()?.Log(ICSEventId.ICS0143, "Encryption.Decrypt", ex4.ToString(), EventKind.Technical, LogLevel.Error, ex4);
            }
            catch (FormatException)
            {
                // [IGNORE] Logger.Instance()?.Log(ICSEventId.ICS0129, "Encryption.Decrypt", ex5.ToString(), EventKind.Technical, LogLevel.Error, ex5);
                throw;
            }
            catch (ArgumentException)
            {
            // [IGNORE] Logger.Instance()?.Log(ICSEventId.ICS0123, "Encryption.Decrypt", ex6.ToString(), EventKind.Technical, LogLevel.Error, ex6);
            }

            return string.Empty;
        }

        internal static SecureString GetSecuredDecryptedPassword(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new SecureString();
            }

            return SecureStringHelper.ConvertToSecureString(Decrypt(input));
        }

        [PreserveSource(Hint = "Log removed, changed to Aes", OriginalHash = "44E009B63DFEC7852BC1CDB2DC4A26F8")]
        internal static Aes InializeAesProvider()
        {
            Aes aesManaged = null;
            Aes aesManaged2 = null;
            try
            {
                string clientID = GetClientID();
                string volumeSNr = GetVolumeSNr();
                string text = ReverseString(volumeSNr);
                string s = clientID.Substring(0, clientID.Length / 2);
                string s2 = text + clientID.Substring(clientID.Length / 2) + volumeSNr;
                aesManaged = Aes.Create();
                aesManaged.Key = Encoding.UTF8.GetBytes(s2);
                aesManaged.IV = Encoding.UTF8.GetBytes(s);
                aesManaged2 = aesManaged;
                aesManaged = null;
                return aesManaged2;
            }
            catch (EncoderFallbackException)
            {
                // [IGNORE] Logger.Instance()?.Log(ICSEventId.ICS0139, "Encryption.InitializeAesProvider", $"Could not initialize Aes Provider: Error {ex}", EventKind.Technical, LogLevel.Error, ex);
                throw;
            }
            catch (CryptographicException)
            {
                // [IGNORE] Logger.Instance()?.Log(ICSEventId.ICS0010, "Encryption.InitializeAesProvider", $"Could not initialize Aes Provider: Error {ex2}", EventKind.Technical, LogLevel.Error, ex2);
                throw;
            }
            finally
            {
                aesManaged?.Dispose();
            }
        }

        private static string GetVolumeSNr()
        {
            if (string.IsNullOrEmpty(_volumeSNr))
            {
                _volumeSNr = MachineIdentifier.GetVolumeSerialNumber().Replace("-", string.Empty);
            }

            return _volumeSNr;
        }

        private static string GetClientID()
        {
            if (!string.IsNullOrEmpty(_clientId))
            {
                return _clientId;
            }

            _clientId = ReverseString(MachineIdentifier.GetMachineGuid().Replace("-", string.Empty));
            return _clientId;
        }

        private static string ReverseString(string textvalue)
        {
            if (string.IsNullOrEmpty(textvalue))
            {
                return string.Empty;
            }

            char[] array = textvalue.ToCharArray();
            Array.Reverse(array);
            return new string (array);
        }

        [PreserveSource(Hint = "Log removed, RijndaelManaged replaced", OriginalHash = "9D74D8D08B12D1C3915C71CE8702563C")]
        public static string DecryptPassword(string encryptedPassWord, string deviceIdent)
        {
            if (string.IsNullOrEmpty(encryptedPassWord) || string.IsNullOrEmpty(deviceIdent))
            {
                return string.Empty;
            }

            string result = string.Empty;
            MemoryStream memoryStream = null;
            CryptoStream cryptoStream = null;
            StreamReader streamReader = null;
            Aes aes = null;
            try
            {
                aes = Aes.Create();
                TokenObject tokenObject = GenerateTokens(deviceIdent);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;
                aes.FeedbackSize = 128;
                aes.Key = Encoding.UTF8.GetBytes(tokenObject.Password);
                aes.IV = Encoding.UTF8.GetBytes(tokenObject.Token);
                ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);
                memoryStream = new MemoryStream(Convert.FromBase64String(encryptedPassWord));
                cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read);
                streamReader = new StreamReader(cryptoStream, detectEncodingFromByteOrderMarks: true);
                result = streamReader.ReadToEnd();
            }
            catch (CryptographicException)
            {
            // [IGNORE] Logger.Instance()?.Log(ICSEventId.ICS0010, "Encryption.DecryptPassword", ex.ToString(), EventKind.Technical, LogLevel.Error, ex);
            }
            catch (EncoderFallbackException)
            {
            // [IGNORE] Logger.Instance()?.Log(ICSEventId.ICS0139, "Encryption.DecryptPassword", ex2.ToString(), EventKind.Technical, LogLevel.Error, ex2);
            }
            catch (IOException)
            {
            // [IGNORE] Logger.Instance()?.Log(ICSEventId.ICS0117, "Encryption.DecryptPassword", ex3.ToString(), EventKind.Technical, LogLevel.Error, ex3);
            }
            catch (FormatException)
            {
            // [IGNORE] Logger.Instance()?.Log(ICSEventId.ICS0129, "Encryption.DecryptPassword", ex4.ToString(), EventKind.Technical, LogLevel.Error, ex4);
            }
            catch (InvalidOperationException)
            {
            // [IGNORE] Logger.Instance()?.Log(ICSEventId.ICS0001, "Encryption.DecryptPassword", ex5.ToString(), EventKind.Technical, LogLevel.Error, ex5);
            }
            catch (Exception ex6)when (ex6 is ArgumentNullException || ex6 is ArgumentException)
            {
                // [IGNORE] Logger.Instance()?.Log(ICSEventId.ICS0123, "Encryption.DecryptPassword", ex6.ToString(), EventKind.Technical, LogLevel.Error, ex6);
                throw;
            }
            finally
            {
                if (streamReader != null)
                {
                    streamReader.Close();
                }
                else if (cryptoStream != null)
                {
                    cryptoStream.Close();
                }
                else
                {
                    memoryStream?.Close();
                }

                aes?.Dispose();
            }

            return result;
        }

        internal static TokenObject GenerateTokens(string deviceIdent)
        {
            if (string.IsNullOrEmpty(deviceIdent) || deviceIdent.Length < 19)
            {
                throw new ArgumentException("Invalid argument provided", "deviceIdent");
            }

            TokenObject tokenObject = new TokenObject();
            string text = string.Format(CultureInfo.InvariantCulture, "{0,-19}", deviceIdent.Substring(11, 8));
            text = text.Replace(" ", deviceIdent.Substring(9, 2));
            string text2 = string.Format(CultureInfo.InvariantCulture, "{0,-19}", deviceIdent.Substring(deviceIdent.Length - 19, 8));
            text2 = text2.Replace(" ", deviceIdent.Substring(9, 2));
            tokenObject.Password = GeneratePasswordHash(text).Substring(0, 16);
            tokenObject.Token = GeneratePasswordHash(text2).Substring(0, 16);
            return tokenObject;
        }

        [PreserveSource(Hint = "Log removed, changed to SHA256", OriginalHash = "058171376B703BD354BBFA66FB17720E")]
        private static string GeneratePasswordHash(string passwordString)
        {
            SHA256 sHA256Managed = null;
            try
            {
                sHA256Managed = SHA256.Create();
                byte[] array = sHA256Managed.ComputeHash(Encoding.ASCII.GetBytes(passwordString ?? string.Empty));
                StringBuilder stringBuilder = new StringBuilder();
                byte[] array2 = array;
                foreach (byte b in array2)
                {
                    stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0:x2}", b));
                }

                return stringBuilder.ToString();
            }
            catch (InvalidOperationException)
            {
            // [IGNORE] Logger.Instance()?.Log(ICSEventId.ICS0001, "Encryption.GeneratePasswordHash", ex.ToString(), EventKind.Technical, LogLevel.Error, ex);
            }
            catch (EncoderFallbackException)
            {
            // [IGNORE] Logger.Instance()?.Log(ICSEventId.ICS0139, "Encryption.GeneratePasswordHash", ex2.ToString(), EventKind.Technical, LogLevel.Error, ex2);
            }
            finally
            {
                sHA256Managed?.Dispose();
            }

            return string.Empty;
        }

        [PreserveSource(Hint = "Added")]
        public static string DecryptFile(string fileName)
        {
            try
            {
                if (!File.Exists(fileName))
                {
                    return null;
                }

                string text = Decrypt(ReadAllText(fileName));
                if (string.IsNullOrEmpty(text))
                {
                    return null;
                }

                return text;
            }
            catch (Exception)
            {
                return null;
            }
        }

        [PreserveSource(Hint = "Added")]
        public static bool EncryptFile(string contents, string fileName)
        {
            try
            {
                string encryptedText = Encrypt(contents);
                if (string.IsNullOrEmpty(encryptedText))
                {
                    return false;
                }

                using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (StreamWriter streamWriter = new StreamWriter(fileStream))
                    {
                        streamWriter.Write(encryptedText);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [PreserveSource(Hint = "Added")]
        public static bool SetFileFullAccessControl(string fileName)
        {
            try
            {
                FileInfo fInfo = new FileInfo(fileName);
                FileSecurity accessControl = fInfo.GetAccessControl();
                accessControl.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, AccessControlType.Allow));
                fInfo.SetAccessControl(accessControl);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [PreserveSource(Hint = "Added")]
        public static string ReadAllText(string path)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    byte[] array = new byte[2048];
                    UTF8Encoding utf8Encoding = new UTF8Encoding(true);
                    int num;
                    while ((num = fileStream.Read(array, 0, array.Length)) > 0)
                    {
                        stringBuilder.Append(utf8Encoding.GetString(array, 0, num));
                    }
                }

                string text = stringBuilder.ToString();
                return text;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
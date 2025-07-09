using System;
using System.Globalization;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace PsdzClient.Utility
{
    public class Encryption
    {
        private static string _clientId = string.Empty;

        private static string _volumeSNr = string.Empty;

        private const string logEncryptionPublicKey = "<RSAKeyValue><Modulus>o0DHJwtLBqYxDLkp7fqN9fhubcWACo2GVfz3qPUJxljUPT4xfZ0QUaFzLpf2YCeOqHGN9093V6dIYtNrukrnLZJtIiZ8kVdBSd3jlJ42QEBjW87XklMez5UKJmjzebs+2NDlaNNcEmhvli2l7GRSbkokqWUuN6SzrS6jIpO8MUk=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        private const string ICSLogEncryptionKeyName = "ICSLogEncryption";

        public static string Encrypt(string toEncrypt, bool? isSensitive = false)
        {
            if (isSensitive.HasValue && isSensitive == true)
            {
                //Logger.Instance()?.LogEncrypted(ICSEventId.ICSNone, "Encryption.Encrypt - string to encrypt", toEncrypt, EventKind.Technical, LogLevel.Info);
            }
            else
            {
                //Logger.Instance()?.Log(ICSEventId.ICSNone, "Encryption.Encrypt - string to encrypt", toEncrypt, EventKind.Technical, LogLevel.Info);
            }
            if (string.IsNullOrEmpty(toEncrypt))
            {
                return string.Empty;
            }
            AesManaged aesManaged = null;
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
                //Logger.Instance()?.Log(ICSEventId.ICS0145, "Encryption.Encrypt", ex.ToString(), EventKind.Technical, LogLevel.Error, ex);
            }
            catch (EncoderFallbackException)
            {
                //Logger.Instance()?.Log(ICSEventId.ICS0139, "Encryption.Encrypt", ex2.ToString(), EventKind.Technical, LogLevel.Error, ex2);
            }
            catch (CryptographicException)
            {
                //Logger.Instance()?.Log(ICSEventId.ICS0010, "Encryption.Encrypt", ex3.ToString(), EventKind.Technical, LogLevel.Error, ex3);
            }
            catch (Exception ex4) when (ex4 is ArgumentOutOfRangeException || ex4 is ArgumentException)
            {
                //Logger.Instance()?.Log(ICSEventId.ICS0123, "Encryption.Encrypt", ex4.ToString(), EventKind.Technical, LogLevel.Error, ex4);
            }
            finally
            {
                memoryStream?.Dispose();
                aesManaged?.Dispose();
            }
            return string.Empty;
        }

        public static string EncryptSensitveContent(string toEncrypt)
        {
            RSACryptoServiceProvider rSACryptoServiceProvider = null;
            try
            {
                if (string.IsNullOrEmpty(toEncrypt))
                {
                    return string.Empty;
                }
                rSACryptoServiceProvider = new RSACryptoServiceProvider(new CspParameters
                {
                    KeyContainerName = ICSLogEncryptionKeyName
                });
                rSACryptoServiceProvider.FromXmlString(logEncryptionPublicKey);
                rSACryptoServiceProvider.PersistKeyInCsp = true;
                return Convert.ToBase64String(rSACryptoServiceProvider.Encrypt(Encoding.UTF8.GetBytes(toEncrypt), fOAEP: true));
            }
            catch (NotSupportedException)
            {
                //Logger.Instance()?.Log(ICSEventId.ICS0145, "Encryption.EncryptSensitveContent", ex.ToString(), EventKind.Technical, LogLevel.Error, ex);
            }
            catch (EncoderFallbackException)
            {
                //Logger.Instance()?.Log(ICSEventId.ICS0139, "Encryption.EncryptSensitveContent", ex2.ToString(), EventKind.Technical, LogLevel.Error, ex2);
            }
            catch (CryptographicException)
            {
                //Logger.Instance()?.Log(ICSEventId.ICS0010, "Encryption.EncryptSensitveContent", ex3.ToString(), EventKind.Technical, LogLevel.Error, ex3);
            }
            catch (Exception ex4) when (ex4 is ArgumentOutOfRangeException || ex4 is ArgumentException)
            {
                //Logger.Instance()?.Log(ICSEventId.ICS0123, "Encryption.EncryptSensitveContent", ex4.ToString(), EventKind.Technical, LogLevel.Error, ex4);
            }
            finally
            {
                rSACryptoServiceProvider?.Dispose();
            }
            return string.Empty;
        }

        public static string Decrypt(string toDecrypt)
        {
            //Logger.Instance()?.Log(ICSEventId.ICSNone, "Encryption.Decrypt - string to decrypt", toDecrypt, EventKind.Technical, LogLevel.Info);
            if (string.IsNullOrEmpty(toDecrypt))
            {
                return string.Empty;
            }
            try
            {
                using (AesManaged aesManaged = InializeAesProvider())
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
                //Logger.Instance()?.Log(ICSEventId.ICS0145, "Encryption.Decrypt", ex.ToString(), EventKind.Technical, LogLevel.Error, ex);
            }
            catch (EncoderFallbackException)
            {
                //Logger.Instance()?.Log(ICSEventId.ICS0139, "Encryption.Decrypt", ex2.ToString(), EventKind.Technical, LogLevel.Error, ex2);
            }
            catch (CryptographicException)
            {
                //Logger.Instance()?.Log(ICSEventId.ICS0010, "Encryption.Decrypt", ex3.ToString(), EventKind.Technical, LogLevel.Error, ex3);
                throw;
            }
            catch (DecoderFallbackException)
            {
                //Logger.Instance()?.Log(ICSEventId.ICS0143, "Encryption.Decrypt", ex4.ToString(), EventKind.Technical, LogLevel.Error, ex4);
            }
            catch (FormatException)
            {
                //Logger.Instance()?.Log(ICSEventId.ICS0129, "Encryption.Decrypt", ex5.ToString(), EventKind.Technical, LogLevel.Error, ex5);
                throw;
            }
            catch (ArgumentException)
            {
                //Logger.Instance()?.Log(ICSEventId.ICS0123, "Encryption.Decrypt", ex6.ToString(), EventKind.Technical, LogLevel.Error, ex6);
            }
            return string.Empty;
        }

        public static SecureString GetSecuredDecryptedPassword(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new SecureString();
            }
            return SecureStringHelper.ConvertToSecureString(Decrypt(input));
        }

        public static AesManaged InializeAesProvider()
        {
            AesManaged aesManaged = null;
            AesManaged aesManaged2 = null;
            try
            {
                string clientID = GetClientID();
                string volumeSNr = GetVolumeSNr();
                string text = ReverseString(volumeSNr);
                string s = clientID.Substring(0, clientID.Length / 2);
                string s2 = text + clientID.Substring(clientID.Length / 2) + volumeSNr;
                aesManaged = new AesManaged
                {
                    Key = Encoding.UTF8.GetBytes(s2),
                    IV = Encoding.UTF8.GetBytes(s)
                };
                aesManaged2 = aesManaged;
                aesManaged = null;
                return aesManaged2;
            }
            catch (EncoderFallbackException)
            {
                //Logger.Instance()?.Log(ICSEventId.ICS0139, "Encryption.InitializeAesProvider", $"Could not initialize Aes Provider: Error {ex}", EventKind.Technical, LogLevel.Error, ex);
                throw;
            }
            catch (CryptographicException)
            {
                //Logger.Instance()?.Log(ICSEventId.ICS0010, "Encryption.InitializeAesProvider", $"Could not initialize Aes Provider: Error {ex2}", EventKind.Technical, LogLevel.Error, ex2);
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
            return new string(array);
        }

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
            RijndaelManaged rijndaelManaged = null;
            try
            {
                rijndaelManaged = new RijndaelManaged();
                TokenObject tokenObject = GenerateTokens(deviceIdent);
                rijndaelManaged.Mode = CipherMode.CBC;
                rijndaelManaged.Padding = PaddingMode.None;
                rijndaelManaged.FeedbackSize = 128;
                rijndaelManaged.Key = Encoding.UTF8.GetBytes(tokenObject.Password);
                rijndaelManaged.IV = Encoding.UTF8.GetBytes(tokenObject.Token);
                ICryptoTransform transform = rijndaelManaged.CreateDecryptor(rijndaelManaged.Key, rijndaelManaged.IV);
                memoryStream = new MemoryStream(Convert.FromBase64String(encryptedPassWord));
                cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read);
                streamReader = new StreamReader(cryptoStream, detectEncodingFromByteOrderMarks: true);
                result = streamReader.ReadToEnd();
            }
            catch (CryptographicException)
            {
                //Logger.Instance()?.Log(ICSEventId.ICS0010, "Encryption.DecryptPassword", ex.ToString(), EventKind.Technical, LogLevel.Error, ex);
            }
            catch (EncoderFallbackException)
            {
                //Logger.Instance()?.Log(ICSEventId.ICS0139, "Encryption.DecryptPassword", ex2.ToString(), EventKind.Technical, LogLevel.Error, ex2);
            }
            catch (IOException)
            {
                //Logger.Instance()?.Log(ICSEventId.ICS0117, "Encryption.DecryptPassword", ex3.ToString(), EventKind.Technical, LogLevel.Error, ex3);
            }
            catch (FormatException)
            {
                //Logger.Instance()?.Log(ICSEventId.ICS0129, "Encryption.DecryptPassword", ex4.ToString(), EventKind.Technical, LogLevel.Error, ex4);
            }
            catch (InvalidOperationException)
            {
                //Logger.Instance()?.Log(ICSEventId.ICS0001, "Encryption.DecryptPassword", ex5.ToString(), EventKind.Technical, LogLevel.Error, ex5);
            }
            catch (Exception ex6) when (ex6 is ArgumentNullException || ex6 is ArgumentException)
            {
                //Logger.Instance()?.Log(ICSEventId.ICS0123, "Encryption.DecryptPassword", ex6.ToString(), EventKind.Technical, LogLevel.Error, ex6);
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
                rijndaelManaged?.Dispose();
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

        private static string GeneratePasswordHash(string passwordString)
        {
            SHA256Managed sHA256Managed = null;
            try
            {
                sHA256Managed = new SHA256Managed();
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
                //Logger.Instance()?.Log(ICSEventId.ICS0001, "Encryption.GeneratePasswordHash", ex.ToString(), EventKind.Technical, LogLevel.Error, ex);
            }
            catch (EncoderFallbackException)
            {
                //Logger.Instance()?.Log(ICSEventId.ICS0139, "Encryption.GeneratePasswordHash", ex2.ToString(), EventKind.Technical, LogLevel.Error, ex2);
            }
            finally
            {
                sHA256Managed?.Dispose();
            }
            return string.Empty;
        }
    }
}
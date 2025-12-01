using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;

namespace PsdzClient.Utility
{
    internal static class SecureStringHelper
    {
        [PreserveSource(Hint = "Modified")]
        public static SecureString ConvertToSecureString(string input)
        {
            SecureString secureString = null;
            SecureString secureString2 = null;
            try
            {
                secureString2 = new SecureString();
                if (!string.IsNullOrEmpty(input))
                {
                    char[] array = input.ToCharArray();
                    for (int i = 0; i < array.Length; i++)
                    {
                        secureString2.AppendChar(array[i]);
                    }
                }
                secureString2.MakeReadOnly();
                secureString = secureString2;
                secureString2 = null;
            }
            catch (ArgumentOutOfRangeException)
            {
                // [IGNORE] Logger.Instance()?.Log(ICSEventId.ICS0123, "SecureStringHelper.ConvertToSecureString", $"Length of the secure string is grater than 65,536 characters:  {ex}", EventKind.Technical, LogLevel.Error, ex);
            }
            catch (CryptographicException)
            {
                // [IGNORE] Logger.Instance()?.Log(ICSEventId.ICS0010, "SecureStringHelper.ConvertToSecureString", $"Error converting to secure string: {ex2}", EventKind.Technical, LogLevel.Error, ex2);
            }
            finally
            {
                secureString2?.Dispose();
            }
            return secureString ?? new SecureString();
        }

        public static string ConvertToUnsecureString(SecureString input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            IntPtr intPtr = IntPtr.Zero;
            try
            {
                intPtr = Marshal.SecureStringToGlobalAllocUnicode(input);
                return Marshal.PtrToStringUni(intPtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(intPtr);
            }
        }

        public static bool IsEqual(this SecureString str1, SecureString str2)
        {
            if (IsNullOrEmpty(str1))
            {
                return IsNullOrEmpty(str2);
            }
            return ConvertToUnsecureString(str1) == ConvertToUnsecureString(str2);
        }

        [PreserveSource(Hint = "Modified")]
        public static byte[] GetAsByteArray(SecureString input)
        {
            if (IsNullOrEmpty(input))
            {
                return new byte[0];
            }
            byte[] array = new byte[input.Length];
            IntPtr intPtr = Marshal.SecureStringToBSTR(input);
            try
            {
                byte b = 1;
                int num = 0;
                int num2 = 0;
                while ((b = Marshal.ReadByte(intPtr, num)) != 0)
                {
                    array[num2] = b;
                    num += 2;
                    num2++;
                }
            }
            catch (AccessViolationException)
            {
                // [IGNORE] Logger.Instance()?.Log(ICSEventId.ICSNone, "SecureStringHelper.GetAsByteArray", ex.ToString(), EventKind.Technical, LogLevel.Error, ex);
            }
            finally
            {
                Marshal.ZeroFreeBSTR(intPtr);
            }
            return array;
        }

        public static bool IsNullOrEmpty(SecureString input)
        {
            if (input != null)
            {
                return input.Length == 0;
            }
            return true;
        }
    }
}
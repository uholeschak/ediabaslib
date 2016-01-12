using Mono.Options;
using Mono.Touch.Activation.Client;
using Mono.Touch.Activation.Common;
using MonoTouch;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using Xamarin.Licensing.Interop;
namespace Xamarin.Licensing
{
	internal static class PlatformActivation
	{
		private const string Passphrase = "22BA3F38-7552-47BC-9548-ECA8AFB9356D";
		private const int IterationCount = 2000;
		private const int KeyLengthBits = 256;
		private const int SaltLength = 8;
		private const string PRODUCT = "Xamarin.iOS";
		private const string ENTITLEMENT_PRODUCT = "MonoTouch";
		private const string PRODUCT_ID = "MT";
		private const int PRODUCT_VERSION = 5;
		private const string OfflineActivationFilename = "MTActivation.dat";
		private const bool WRITE_OFFLINE_ACTIVATION_FILE = false;
		private const string LicenseTrialFilename = "monotouch.trial.licx";
		private const string LicenseFilename = "monotouch.licx";
		private const bool SupportsStarter = true;
		private const string ServerVariantFilenameBase = "mtouch";
		public static long BuildStamp;
		private static LicenseType level;
		private static DateTime expires;
		private static readonly string ACTIVATION_URL;
		private static string TrialPath;
		private static string LicensePath;
		private static Activation activation;
		private static bool activate;
		private static bool show_entitlements;
		private static bool check_subscription;
		private static bool data_file;
		private static string activation_email = "b4a";
		private static string activation_company = "b4a";
		private static string activation_phone = "b4a";
		private static string activation_code = "b4a";
		private static string activation_name = "b4a";
		private static string activation_debug_file;
		private static readonly RNGCryptoServiceProvider rng;
		private static string LicenseDirectory;
		public static LicenseType Level
		{
			get
			{
				if (PlatformActivation.level == LicenseType.None)
				{
					ErrorHelper.Error(9998, "Internal error. Please contact support@xamarin.com", new object[0]);
				}
				return PlatformActivation.level;
			}
		}
		public static bool Trial
		{
			get
			{
				return File.Exists(PlatformActivation.TrialPath);
			}
		}
		public static DateTime Expires
		{
			get
			{
				if (PlatformActivation.level == LicenseType.None)
				{
					ErrorHelper.Error(9998, "Internal error. Please contact support@xamarin.com", new object[0]);
				}
				return PlatformActivation.expires;
			}
		}
		private static Activation Activation
		{
			get
			{
				if (PlatformActivation.activation == null)
				{
					PlatformActivation.activation = new Activation(Certificates.Server, Certificates.Client, null, PlatformActivation.ACTIVATION_URL);
				}
				return PlatformActivation.activation;
			}
		}
		private static Crypto Crypto
		{
			get
			{
				return PlatformActivation.Activation.Crypto;
			}
		}
		private static bool HasFileVariantInHomeFolder(string baseName)
		{
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			return File.Exists(Path.Combine(folderPath, baseName)) || File.Exists(Path.Combine(folderPath, "." + baseName));
		}
		static PlatformActivation()
		{
			PlatformActivation.BuildStamp = 635253033668640131L;
			PlatformActivation.level = LicenseType.None;
			PlatformActivation.expires = DateTime.MinValue;
			PlatformActivation.ACTIVATION_URL = "https://activation.xamarin.com/ActivationService.asmx";
			PlatformActivation.activation_debug_file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "XamarinActivationLog.bin");
			PlatformActivation.rng = new RNGCryptoServiceProvider();
			PlatformActivation.InitPaths();
			PlatformActivation.TrialPath = Path.Combine(PlatformActivation.LicenseDirectory, "monotouch.trial.licx");
			PlatformActivation.LicensePath = Path.Combine(PlatformActivation.LicenseDirectory, "monotouch.licx");
			bool flag = PlatformActivation.HasFileVariantInHomeFolder("xamarin-use-staging") || PlatformActivation.HasFileVariantInHomeFolder("mtouch-use-staging");
			bool flag2 = !flag && (PlatformActivation.HasFileVariantInHomeFolder("xamarin-use-dev") || PlatformActivation.HasFileVariantInHomeFolder("mtouch-use-dev"));
			if (flag)
			{
				Console.Error.WriteLine("Xamarin.iOS: USING STAGING SERVER FOR ACTIVATION");
				PlatformActivation.ACTIVATION_URL = "https://activation.xamstage.com/ActivationService.asmx";
				Certificates.Server = Certificates.StagingServer;
			}
			else
			{
				if (flag2)
				{
					Console.Error.WriteLine("Xamarin.iOS: USING DEV SERVER FOR ACTIVATION");
					PlatformActivation.ACTIVATION_URL = "https://activation.xamdev.com/ActivationService.asmx";
					Certificates.Server = Certificates.StagingServer;
				}
			}
			ServicePointManager.ServerCertificateValidationCallback = delegate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
			{
				switch (sslPolicyErrors)
				{
				case SslPolicyErrors.None:
					return true;
				case SslPolicyErrors.RemoteCertificateChainErrors:
					return chain.ChainElements.Count > 1;
				}
				return false;
			};
		}
		private static string GetRegistrationXml(License license = null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string text = WinNetworkInterfaces.B();
			string text2 = null;
			if (license != null)
			{
				string[] array = WinNetworkInterfaces.A();
				for (int i = 0; i < array.Length; i++)
				{
					string text3 = array[i];
					if (PlatformActivation.CheckHashes(license, Certificates.Salt, text, text3))
					{
						text2 = text3;
						break;
					}
				}
			}
			if (text2 == null)
			{
				text2 = WinNetworkInterfaces.A().First<string>();
			}
			stringBuilder.Append("<plist><array><dict><array><dict>\n");
			stringBuilder.AppendFormat("  <key>serial_number</key><string>{0}</string>\n", text);
			stringBuilder.AppendFormat("  <key>local_host_name</key><string>{0}</string>\n", Environment.MachineName);
			stringBuilder.Append("</dict></array></dict><dict><array><dict><array><dict>\n");
			stringBuilder.Append("  <key>bsd_device_name</key><string>en0</string>\n");
			stringBuilder.AppendFormat("  <key>hardware_address</key><string>{0}</string>\n", text2);
			stringBuilder.AppendFormat("  <key>os_version</key><string>{0}</string>\n", Environment.OSVersion.ToString());
			stringBuilder.Append("  <key>ide</key><string>VS2010</string>\n");
			stringBuilder.Append("</dict></array></dict></array></dict></array></plist>\n");
			return stringBuilder.ToString();
		}
		private static bool CheckHashes(License license, byte[] salt, string s1, string s2)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(s1);
			byte[] bytes2 = Encoding.UTF8.GetBytes(s2);
			MemoryStream ms = new MemoryStream(bytes.Length + bytes2.Length + salt.Length);
			HashAlgorithm alg = SHA1.Create();
			byte[] hash = PlatformActivation.GetHash(alg, ms, bytes, bytes2, salt);
			byte[] hash2 = PlatformActivation.GetHash(alg, ms, bytes, salt, bytes2);
			byte[] hash3 = PlatformActivation.GetHash(alg, ms, salt, bytes, bytes2);
			return PlatformActivation.BuffersAreEqual(hash, license.UserData.H1) && PlatformActivation.BuffersAreEqual(hash2, license.UserData.H2) && PlatformActivation.BuffersAreEqual(hash3, license.UserData.H3);
		}
		private static bool CheckHashes(License license, string s1, string s2)
		{
			return PlatformActivation.CheckHashes(license, Certificates.Salt, s1, s2);
		}
		private static byte[] GetHash(HashAlgorithm alg, MemoryStream ms, byte[] b1, byte[] b2, byte[] b3)
		{
			ms.Position = 0L;
			ms.SetLength(0L);
			ms.Write(b1, 0, b1.Length);
			ms.Write(b2, 0, b2.Length);
			ms.Write(b3, 0, b3.Length);
			return alg.ComputeHash(ms.GetBuffer(), 0, (int)ms.Length);
		}
		private static bool BuffersAreEqual(byte[] a, byte[] b)
		{
			for (int i = 0; i < a.Length; i++)
			{
				if (a[i] != b[i])
				{
					return false;
				}
			}
			return true;
		}
		private static byte[] GetFingerprint(string salt)
		{
			string s = string.Concat(new string[]
			{
				"MonoTouch",
				PlatformActivation.expires.ToString("s"),
				PlatformActivation.Trial ? "TRIAL" : PlatformActivation.level.ToString(),
				Environment.MachineName,
				salt
			});
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			byte[] result;
			using (SHA1 sHA = SHA1.Create())
			{
				result = sHA.ComputeHash(bytes);
			}
			return result;
		}
		private static string GetEntitlements()
		{
			StringBuilder stringBuilder = new StringBuilder();
			byte[] fingerprint = PlatformActivation.GetFingerprint("nlJ2/Tj\fn,Xi(4rVq!A");
			for (int i = 0; i < fingerprint.Length; i++)
			{
				byte b = fingerprint[i];
				stringBuilder.Append(b.ToString("x2"));
			}
			stringBuilder.Append(' ');
			stringBuilder.Append("MonoTouch");
			stringBuilder.Append(' ');
			stringBuilder.Append(PlatformActivation.Trial ? "TRIAL" : PlatformActivation.level.ToString());
			stringBuilder.Append(' ').Append(PlatformActivation.expires.ToString("s"));
			return stringBuilder.ToString();
		}
		private static string GetNodeText(XmlDocument doc, string path, string key)
		{
			foreach (XmlNode xmlNode in doc.SelectNodes(path))
			{
				if (!(xmlNode.InnerText != key))
				{
					XmlNode nextSibling = xmlNode.NextSibling;
					string result;
					if (nextSibling.Name != "string")
					{
						result = null;
						return result;
					}
					result = nextSibling.InnerText;
					return result;
				}
			}
			return null;
		}
		private static License LoadLicense(string path)
		{
			byte[] array;
			try
			{
				using (FileStream fileStream = File.OpenRead(path))
				{
					long length = fileStream.Length;
					if (fileStream.Length > 32768L)
					{
						ErrorHelper.Error(9996, "Invalid license. Please reactivate {0}", new object[]
						{
							"Xamarin.iOS"
						});
					}
					array = new byte[(int)length];
					fileStream.Read(array, 0, (int)length);
				}
			}
			catch (UnauthorizedAccessException)
			{
				ErrorHelper.Error(9028, "Access to the path '{0}' is denied.", new object[]
				{
					path
				});
				throw;
			}
			catch (MonoTouchException)
			{
				throw;
			}
			catch
			{
				ErrorHelper.Error(9999, "Invalid license.  Please reactivate {0}", new object[]
				{
					"Xamarin.iOS"
				});
				throw;
			}
			License result;
			try
			{
				result = License.LoadFromBytes(PlatformActivation.Crypto, array);
			}
			catch (Exception)
			{
				ErrorHelper.Error(9999, "Invalid license.  Please reactivate {0}", new object[]
				{
					"Xamarin.iOS"
				});
				throw;
			}
			return result;
		}
		public static void VerifyLicense()
		{
			License license = null;
			if (File.Exists(PlatformActivation.TrialPath))
			{
				license = PlatformActivation.LoadLicense(PlatformActivation.TrialPath);
				if (!PlatformActivation.IsTrial(license.UserData.ProductId))
				{
					ErrorHelper.Error(9020, "Invalid license. Please reactivate {0}.", new object[]
					{
						"Xamarin.iOS"
					});
				}
				PlatformActivation.level = LicenseType.Business;
				PlatformActivation.expires = license.UserData.ExpirationDate;
				if (PlatformActivation.expires < DateTime.Now)
				{
					ErrorHelper.Error(9001, "Trial period has expired.", new object[0]);
				}
			}
			else
			{
				if (File.Exists(PlatformActivation.LicensePath))
				{
					license = PlatformActivation.LoadLicense(PlatformActivation.LicensePath);
					if (license.UserData.ProductId == ProductId.None)
					{
						PlatformActivation.level = LicenseType.Priority;
						license.UserData.ProductVersion = 5;
						DateTime dateTime = new DateTime(2013, 7, 20);
						if (license.UserData.ExpirationDate > dateTime)
						{
							license.UserData.ExpirationDate = dateTime;
						}
					}
					else
					{
						if (PlatformActivation.IsTrial(license.UserData.ProductId))
						{
							ErrorHelper.Error(9018, "Invalid license. Please reactivate {0}.", new object[]
							{
								"Xamarin.iOS"
							});
						}
						else
						{
							PlatformActivation.level = PlatformActivation.GetLicenseType(license.UserData.ProductId);
							if (PlatformActivation.level == LicenseType.None)
							{
								ErrorHelper.Error(9010, "License type could not be verified ({0}). Please contact support@xamarin.com", new object[]
								{
									(int)license.UserData.ProductId
								});
							}
						}
					}
					PlatformActivation.expires = license.UserData.ExpirationDate;
					if (PlatformActivation.expires.Ticks < PlatformActivation.BuildStamp)
					{
						ErrorHelper.Error(9000, "This version was released after your subscription expired ({0}).", new object[]
						{
							PlatformActivation.expires
						});
					}
				}
				else
				{
					PlatformActivation.level = LicenseType.Starter;
				}
			}
			if (license != null)
			{
				bool flag = false;
				string s = WinNetworkInterfaces.B();
				string[] array = WinNetworkInterfaces.A();
				for (int i = 0; i < array.Length; i++)
				{
					string s2 = array[i];
					if (PlatformActivation.CheckHashes(license, s, s2))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					ErrorHelper.Error(9021, "Invalid license. Please reactivate Xamarin.iOS", new object[0]);
				}
				if (license.UserData.ProductVersion < 5)
				{
					ErrorHelper.Error(9023, "Invalid license. Please reactivate Xamarin.iOS", new object[0]);
				}
			}
		}
		private static bool ValidateXML(string xml)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(new StringReader(xml));
			string a = PlatformActivation.GetNodeText(xmlDocument, "/plist/array/dict/array/dict/key", "serial_number");
			if (a == "Not Available")
			{
				a = "System Serial#";
			}
			string b = WinNetworkInterfaces.B();
			return !(a != b);
		}
		private static void WriteOfflineActivationFile(UserData user)
		{
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "MTActivation.dat");
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			byte[] array = user.Serialize();
			using (FileStream fileStream = File.Create(path))
			{
				fileStream.Write(array, 0, array.Length);
			}
		}
		private static void Activate(UserData user)
		{
			if (!PlatformActivation.ValidateXML(user.DataFile))
			{
				ErrorHelper.Error(9015, "Activation failed: Integrity checks failed. Please contact support@xamarin.com", new object[0]);
			}
			try
			{
				ActivationResponse activationResponse = PlatformActivation.Activation.Activate3("MT", user);
				if (activationResponse.ResponseCode != ActivationResponseCode.Success)
				{
					if (activationResponse.ResponseCode == ActivationResponseCode.InvalidProductVersion)
					{
						ErrorHelper.Error(9000, "This version was released after your subscription expired.", new object[0]);
					}
					else
					{
						ErrorHelper.Error(9014, "Activation failed: {0} ({1}). Please contact support@xamarin.com", new object[]
						{
							activationResponse.ResponseCode,
							activationResponse.Message
						});
					}
				}
				License license = License.LoadFromBytes(PlatformActivation.Crypto, activationResponse.License);
				if (license.UserData.ProductId == ProductId.None || !Enum.IsDefined(typeof(ProductId), license.UserData.ProductId))
				{
					ErrorHelper.Error(9017, "Activation failed: Server did not provide a valid license ({0}). Please contact support@xamarin.com", new object[]
					{
						(int)license.UserData.ProductId
					});
				}
				bool flag = PlatformActivation.IsTrial(license.UserData.ProductId);
				try
				{
					if (!Directory.Exists(PlatformActivation.LicenseDirectory))
					{
						Directory.CreateDirectory(PlatformActivation.LicenseDirectory);
					}
				}
				catch (Exception innerException)
				{
					ErrorHelper.Error(9027, innerException, "Failed to create the directory '{0}' for the license.", new object[]
					{
						PlatformActivation.LicenseDirectory
					});
				}
				string text = flag ? PlatformActivation.TrialPath : PlatformActivation.LicensePath;
				try
				{
					File.WriteAllBytes(text, activationResponse.License);
				}
				catch (Exception ex)
				{
					ErrorHelper.Error(9026, ex, "Failed to write the license file '{0}': {1}", new object[]
					{
						text,
						ex.Message
					});
				}
				if (!flag && File.Exists(PlatformActivation.TrialPath))
				{
					File.Delete(PlatformActivation.TrialPath);
				}
			}
			catch (WebException innerException2)
			{
				ErrorHelper.Error(9012, innerException2, "Activation requires a working network connection. Please contact support@xamarin.com", new object[0]);
			}
			catch (MonoTouchException)
			{
				throw;
			}
			catch (Exception innerException3)
			{
				PlatformActivation.ReportError(9013, innerException3, "Unknown error. Please contact support@xamarin.com", new object[0]);
			}
		}
		private static void ActivateImpl(string name, string email, string company, string phone, string code)
		{
			string text = Path.Combine(PlatformActivation.LicenseDirectory, "monotouch.licx");
			License license = null;
			try
			{
				if (File.Exists(text))
				{
					license = License.LoadFromFile(PlatformActivation.Crypto, text);
				}
			}
			catch
			{
				ErrorHelper.Warning(9022, "Failed to load existing subscription data from '{0}'. Please contact support@xamarin.com if activation doesn't proceed as expected.", new object[]
				{
					text
				});
			}
			UserData user = new UserData
			{
				Name = name,
				Email = email,
				ActivationCode = code,
				Company = company,
				Phone = phone,
				DataFile = PlatformActivation.GetRegistrationXml(license),
				ProductVersion = 5
			};
			try
			{
				PlatformActivation.Activate(user);
			}
			catch
			{
				PlatformActivation.WriteOfflineActivationFile(user);
				throw;
			}
		}
		private static void Activate(string name, string email, string company, string phone, string code)
		{
			try
			{
				PlatformActivation.ActivateImpl(name, email, company, phone, code);
			}
			catch (MonoTouchException)
			{
				throw;
			}
			catch (Exception innerException)
			{
				PlatformActivation.ReportError(9013, innerException, "Unknown error. Please contact support@xamarin.com", new object[0]);
			}
		}
		private static void CheckImpl()
		{
			string path = PlatformActivation.Trial ? PlatformActivation.TrialPath : PlatformActivation.LicensePath;
			if (!File.Exists(path))
			{
				PlatformActivation.Entitlements();
				return;
			}
			License license = PlatformActivation.LoadLicense(path);
			PlatformActivation.expires = license.UserData.ExpirationDate;
			try
			{
				PlatformActivation.Activate(new UserData
				{
					Name = license.UserData.Name,
					Company = license.UserData.Company,
					Email = license.UserData.Email,
					Phone = license.UserData.Phone,
					ActivationCode = license.UserData.ActivationCode,
					DataFile = PlatformActivation.GetRegistrationXml(license),
					ProductVersion = 5
				});
			}
			catch (MonoTouchException ex)
			{
				if (ex.Code >= 9000 && ex.Code <= 9999)
				{
					int code = ex.Code;
					if (code != 9000)
					{
						switch (code)
						{
						case 9012:
						case 9013:
							break;
						default:
							PlatformActivation.ReportError(ex.Code, ex, false, "This license file has been revoked due to ProductException caught in CheckImpl.", new object[0]);
							File.WriteAllText(path, "This license file has been revoked.");
							throw;
						}
					}
					PlatformActivation.Entitlements();
					throw;
				}
			}
			catch
			{
			}
			PlatformActivation.Entitlements();
		}
		public static void Check()
		{
			try
			{
				PlatformActivation.CheckImpl();
			}
			catch (MonoTouchException)
			{
				throw;
			}
			catch (Exception innerException)
			{
				PlatformActivation.ReportError(9013, innerException, "Unknown error. Please contact support@xamarin.com", new object[0]);
			}
		}
		private static void EntitlementsImpl()
		{
			try
			{
				PlatformActivation.VerifyLicense();
			}
			catch (MonoTouchException ex)
			{
				switch (ex.Code)
				{
				case 9000:
				case 9001:
					Console.WriteLine(PlatformActivation.GetEntitlements());
					throw;
				default:
					throw;
				}
			}
			Console.WriteLine(PlatformActivation.GetEntitlements());
		}
		private static void Entitlements()
		{
			try
			{
				PlatformActivation.EntitlementsImpl();
			}
			catch (MonoTouchException)
			{
				throw;
			}
			catch (Exception innerException)
			{
				PlatformActivation.ReportError(9013, innerException, "Unknown error. Please contact support@xamarin.com", new object[0]);
			}
		}
		public static void AddOptions(OptionSet options)
		{
			options.Add("activate", "Activate the product", delegate(string v)
			{
				PlatformActivation.activate = true;
			}, true);
			options.Add("activation-name=", "Name for activation", delegate(string v)
			{
				PlatformActivation.activation_name = v;
			}, true);
			options.Add("activation-email=", "E-mail for activation", delegate(string v)
			{
				PlatformActivation.activation_email = v;
			}, true);
			options.Add("activation-company=", "Company for activation", delegate(string v)
			{
				PlatformActivation.activation_company = v;
			}, true);
			options.Add("activation-phone=", "Phone for activation", delegate(string v)
			{
				PlatformActivation.activation_phone = v;
			}, true);
			options.Add("activation-code=", "Code for activation", delegate(string v)
			{
				PlatformActivation.activation_code = v;
			}, true);
			options.Add("activation-debug-file=", "File to write debug information to", delegate(string v)
			{
				PlatformActivation.activation_debug_file = v;
			}, true);
			options.Add("check", "Check (online) if the current subscription is valid", delegate(string v)
			{
				PlatformActivation.check_subscription = true;
			}, true);
			options.Add("entitlements", "Output the licensing information and exit.", delegate(string v)
			{
				PlatformActivation.show_entitlements = true;
			});
			options.Add("datafile", "Output the data file used for system identification.", delegate(string v)
			{
				PlatformActivation.data_file = true;
			}, true);
		}
		public static bool ProcessOptions()
		{
			if (PlatformActivation.activate)
			{
				if (PlatformActivation.activation_email == null || PlatformActivation.activation_company == null || PlatformActivation.activation_phone == null || PlatformActivation.activation_code == null || PlatformActivation.activation_name == null)
				{
					ErrorHelper.Error(9997, "Incomplete data provided to complete activation.", new object[0]);
				}
				PlatformActivation.Activate(PlatformActivation.activation_name, PlatformActivation.activation_email, PlatformActivation.activation_company, PlatformActivation.activation_phone, PlatformActivation.activation_code);
				return true;
			}
			if (PlatformActivation.check_subscription)
			{
				PlatformActivation.Check();
				return true;
			}
			if (PlatformActivation.show_entitlements)
			{
				PlatformActivation.Entitlements();
				return true;
			}
			if (PlatformActivation.data_file)
			{
				PlatformActivation.ShowDataFile();
				return true;
			}
			try
			{
				PlatformActivation.VerifyLicense();
			}
			catch (MonoTouchException)
			{
				throw;
			}
			catch (AggregateException)
			{
				throw;
			}
			catch (Exception)
			{
				ErrorHelper.Error(9999, "Invalid license.  Please reactivate {0}", new object[]
				{
					"Xamarin.iOS"
				});
			}
			return false;
		}
		private static byte[] GenerateRandomBytes(int count)
		{
			byte[] array = new byte[count];
			PlatformActivation.rng.GetBytes(array);
			return array;
		}
		private static string EncryptString(string ciphertext)
		{
			byte[] array = PlatformActivation.GenerateRandomBytes(8);
			byte[] array2 = PlatformActivation.GenerateRandomBytes(16);
			byte[] rgbKey = PlatformActivation.DeriveKey(array);
			byte[] bytes = Encoding.UTF8.GetBytes(ciphertext);
			byte[] array3;
			using (AesCryptoServiceProvider aesCryptoServiceProvider = new AesCryptoServiceProvider())
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					ICryptoTransform transform = aesCryptoServiceProvider.CreateEncryptor(rgbKey, array2);
					using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
					{
						cryptoStream.Write(bytes, 0, bytes.Length);
					}
					array3 = memoryStream.ToArray();
				}
			}
			byte[] array4 = new byte[24 + array3.Length];
			Buffer.BlockCopy(array2, 0, array4, 0, 16);
			Buffer.BlockCopy(array, 0, array4, 16, 8);
			Buffer.BlockCopy(array3, 0, array4, 24, array3.Length);
			return Convert.ToBase64String(array4);
		}
		private static byte[] DeriveKey(byte[] salt)
		{
			Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes("22BA3F38-7552-47BC-9548-ECA8AFB9356D", salt, 2000);
			return rfc2898DeriveBytes.GetBytes(32);
		}
		private static void ShowDataFile()
		{
			string registrationXml = PlatformActivation.GetRegistrationXml(null);
			Console.WriteLine(PlatformActivation.EncryptString(registrationXml));
		}
		private static void ReportError(int code, Exception innerException, string message, params object[] args)
		{
			PlatformActivation.ReportError(code, innerException, true, message, args);
		}
		private static void ReportError(int code, Exception innerException, bool report, string message, params object[] args)
		{
			try
			{
				if (!string.IsNullOrEmpty(PlatformActivation.activation_debug_file))
				{
					using (MemoryStream memoryStream = new MemoryStream())
					{
						using (StreamWriter streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
						{
							streamWriter.WriteLine("State");
							if (PlatformActivation.activate)
							{
								streamWriter.WriteLine("--activate");
							}
							if (PlatformActivation.show_entitlements)
							{
								streamWriter.WriteLine("--entitlements");
							}
							if (PlatformActivation.check_subscription)
							{
								streamWriter.WriteLine("--check");
							}
							streamWriter.WriteLine("Activation Email: {0}", PlatformActivation.activation_email);
							streamWriter.WriteLine("Activation Company: {0}", PlatformActivation.activation_company);
							streamWriter.WriteLine("Activation Phone: {0}", PlatformActivation.activation_phone);
							streamWriter.WriteLine("Activation Code: {0}", PlatformActivation.activation_code);
							streamWriter.WriteLine("Activation Name: {0}", PlatformActivation.activation_name);
							streamWriter.WriteLine("Error Code: {0}", code);
							streamWriter.WriteLine("Message: {0}", string.Format(message, args));
							streamWriter.WriteLine("Inner Exception: {0}", (innerException == null) ? "null" : innerException.ToString());
						}
						File.WriteAllBytes(PlatformActivation.activation_debug_file, PlatformActivation.Crypto.EncryptAndSign(memoryStream.ToArray()));
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHelper.Warning(9024, "Failed to write debug information to '{0}': {1}", new object[]
				{
					PlatformActivation.activation_debug_file,
					ex.Message
				});
			}
			if (report)
			{
				ErrorHelper.Error(code, innerException, message, args);
			}
		}
		private static void InitPaths()
		{
			PlatformActivation.LicenseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MonoTouch", "License");
		}
		private static bool IsTrial(ProductId product_id)
		{
			return product_id == ProductId.MonoTouchTrial;
		}
		private static LicenseType GetLicenseType(ProductId product_id)
		{
			switch (product_id)
			{
			case ProductId.MonoTouchPersonal:
			case ProductId.MonoTouchEnterprise:
			case ProductId.MonoTouchEnterprise5:
			case ProductId.MonoTouchPromotional:
			case ProductId.MonoTouchAcademic:
				break;
			case ProductId.MAPersonal:
			case ProductId.MAEnterprise:
			case ProductId.MAEnterprise5:
			case ProductId.MAPromotional:
			case ProductId.MAAcademic:
				return LicenseType.None;
			case ProductId.MonoTouchEnterprisePrio:
				return LicenseType.Priority;
			default:
				switch (product_id)
				{
				case ProductId.MonoTouchIndie:
					return LicenseType.Indie;
				case ProductId.MAIndie:
				case ProductId.MAStandard:
					return LicenseType.None;
				case ProductId.MonoTouchStandard:
					break;
				case ProductId.MonoTouchPriority:
					return LicenseType.Priority;
				default:
					return LicenseType.None;
				}
				break;
			}
			return LicenseType.Business;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace BMW.Rheingold.Psdz
{
	[DataContract]
	public class PsdzServiceArgs
	{
		[DataMember]
		public ClientConfigArgs ClientConfigArgs { get; set; }

		[DataMember(IsRequired = false)]
		public string EdiabasBinPath { get; set; }

		[DataMember]
		public int IdleTimeout { get; set; }

		[DataMember]
		public bool IsTestRun { get; set; }

		[DataMember(IsRequired = true)]
		public string JrePath { get; set; }

		[DataMember(IsRequired = false)]
		public string[] JvmOptions { get; set; }

		[DataMember(IsRequired = true)]
		public string PsdzBinaryPath { get; set; }

		[DataMember(IsRequired = true)]
		public string PsdzDataPath { get; set; }

		[DataMember(IsRequired = false)]
		public TestRunParams TestRunParams { get; set; }

		public static PsdzServiceArgs Deserialize(string filename)
		{
			PsdzServiceArgs result;
			using (FileStream fileStream = new FileStream(filename, FileMode.Open))
			{
				using (XmlDictionaryReader xmlDictionaryReader = XmlDictionaryReader.CreateTextReader(fileStream, new XmlDictionaryReaderQuotas()))
				{
					result = (PsdzServiceArgs)new DataContractSerializer(typeof(PsdzServiceArgs)).ReadObject(xmlDictionaryReader, true);
				}
			}
			return result;
		}

		public static void Serialize(string filename, PsdzServiceArgs psdzServiceArgs)
		{
			using (FileStream fileStream = new FileStream(filename, FileMode.Create))
			{
				new DataContractSerializer(typeof(PsdzServiceArgs)).WriteObject(fileStream, psdzServiceArgs);
			}
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "JrePath:             {0}\n", this.JrePath);
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "JvmOptions:          {0}\n", (this.JvmOptions == null) ? "<null>" : string.Join(" ", this.JvmOptions));
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "PsdzBinaryPath:      {0}\n", this.PsdzBinaryPath);
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "PsdzDataPath:        {0}\n", this.PsdzDataPath);
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "EdiabasBinPath:      {0}\n", this.EdiabasBinPath);
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "IdleTimeout:         {0}\n", this.IdleTimeout);
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "IsTestRun:           {0}\n", this.IsTestRun);
			IFormatProvider invariantCulture = CultureInfo.InvariantCulture;
			string format = "\nClientConfigArgs\n{0}";
			ClientConfigArgs clientConfigArgs = this.ClientConfigArgs;
			stringBuilder.AppendFormat(invariantCulture, format, (clientConfigArgs != null) ? clientConfigArgs.ToString() : null);
			return stringBuilder.ToString();
		}
	}
}

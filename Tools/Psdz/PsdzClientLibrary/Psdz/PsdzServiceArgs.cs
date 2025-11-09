using PsdzClientLibrary;
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
    [PreserveSource(Removed = true)]
    [DataContract]
    public class PsdzServiceArgs
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public ClientConfigArgs ClientConfigArgs { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember(IsRequired = false)]
        public string EdiabasBinPath { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int IdleTimeout { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsTestRun { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember(IsRequired = true)]
        public string JrePath { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember(IsRequired = false)]
        public string[] JvmOptions { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember(IsRequired = true)]
        public string PsdzBinaryPath { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember(IsRequired = true)]
        public string PsdzDataPath { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember(IsRequired = false)]
        public TestRunParams TestRunParams { get; set; }

        public static PsdzServiceArgs Deserialize(string filename)
        {
            using (FileStream stream = new FileStream(filename, FileMode.Open))
            {
                XmlDictionaryReader xmlDictionaryReader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas());
                try
                {
                    return (PsdzServiceArgs)new DataContractSerializer(typeof(PsdzServiceArgs)).ReadObject(xmlDictionaryReader, verifyObjectName: true);
                }
                finally
                {
                    ((IDisposable)xmlDictionaryReader)?.Dispose();
                }
            }
        }

        public static void Serialize(string filename, PsdzServiceArgs psdzServiceArgs)
        {
            using (FileStream stream = new FileStream(filename, FileMode.Create))
            {
                new DataContractSerializer(typeof(PsdzServiceArgs)).WriteObject((Stream)stream, (object)psdzServiceArgs);
            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "JrePath:             {0}\n", JrePath);
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "JvmOptions:          {0}\n", (JvmOptions == null) ? "<null>" : string.Join(" ", JvmOptions));
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "PsdzBinaryPath:      {0}\n", PsdzBinaryPath);
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "PsdzDataPath:        {0}\n", PsdzDataPath);
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "EdiabasBinPath:      {0}\n", EdiabasBinPath);
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "IdleTimeout:         {0}\n", IdleTimeout);
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "IsTestRun:           {0}\n", IsTestRun);
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\nClientConfigArgs\n{0}", ClientConfigArgs?.ToString());
            return stringBuilder.ToString();
        }
    }
}

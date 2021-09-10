using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Ecu
{
	[DataContract]
	public class PsdzEcuPdxInfo : IPsdzEcuPdxInfo
	{
		public PsdzEcuPdxInfo()
		{
		}

		public PsdzEcuPdxInfo(bool isSecOcEnabled, bool isSfaEnabled, int certVersion, bool isIpSecEnabled, bool isLcsServicePackSupported, bool isLcsSystemTimeSwitchSupported, bool isCert2018 = false, bool isCert2021 = false, bool isCertEnabled = false, bool isMirrorProtocolSupported = false)
		{
			this.CertVersion = certVersion;
			this.IsSecOcEnabled = isSecOcEnabled;
			this.IsSfaEnabled = isSfaEnabled;
			this.IsIPSecEnabled = isIpSecEnabled;
			this.IsLcsServicePackSupported = isLcsServicePackSupported;
			this.IsLcsSystemTimeSwitchSupported = isLcsSystemTimeSwitchSupported;
			this.IsCert2018 = isCert2018;
			this.IsCert2021 = isCert2021;
			this.IsCertEnabled = isCertEnabled;
			this.IsMirrorProtocolSupported = isMirrorProtocolSupported;
		}

		[DataMember]
		public int CertVersion { get; set; }

		[DataMember]
		public bool IsCert2018 { get; set; }

		[DataMember]
		public bool IsCert2021 { get; set; }

		[DataMember]
		public bool IsCertEnabled { get; set; }

		[DataMember]
		public bool IsSecOcEnabled { get; set; }

		[DataMember]
		public bool IsSfaEnabled { get; set; }

		[DataMember]
		public bool IsIPSecEnabled { get; set; }

		[DataMember]
		public bool IsLcsServicePackSupported { get; set; }

		[DataMember]
		public bool IsLcsSystemTimeSwitchSupported { get; set; }

		[DataMember]
		public bool IsMirrorProtocolSupported { get; set; }

		public override bool Equals(object obj)
		{
			PsdzEcuPdxInfo psdzEcuPdxInfo = obj as PsdzEcuPdxInfo;
			return this.CertVersion == psdzEcuPdxInfo.CertVersion && this.IsCert2018 == psdzEcuPdxInfo.IsCert2018 && this.IsCert2021 == psdzEcuPdxInfo.IsCert2021 && this.IsCertEnabled == psdzEcuPdxInfo.IsCertEnabled && this.IsSecOcEnabled == psdzEcuPdxInfo.IsSecOcEnabled && this.IsSfaEnabled == psdzEcuPdxInfo.IsSfaEnabled && this.IsIPSecEnabled == psdzEcuPdxInfo.IsIPSecEnabled && this.IsLcsServicePackSupported == psdzEcuPdxInfo.IsLcsServicePackSupported && this.IsLcsSystemTimeSwitchSupported == psdzEcuPdxInfo.IsLcsSystemTimeSwitchSupported && this.IsMirrorProtocolSupported == psdzEcuPdxInfo.IsMirrorProtocolSupported;
		}

		public override int GetHashCode()
		{
			return this.ToString().GetHashCode();
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}:{1}:{2}:{3}:{4}:{5}", new object[]
			{
				this.CertVersion,
				this.IsCert2018,
				this.IsCert2021,
				this.IsCertEnabled,
				this.IsSecOcEnabled,
				this.IsSfaEnabled,
				this.IsMirrorProtocolSupported
			});
		}
	}
}

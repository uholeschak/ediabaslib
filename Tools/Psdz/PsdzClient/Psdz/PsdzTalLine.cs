using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz.Model.Tal
{
	[KnownType(typeof(PsdzFscBackup))]
	[KnownType(typeof(PsdzFscDeploy))]
	[KnownType(typeof(PsdzHddUpdate))]
	[KnownType(typeof(PsdzHwDeinstall))]
	[KnownType(typeof(PsdzHwInstall))]
	[KnownType(typeof(PsdzIbaDeploy))]
	[KnownType(typeof(PsdzGatewayTableDeploy))]
	[KnownType(typeof(PsdzCdDeploy))]
	[KnownType(typeof(PsdzPreviousRun))]
	[KnownType(typeof(PsdzSwDelete))]
	[KnownType(typeof(PsdzSwDeploy))]
	[KnownType(typeof(PsdzSFADeploy))]
	[KnownType(typeof(PsdzIdRestore))]
	[KnownType(typeof(PsdzIdBackup))]
	[DataContract]
	[KnownType(typeof(PsdzEcuIdentifier))]
	[KnownType(typeof(PsdzTaCategory))]
	[KnownType(typeof(PsdzBlFlash))]
	public class PsdzTalLine : PsdzTalElement, IPsdzTalElement, IPsdzTalLine
	{
		[DataMember]
		public IPsdzEcuIdentifier EcuIdentifier { get; set; }

		[DataMember]
		public PsdzFscDeploy FscDeploy { get; set; }

		[DataMember]
		public PsdzBlFlash BlFlash { get; set; }

		[DataMember]
		public PsdzIbaDeploy IbaDeploy { get; set; }

		[DataMember]
		public PsdzSwDeploy SwDeploy { get; set; }

		[DataMember]
		public PsdzIdRestore IdRestore { get; set; }

		[DataMember]
		public PsdzSFADeploy SFADeploy { get; set; }

		[DataMember]
		public PsdzTaCategories TaCategories { get; set; }

		[DataMember]
		public IPsdzTaCategory TaCategory { get; set; }
	}
}

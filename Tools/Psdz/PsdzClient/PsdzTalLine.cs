using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
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
		// Token: 0x170000CE RID: 206
		// (get) Token: 0x0600022B RID: 555 RVA: 0x0000296E File Offset: 0x00000B6E
		// (set) Token: 0x0600022C RID: 556 RVA: 0x00002976 File Offset: 0x00000B76
		[DataMember]
		public IPsdzEcuIdentifier EcuIdentifier { get; set; }

		// Token: 0x170000CF RID: 207
		// (get) Token: 0x0600022D RID: 557 RVA: 0x0000297F File Offset: 0x00000B7F
		// (set) Token: 0x0600022E RID: 558 RVA: 0x00002987 File Offset: 0x00000B87
		[DataMember]
		public PsdzFscDeploy FscDeploy { get; set; }

		// Token: 0x170000D0 RID: 208
		// (get) Token: 0x0600022F RID: 559 RVA: 0x00002990 File Offset: 0x00000B90
		// (set) Token: 0x06000230 RID: 560 RVA: 0x00002998 File Offset: 0x00000B98
		[DataMember]
		public PsdzBlFlash BlFlash { get; set; }

		// Token: 0x170000D1 RID: 209
		// (get) Token: 0x06000231 RID: 561 RVA: 0x000029A1 File Offset: 0x00000BA1
		// (set) Token: 0x06000232 RID: 562 RVA: 0x000029A9 File Offset: 0x00000BA9
		[DataMember]
		public PsdzIbaDeploy IbaDeploy { get; set; }

		// Token: 0x170000D2 RID: 210
		// (get) Token: 0x06000233 RID: 563 RVA: 0x000029B2 File Offset: 0x00000BB2
		// (set) Token: 0x06000234 RID: 564 RVA: 0x000029BA File Offset: 0x00000BBA
		[DataMember]
		public PsdzSwDeploy SwDeploy { get; set; }

		// Token: 0x170000D3 RID: 211
		// (get) Token: 0x06000235 RID: 565 RVA: 0x000029C3 File Offset: 0x00000BC3
		// (set) Token: 0x06000236 RID: 566 RVA: 0x000029CB File Offset: 0x00000BCB
		[DataMember]
		public PsdzIdRestore IdRestore { get; set; }

		// Token: 0x170000D4 RID: 212
		// (get) Token: 0x06000237 RID: 567 RVA: 0x000029D4 File Offset: 0x00000BD4
		// (set) Token: 0x06000238 RID: 568 RVA: 0x000029DC File Offset: 0x00000BDC
		[DataMember]
		public PsdzSFADeploy SFADeploy { get; set; }

		// Token: 0x170000D5 RID: 213
		// (get) Token: 0x06000239 RID: 569 RVA: 0x000029E5 File Offset: 0x00000BE5
		// (set) Token: 0x0600023A RID: 570 RVA: 0x000029ED File Offset: 0x00000BED
		[DataMember]
		public PsdzTaCategories TaCategories { get; set; }

		// Token: 0x170000D6 RID: 214
		// (get) Token: 0x0600023B RID: 571 RVA: 0x000029F6 File Offset: 0x00000BF6
		// (set) Token: 0x0600023C RID: 572 RVA: 0x000029FE File Offset: 0x00000BFE
		[DataMember]
		public IPsdzTaCategory TaCategory { get; set; }
	}
}

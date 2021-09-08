using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
	class VcmServiceClient : PsdzClientBase<IVcmService>, IVcmService
	{
		internal VcmServiceClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
		{
		}

		public IPsdzIstufenTriple GetIStufenTripleActual(IPsdzConnection connection)
		{
			return base.CallFunction<IPsdzIstufenTriple>((IVcmService m) => m.GetIStufenTripleActual(connection));
		}

		public IPsdzIstufenTriple GetIStufenTripleBackup(IPsdzConnection connection)
		{
			return base.CallFunction<IPsdzIstufenTriple>((IVcmService m) => m.GetIStufenTripleBackup(connection));
		}

		public IPsdzStandardFa GetStandardFaActual(IPsdzConnection connection)
		{
			return base.CallFunction<IPsdzStandardFa>((IVcmService m) => m.GetStandardFaActual(connection));
		}

		public IPsdzStandardFa GetStandardFaBackup(IPsdzConnection connection)
		{
			return base.CallFunction<IPsdzStandardFa>((IVcmService m) => m.GetStandardFaBackup(connection));
		}

		public IPsdzStandardFp GetStandardFp(IPsdzConnection connection)
		{
			return base.CallFunction<IPsdzStandardFp>((IVcmService m) => m.GetStandardFp(connection));
		}

		public IPsdzStandardSvt GetStandardSvtActual(IPsdzConnection connection)
		{
			return base.CallFunction<IPsdzStandardSvt>((IVcmService m) => m.GetStandardSvtActual(connection));
		}

		public IPsdzVin GetVinFromBackup(IPsdzConnection connection)
		{
			return base.CallFunction<IPsdzVin>((IVcmService m) => m.GetVinFromBackup(connection));
		}

		public IPsdzVin GetVinFromMaster(IPsdzConnection connection)
		{
			return base.CallFunction<IPsdzVin>((IVcmService m) => m.GetVinFromMaster(connection));
		}

		public void WriteFa(IPsdzConnection connection, IPsdzStandardFa standardFa)
		{
			base.CallMethod(delegate (IVcmService m)
			{
				m.WriteFa(connection, standardFa);
			}, true);
		}

		public void WriteFaToBackup(IPsdzConnection connection, IPsdzStandardFa standardFa)
		{
			base.CallMethod(delegate (IVcmService m)
			{
				m.WriteFaToBackup(connection, standardFa);
			}, true);
		}

		public void WriteFp(IPsdzConnection connection, IPsdzStandardFp standardFp)
		{
			base.CallMethod(delegate (IVcmService m)
			{
				m.WriteFp(connection, standardFp);
			}, true);
		}

		public void WriteIStufen(IPsdzConnection connection, string iStufeShipment, string iStufeLast, string iStufeCurrent)
		{
			base.CallMethod(delegate (IVcmService m)
			{
				m.WriteIStufen(connection, iStufeShipment, iStufeLast, iStufeCurrent);
			}, true);
		}

		public void WriteIStufenToBackup(IPsdzConnection connection, string iStufeShipment, string iStufeLast, string iStufeCurrent)
		{
			base.CallMethod(delegate (IVcmService m)
			{
				m.WriteIStufenToBackup(connection, iStufeShipment, iStufeLast, iStufeCurrent);
			}, true);
		}

		public void WriteSvt(IPsdzConnection connection, IPsdzStandardSvt standardSvt)
		{
			base.CallMethod(delegate (IVcmService m)
			{
				m.WriteSvt(connection, standardSvt);
			}, true);
		}
	}
}

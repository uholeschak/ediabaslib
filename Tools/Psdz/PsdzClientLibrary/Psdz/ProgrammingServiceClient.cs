using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Swt;
using BMW.Rheingold.Psdz.Model.Tal;

namespace BMW.Rheingold.Psdz.Client
{
	class ProgrammingServiceClient : PsdzClientBase<IProgrammingService>, IProgrammingService
	{
		internal ProgrammingServiceClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
		{
		}

		public IEnumerable<IPsdzEcuIdentifier> CheckProgrammingCounter(IPsdzConnection connection, IPsdzTal tal)
		{
			return base.CallFunction<IEnumerable<IPsdzEcuIdentifier>>((IProgrammingService m) => m.CheckProgrammingCounter(connection, tal));
		}

		public bool DisableFsc(IPsdzConnection connection, IPsdzEcuIdentifier ecuIdentifier, IPsdzSwtApplicationId swtApplicationId)
		{
			return base.CallFunction<bool>((IProgrammingService m) => m.DisableFsc(connection, ecuIdentifier, swtApplicationId));
		}

		public IDictionary<string, object> ExecuteAsamJob(IPsdzConnection connection, IPsdzEcuIdentifier ecuIdentifier, string jobId, IPsdzAsamJobInputDictionary inputDictionary)
		{
			return base.CallFunction<IDictionary<string, object>>((IProgrammingService m) => m.ExecuteAsamJob(connection, ecuIdentifier, jobId, inputDictionary));
		}

		public long GetExecutionTimeEstimate(IPsdzConnection connection, IPsdzTal tal, bool isParallel)
		{
			return base.CallFunction<long>((IProgrammingService m) => m.GetExecutionTimeEstimate(connection, tal, isParallel));
		}

		public byte[] GetFsc(IPsdzConnection connection, IPsdzEcuIdentifier ecuIdentifier, IPsdzSwtApplicationId swtApplicationId)
		{
			return base.CallFunction<byte[]>((IProgrammingService m) => m.GetFsc(connection, ecuIdentifier, swtApplicationId));
		}

		public void RequestBackupdata(string talExecutionId, string targetDir)
		{
			base.CallMethod(delegate (IProgrammingService m)
			{
				m.RequestBackupdata(talExecutionId, targetDir);
			}, true);
		}

		public IPsdzSwtAction RequestSwtAction(IPsdzConnection connection, IPsdzEcuIdentifier ecuIdentifier, IPsdzSwtApplicationId swtApplicationId, bool periodicalCheck)
		{
			return base.CallFunction<IPsdzSwtAction>((IProgrammingService m) => m.RequestSwtAction(connection, ecuIdentifier, swtApplicationId, periodicalCheck));
		}

		public IPsdzSwtAction RequestSwtAction(IPsdzConnection connection, bool periodicalCheck)
		{
			return base.CallFunction<IPsdzSwtAction>((IProgrammingService m) => m.RequestSwtAction(connection, periodicalCheck));
		}

		public bool StoreFsc(IPsdzConnection connection, byte[] fsc, IPsdzEcuIdentifier ecuIdentifier, IPsdzSwtApplicationId swtApplicationId)
		{
			return base.CallFunction<bool>((IProgrammingService m) => m.StoreFsc(connection, fsc, ecuIdentifier, swtApplicationId));
		}

		public void TslUpdate(IPsdzConnection connection, bool complete, IPsdzSvt svtActual, IPsdzSvt svtTarget)
		{
			base.CallMethod(delegate (IProgrammingService m)
			{
				m.TslUpdate(connection, complete, svtActual, svtTarget);
			}, true);
		}
	}
}

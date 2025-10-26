using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Exceptions;

namespace BMW.Rheingold.Psdz
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IConfigurationService
    {
        bool IsReady();

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        string GetExpectedPsdzVersion();

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        string GetPsdzVersion();

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        string GetRootDirectory();

        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(PsdzRuntimeException))]
        bool ImportPdx(string pathToPdxContainer, string projectName);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        string RequestBaureihenverbund(string baureihe);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        void SetRootDirectory(string rootDir);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        void UnsetRootDirectory();

        RootDirectorySetupResultModel GetRootDirectorySetupResult();
    }
}

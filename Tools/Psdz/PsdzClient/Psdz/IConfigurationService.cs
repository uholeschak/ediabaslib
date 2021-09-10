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
        [FaultContract(typeof(PsdzRuntimeException))]
        [OperationContract]
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

        [FaultContract(typeof(PsdzRuntimeException))]
        [OperationContract]
        void SetRootDirectory(string rootDir);

        [FaultContract(typeof(PsdzRuntimeException))]
        [OperationContract]
        void UnsetRootDirectory();
    }
}

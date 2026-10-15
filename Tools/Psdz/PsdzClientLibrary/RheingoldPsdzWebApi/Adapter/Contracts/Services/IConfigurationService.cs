using PsdzClient;
using System;
using System.ServiceModel;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Exceptions;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Services
{
    [PreserveSource(AttributesModified = true)]
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IConfigurationService
    {
        bool IsReady();
        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        string GetPsdzVersion();
        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        string GetRootDirectory();
        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(PsdzRuntimeException))]
        bool ImportPdx(string pathToPdxContainer, string projectName);
        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        string RequestBaureihenverbund(string baureihe);
        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        void SetRootDirectory(string rootDir);
        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        void UnsetRootDirectory();
        RootDirectorySetupResultModel GetRootDirectorySetupResult();
        [PreserveSource(Added = true, KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        string GetExpectedPsdzVersion();
    }
}
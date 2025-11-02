using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Contracts;
using PsdzClient.Core;
using System.Collections.Generic;
using System.ComponentModel;
using System;

namespace PsdzClient.Core
{
    // ToDo: Check on update
    public interface IIdentVehicle : IReactorVehicle, INotifyPropertyChanged, IVehicleRuleEvaluation
    {
        new string MOTKraftstoffart { get; set; }

        string ChassisCode { get; set; }

        new List<HeatMotor> HeatMotors { get; set; }

        List<string> SxCodes { get; set; }

        TransmissionDataType TransmissionDataType { get; }

        BordnetType BordnetType { get; set; }

        string EBezeichnungUIText { get; set; }

        GenericMotor GenericMotor { get; set; }

        string MainSeriesSgbd { get; set; }

        string MainSeriesSgbdAdditional { get; set; }

        DateTime? C_DATETIME { get; }

        DateTime? LastProgramDate { get; set; }

        string SoftwareId { get; set; }

        string VIN7 { get; }

        string VINType { get; }

        string VehicleModelRecognition { get; set; }

        string TempTypeKeyLeadFromDb { get; set; }

        string TempTypeKeyBasicFromFbm { get; set; }

        IReactorFa GetFaInstance();

        bool HasSA(string checkSA);

        void AddEcu(IIdentEcu ecu);

        bool IsPreE65Vehicle();

        bool IsVehicleWithOnlyVin7();

        IIdentEcu getECU(long? sgAdr);

        IIdentEcu getECUbyECU_GRUPPE(string ECU_GRUPPE);
    }
}
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Contracts;
using PsdzClient.Core;
using System.Collections.Generic;
using System.ComponentModel;
using System;

namespace PsdzClient.Core
{
    public interface IIdentVehicle : IReactorVehicle, INotifyPropertyChanged, IVehicleRuleEvaluation
    {
        new string MOTKraftstoffart { get; set; }

        string ChassisCode { get; set; }

        new List<HeatMotor> HeatMotors { get; set; }

        List<string> sxCodes { get; set; }

        TransmissionDataType TransmissionDataType { get; }

        BordnetType BordnetType { get; set; }

        string EBezeichnungUIText { get; set; }

        GenericMotor GenericMotor { get; set; }

        string MainSeriesSgbd { get; set; }

        string MainSeriesSgbdAdditional { get; set; }

        DateTime? C_DATETIME { get; }

        DateTime? LastProgramDate { get; set; }

        string SoftwareId { get; set; }

        string VehicleModelRecognition { get; set; }

        string TempTypeKeyLeadFromDb { get; set; }

        string TempTypeKeyBasicFromFbm { get; set; }

        IReactorFa GetFaInstance();

        bool HasSA(string checkSA);

        void AddEcu(IIdentEcu ecu);

        bool IsPreE65Vehicle();

        bool IsVehicleWithOnlyVin7();
    }
}
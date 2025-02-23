using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Contracts;
using PsdzClient.Core;
using System.Collections.Generic;
using System.ComponentModel;
using System;

namespace PsdzClientLibrary.Core
{
    public interface IIdentVehicle : IReactorVehicle, INotifyPropertyChanged, IVehicleRuleEvaluation
    {
        new string MOTKraftstoffart { get; set; }

        string ChassisCode { get; set; }

        new List<HeatMotor> HeatMotors { get; set; }

        List<string> sxCodes { get; set; }

        ITransmissionDataType TransmissionDataType { get; }

        BordnetType BordnetType { get; set; }

        string EBezeichnungUIText { get; set; }

        GenericMotor GenericMotor { get; set; }

        string MainSeriesSgbd { get; set; }

        string MainSeriesSgbdAdditional { get; set; }

        DateTime? C_DATETIME { get; }

        DateTime? LastProgramDate { get; set; }

        IReactorFa GetFaInstance();

        bool hasSA(string checkSA);

        void AddEcu(IIdentEcu ecu);

        bool IsPreE65Vehicle();
    }
}
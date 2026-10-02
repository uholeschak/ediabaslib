using PsdzClient;
using PsdzClient.Contracts;
using PsdzClient.Programming;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    public interface IProgrammingSessionData : INotifyPropertyChanged
    {
        ObservableCollection<SecureFeatureData> CurrentSecureFeatures { get; }

        IProgrammingStateData CurrentState { get; }

        IEcuProgrammingInfosData EcuProgrammingInfos { get; }

        ObservableCollection<EnablingCodeData> EnablingCodes { get; }

        IList<IFSCProvided> RepEnablingCodes { get; set; }

        bool IsActive { get; }

        bool IsCheckedCodingForAllEcus { get; }

        bool IsNotificationModeSilent { get; }

        bool IsProgressIndeterminate { get; }

        bool IsSuspended { get; }

        bool IsValid { get; }

        DateTime LastActive { get; }

        ProgrammingNote Note { get; }

        string ProgressLabel { get; }

        double ProgressValue { get; }

        TimeSpan TimeLeft { get; }

        [PreserveSource(Hint = "ITherapyPlanData", Placeholder = true)]
        PlaceholderType TherapyPlan { get; }

        string VehicleOrder { get; }

        bool WithErrorOrMessageForIndustrialCustomer { get; }

        IEnumerable<int> ZgwEcusToRepair { get; set; }

        bool ZgwIsInBootMode { get; set; }

        bool CanSelectCodingForAllEcus();
    }
}

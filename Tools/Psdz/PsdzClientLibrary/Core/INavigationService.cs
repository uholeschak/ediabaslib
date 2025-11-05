namespace PsdzClient.Core
{
    public enum TabName
    {
        None,
        Page_IstaMenu,
        Page_DocumentViewer,
        Page_Measurement,
        Start,
        Operations,
        Operations_New,
        Operations_New_Vin,
        Operations_New_ReadOutVehicleData,
        Operations_New_BasicFeatures,
        Operations_New_TypeKeyInput,
        Operations_Finished,
        Operations_Finished_OperationList,
        Operations_Active,
        Operations_Active_OperationList,
        Operations_IRAP,
        Operations_IRAP_WorkshopClient,
        VehicleInformation,
        VehicleInformation_VehicleDetails,
        VehicleInformation_VehicleEquipment,
        VehicleInformation_VehicleEquipment_Details,
        VehicleInformation_RepairHistory,
        VehicleInformation_ControlUnitTree,
        VehicleInformation_ControlUnitList,
        VehicleInformation_OperationsReport,
        VehicleInformation_OperationsReport2,
        VehicleInformation_ActionList,
        VehicleInformation_ProcessControlUnits,
        VehicleInformation_InformationByServiceCase,
        VehicleInformation_WiringDiagramBrowser,
        VehicleManagement,
        VehicleManagement_ProgrammingEncoding,
        VehicleManagement_ProgrammingEncoding_Measures,
        VehicleManagement_RepairMaintenance,
        VehicleManagement_RepairMaintenance_ProductStructure,
        VehicleManagement_RepairMaintenance_TextSearch,
        VehicleManagement_Troubleshooting,
        VehicleManagement_Troubleshooting_FaultMemory,
        VehicleManagement_Troubleshooting_FaultMemory_ExpertMode,
        VehicleManagement_Troubleshooting_FaultPattern,
        VehicleManagement_Troubleshooting_FunctionStructure,
        VehicleManagement_Troubleshooting_ComponentStructure,
        VehicleManagement_Troubleshooting_NED,
        VehicleManagement_Troubleshooting_TextSearch,
        VehicleManagement_Troubleshooting_SaeFaultCodeInput,
        VehicleManagement_ServiceFunctions,
        VehicleManagement_ServiceFunctions_ServiceFunctions,
        VehicleManagement_SoftwareUpdate,
        VehicleManagement_SoftwareUpdate_Comfort,
        VehicleManagement_SoftwareUpdate_Advanced,
        VehicleManagement_SoftwareUpdate_AdditionalSW,
        VehicleManagement_EcuExchange,
        VehicleManagement_EcuExchange_PreExchange,
        VehicleManagement_EcuExchange_PostExchange,
        VehicleManagement_VehicleModification,
        VehicleManagement_VehicleModification_Retrofit,
        VehicleManagement_VehicleModification_Conversion,
        VehicleManagement_VehicleModification_CodingConversion,
        VehicleManagement_VehicleModification_BackConversion,
        VehicleManagement_VehicleModification_CodingBackConversion,
        VehicleManagement_VehicleModification_ImmediateActions,
        VehicleManagement_VehicleModification_NewImmediateActions,
        ServicePlan,
        ServicePlan_HitList,
        ServicePlan_TestPlan,
        ServicePlan_ProgrammingPlan,
        ServicePlan_ProgrammingPlan_TherapyPlan,
        ServicePlan_ProgrammingPlan_TherapyPlanReport,
        WorkshopOperatinFluids,
        WorkshopOperatinFluids_WorkshopEquipment,
        WorkshopOperatinFluids_OperationFluids,
        WorkshopOperatinFluids_MobileService,
        WorkshopOperatinFluids_TextSearch,
        WorkshopOperatinFluids_HitList,
        MeasuringDevices,
        MeasuringPage,
        MeasuringDevices_StartPage
    }

    public enum IstaNavigationOriginCacheKey
    {
        HitList,
        TestPlan,
        Measurement
    }

    public interface INavigationService
    {
        TabName CurrentTab { get; }

        void NavigateTo(TabName target);
        void NavigateToLastPage();
        void SetTabToOriginCache(IstaNavigationOriginCacheKey origin);
        void SetTabToOriginCache(IstaNavigationOriginCacheKey origin, TabName tab);
        TabName GetTabFromOriginCache(IstaNavigationOriginCacheKey origin);
    }
}
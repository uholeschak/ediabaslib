using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using BMW.Rheingold.ISTA.CoreFramework.ServiceDialoge.Multisession;
using PsdzClient;

namespace BMW.Rheingold.Module.ISTA
{
    public class ServiceDialogConfiguration
    {
        private static Dictionary<string, ServiceDialogConfiguration> registry;
        private static Dictionary<decimal, string> controlId2Name;
        public ITextContentManager TextCollection { get; set; }
        public decimal ControlId { get; private set; }
        public string Name { get; private set; }
        public Type DialogType { get; private set; }
        public Type DialogUIType { get; private set; }
        public bool IsShowingGui { get; private set; }
        public Type ControllerType { get; private set; }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        private ServiceDialogConfiguration(decimal id, string name, Type dialog, Type dialogUI, Type controller, bool gui)
        {
            if (dialog == null)
            {
                throw new ArgumentException("Parameter dialog must not be null.");
            }

            if (name == null)
            {
                Name = dialog.Name;
            }
            else
            {
                Name = name;
            }

            if (controller != null && !typeof(IServiceDialog).IsAssignableFrom(controller))
            {
                throw new ArgumentException($"The registered controller type {controller} of service dialog {Name} is no IServiceDialog.");
            }

            ControlId = id;
            DialogType = dialog;
            DialogUIType = dialogUI;
            ControllerType = controller;
            IsShowingGui = gui;
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        static ServiceDialogConfiguration()
        {
            registry = new Dictionary<string, ServiceDialogConfiguration>();
            controlId2Name = new Dictionary<decimal, string>();
            Register(51946891m, "AdapterServiceDlg", typeof(AdapterServiceDlgImpl), typeof(IstaOperationServiceDialogUi), null);
            Register(53531955979m, "Dialog_1Balkenhorizontal", typeof(Dialog_1Balkenhorizontal), typeof(BalkenHorizontalDlgUi), null);
            Register(53536324363m, "Dialog_2Balkenhorizontal", typeof(Dialog_2Balkenhorizontal), typeof(BalkenHorizontalDlgUi), null);
            Register(53600486795m, "Dialog_3Balkenhorizontal", typeof(Dialog_3Balkenhorizontal), typeof(BalkenHorizontalDlgUi), null);
            Register(53600523019m, "Dialog_4Balkenhorizontal", typeof(Dialog_4Balkenhorizontal), typeof(BalkenHorizontalDlgUi), null);
            Register(61002193291m, null, typeof(Dialog_Zuendungstatus), typeof(DialogZuendungstatusCmd), hasGui: true);
            Register(52655243m, "DTC_ANZEIGE_DYN", typeof(DtcAnzeigeDynImpl), typeof(DtcAnzeigeDynUi), null);
            Register(51939083m, "ECUKOMServiceDlg", typeof(EcuKomServiceDlgImpl), typeof(IstaOperationServiceDialogUi), null);
            //[-] Register(51888523m, "EnterServiceDlg", typeof(EnterServiceDlgImpl), typeof(IstaOperationServiceDialogUi), null);
            //[-] Register(43608062091m, null, typeof(Fahrzeugauftrag_ausISTA_UX), typeof(FahrzeugauftragAusIstaUxCmd), hasGui: false);
            //[-] Register(68025234187m, "FKB_Anzeige", typeof(FKB_AnzeigeServiceDlgImpl), typeof(DtcAnzeigeDynUi), null);
            //[-] Register(52637835m, null, typeof(FS_LISTE_ISTA), typeof(FsListeIstaCmd), hasGui: false);
            //[-] Register(53971007243m, null, typeof(FS_LISTE_ISTA_FORT), typeof(FsListeIstaFortCmd), hasGui: false);
            //[-] Register(52683531m, null, typeof(FS_LISTE_ISTA_KURZ), typeof(FsListeIstaKurzCmd), hasGui: false);
            //[-] Register(71493731211m, null, typeof(FZG_Kom_IDENT), typeof(FZG_Kom_IDENTCmd), hasGui: true);
            //[-] Register(73271865611m, null, typeof(Identifikationstyp), typeof(IdentifikationstypCmd), hasGui: false);
            //[-] Register(68907559435m, null, typeof(IMIB_TB_HVA), typeof(IMIB_TB_HVACmd), hasGui: true);
            //[-] Register(68909781899m, "IMIB_TB_SYSINFO", typeof(IMIB_TB_Sysinfo), typeof(ImibTbSysinfoCmd), hasGui: true);
            //[-] Register(70271166731m, null, typeof(ISTA_Kontext_Ausstattung_Auswertung), typeof(IstaKontextAusstattungAuswertungCmd), hasGui: false);
            //[-] Register(69973561867m, null, typeof(ISTA_Kontext_Ausstattung_Daten), typeof(IstaKontextAusstattungDatenCmd), hasGui: false);
            //[-] Register(68072409611m, null, typeof(ISTA_Kontext_DTC_Auswertung), typeof(IstaKontextDtcAuswertungCmd), hasGui: false);
            //[-] Register(67207569803m, null, typeof(ISTA_Kontext_DTC_Daten), typeof(IstaKontextDtcDatenCmd), hasGui: false);
            //[-] Register(69913852939m, null, typeof(ISTA_Kontext_FZG_Daten), typeof(ISTA_Kontext_FZG_DatenCmd), hasGui: false);
            //[-] Register(70704210955m, null, typeof(ISTA_Zeit), typeof(IstaZeitCmd), hasGui: false);
            //[-] Register(51892235m, "MeasuringServiceDlg", typeof(MeasuringServiceDlgImpl), typeof(IstaOperationServiceDialogUi), typeof(MeasuringServiceDlgCmd));
            //[-] Register(68904586891m, "Meldung_Neu", typeof(MeldungNeuImpl), typeof(IstaOperationServiceDialogUi), null);
            Register(51915403m, "MessageServiceDlg", typeof(MessageServiceDlgImpl), typeof(IstaOperationServiceDialogUi), typeof(MessageServiceDlgCmd));
            //[-] Register(57410849547m, "Dialog_Messwertanzeige_H", typeof(MesswertanzeigeHorizontalImpl), typeof(IstaOperationServiceDialogUi), null);
            //[-] Register(55479872651m, "Dialog_Messwertanzeige_V", typeof(MesswertanzeigeVerticalImpl), typeof(IstaOperationServiceDialogUi), null);
            //[-] Register(53397493003m, null, typeof(Messwerte_Generator1), typeof(MesswerteGenerator1Cmd), hasGui: false);
            //[-] Register(51884811m, "OsziServiceDlg", typeof(OsziServiceDlgModule), typeof(OsziServiceDlgModuleCmd), hasGui: true);
            //[-] Register(-1m, "OsziServiceDlgPage2", typeof(OsziServiceDlgImpl), typeof(IstaOperationServiceDialogUi), null);
            //[-] Register(68915699723m, null, typeof(Qualität), typeof(QualityCmd), hasGui: true);
            //[-] Register(51695499m, "ReserveIMIBAdapter", typeof(string), typeof(ReserveImibAdapterCmd), hasGui: true);
            //[-] Register(51937067403m, "RueckmeldeDialog", typeof(RueckmeldeDlgImpl), typeof(IstaOperationServiceDialogUi), null);
            //[-] Register(916704907m, null, typeof(SetSuspicionToChildrenServiceDlg), typeof(SetSuspicionToChildrenServiceDlgCmd), hasGui: false);
            //[-] Register(52672267m, null, typeof(SYS_VAR_ISTA), typeof(SysVarIstaCmd), hasGui: false);
            //[-] Register(38122360331m, "Typmerkmal_ausISTA_UX", typeof(Typmerkmale_ausISTA_UX), typeof(Typmerkmal_ausISTA_UXCmd), hasGui: false);
            //[-] Register(52677899m, null, typeof(TYPMERKMAL_ISTA), typeof(TypmerkmalIstaCmd), hasGui: false);
            //[-] Register(51872651m, "VehicleStateServiceDlg", typeof(VehicleStateServiceDlgImpl), typeof(IstaOperationServiceDialogUi), null);
            //[-] Register(51919115m, "ZaehlerServiceDlg", typeof(ZaehlerServiceDlgImpl), typeof(IstaOperationServiceDialogUi), null);
            //[-] Register(54870936203m, "DatumeingabeDlg", typeof(DatumeingabeDlgImpl), typeof(IstaOperationServiceDialogUi), null);
            Register(51911691m, "QuestionSelectServiceDlg", typeof(QuestionSelectServiceDlgDefaultImpl), typeof(IstaOperationServiceDialogUi), null);
            Register(13628358027m, "QuestionSelectServiceDlg_20", typeof(QuestionSelectServiceDlgDefaultImpl), typeof(IstaOperationServiceDialogUi), null);
            //[-] Register(51878795m, "QuestionServiceDlg", typeof(QuestionServiceDlgImpl), typeof(IstaOperationServiceDialogUi), null);
            //[-] Register(20000104329472m, null, typeof(IMIB_BT), null, hasGui: true);
            //[-] Register(20000139577811m, "IMIB_PLUGIN", typeof(ImibGenericServiceDlgImpl), typeof(ImibGenericServiceDlgUi), typeof(ImibGenericServiceCmd));
            //[-] Register(20000104329471m, null, typeof(IMIB_USB), null, hasGui: true);
            //[-] Register(20000104538851m, null, typeof(IMIB_ZAEHLER), null, hasGui: true);
            //[-] Register(20000104329474m, null, typeof(xS_LESEN_DETAIL), null, hasGui: false);
            //[-] Register(20000098022441m, "Fahrzeuginterface", typeof(string), typeof(FahrzeuginterfaceCmd), hasGui: false);
            Register(20000093559516m, "DialogKurvenDisplay", typeof(KurvendisplayDlgImpl), typeof(IstaOperationServiceDialogUi), null);
            Register(20000138655401m, "MehrfachAuswahlDlg", typeof(MehrfachAuswahlDlgImpl), typeof(IstaOperationServiceDialogUi), null);
            //[-] Register(20000100664261m, null, typeof(Vorgangshistorie), null, hasGui: false);
            //[-] Register(20000161161141m, null, typeof(PDIServiceHistory), null, hasGui: false);
            //[-] Register(20000169514081m, null, typeof(AirServiceHistory), null, hasGui: false);
            //[-] Register(20000361414781m, "Rdc Trigger Tool", typeof(RdcTriggerToolDialog), typeof(IstaOperationServiceDialogUi), null);
            //[-] Register(-2m, "QuickCommandMeasuringServiceDlg", typeof(QuickCommandMeasuringServiceDlgImpl), typeof(IstaOperationServiceDialogUi), null);
            Register(20000364853784m, "NewDialogKurvenDisplay", typeof(NewKurvendisplayDlgImpl), typeof(IstaOperationServiceDialogUi), null);
            //[-] Register(20000548205711m, "Balkendisplay", typeof(HealthIndicatorDlgImpl), typeof(IstaOperationServiceDialogUi), null);
            //[-] Register(20000590095471m, "MultiselectServiceDlg", typeof(MultiselectServiceDlgImpl), typeof(IstaOperationServiceDialogUi), null);
            //[-] Register(20001106707371m, "EFuseDlg", typeof(EFuseDlgImpl), typeof(IstaOperationServiceDialogUi), null);
        }

        private static void Register(decimal controlId, string name, Type dialog, Type dialogUI, Type controller)
        {
            Register(controlId, name, dialog, dialogUI, controller, dialogUI != null);
        }

        private static void Register(decimal controlId, string name, Type dialog, Type dialogUI, Type controller, bool hasGui)
        {
            ServiceDialogConfiguration serviceDialogConfiguration = new ServiceDialogConfiguration(controlId, name, dialog, dialogUI, controller, hasGui);
            if (registry.ContainsKey(serviceDialogConfiguration.Name))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Service dialog {0} is already registered to {1}.", serviceDialogConfiguration, registry[serviceDialogConfiguration.Name]));
            }

            registry.Add(serviceDialogConfiguration.Name, serviceDialogConfiguration);
            controlId2Name.Add(serviceDialogConfiguration.ControlId, serviceDialogConfiguration.Name);
        }

        private static void Register(decimal controlId, string name, Type dialog, Type controller, bool hasGui)
        {
            Register(controlId, name, dialog, null, controller, hasGui);
        }

        public static ServiceDialogConfiguration GetRegisteredConfiguration(string serviceDialog)
        {
            if (!registry.ContainsKey(serviceDialog))
            {
                throw new ArgumentException($"No definition registered for service dialog name {serviceDialog}.");
            }

            return registry[serviceDialog];
        }

        public static ServiceDialogConfiguration GetRegisteredConfigurationById(decimal serviceDialogId)
        {
            if (!controlId2Name.ContainsKey(serviceDialogId))
            {
                return null;
            }

            return GetRegisteredConfiguration(controlId2Name[serviceDialogId]);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("[");
            stringBuilder.Append(Name).Append(",");
            stringBuilder.Append(DialogType).Append(",");
            stringBuilder.Append(DialogUIType).Append(",");
            stringBuilder.Append(ControllerType).Append(",");
            stringBuilder.Append("]");
            return stringBuilder.ToString();
        }

        internal static ServiceDialogConfiguration GetDefault(string serviceDialog)
        {
            ServiceDialogConfiguration registeredConfiguration = GetRegisteredConfiguration("MessageServiceDlg");
            registeredConfiguration.Name = serviceDialog;
            return registeredConfiguration;
        }
    }
}
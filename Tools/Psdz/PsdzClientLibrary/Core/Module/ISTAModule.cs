using BMW.Authoring;
using BMW.Authoring.API;
using BMW.Authoring.API.Interface.Rita;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.CoreFramework.EnergySettings;
using BMW.Rheingold.CoreFramework.Feedback;
using BMW.Rheingold.CoreFramework.Module;
using BMW.Rheingold.ISTA.CoreFramework;
using BMW.Rheingold.ISTA.CoreFramework.SOCAccessor;
using BMW.Rheingold.Measurement.Common;
using BMW.Rheingold.Psdz;
using PBMW.Rheingold.CoreFramework.Contracts;
using PsdzClient;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using PsdzClient.Utility;
using PsdzClientLibrary.Core.Module;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

#pragma warning disable CS0649, CS0219, CS0809
namespace BMW.Rheingold.Module.ISTA
{
    [AuthorAPIFlowBase]
    [PreserveSource(Hint = "No update", SuppressWarning = true)]
    public abstract class ISTAModule : IstaModuleBase, IAuthoringModule, IHideObjectMembers
    {
        //private ProgrammingSessionProxy programmingSessionProxy;

        private object onTheFlyCompileLock = new object();

        private readonly IFeedbackViewHeaderTitleHelper feedbackViewHeaderHelper;

        //[EditorBrowsable(EditorBrowsableState.Never)]
        //internal ModuleParameter __RheinGoldCoreModuleParameters__;

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected IModuleExecutionParent _globalTabModuleISTA;

        protected const string InParameterFastaName = "FASTA";

        protected const string InParameterMethodname = "methodname";

        protected const string InParameterTestModuleCacheName = "InParameterTestModuleCache";

        protected string CallingModule = "";

        private Dictionary<string, string> testModuleCache;

        private List<string> serviceCodesHandledInLoop = new List<string>();

        private IFFMDynamicResolver ffmResolver;

        private IVehicleStateManager vehicleStateManager;

        private INOPProvisioning nopProvisioning;

        private IVPSProvisioning vpsProvisioning;

        private IEnergySettings _energySettings;

        private IPersistency persistency;

        private INavigationMapProcessor navigationMapProcessor;

        private IAppSessionContext appSessionContext;

        private ILogger logger;

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected IDiagnosticDeviceResult dscSynchronResult;

        private IVehicleAdapters vehicleAdapters;

        private Vehicle vehicle;

        private Lazy<IVehicle> authoringVehicle;

        private ISPEUserInterface _SPEUserInterface;

        private static int instances;

        private IEcuKomStatement ecuKomStatement;

        private IInputListener inputListener;

        //private readonly Lazy<IPSDataProvider> ipsDataProvider = new Lazy<IPSDataProvider>();

        private ISfaHandler sfaHandler;

        private IRitaFunctionsProvider ritaFunctionsProvider;

        private IVehicleContext vehicleContext;

        private ISOCAccessor _ISOCAccessor;

        internal IModuleExecutionParent GlobalTabModuleISTA => _globalTabModuleISTA;

        [EditorBrowsable(EditorBrowsableState.Always)]
        public override ILogger Logger => logger;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected ISWTProcessor SWTProcessor
        {
            get
            {
                if (logic != null)
                {
                    return logic.SWTProcessor;
                }
                //[-] IRheingoldApp rheingoldApp = Application.Current as IRheingoldApp;
                //[-] ProtocolRheingoldApiUsage("IRheingoldApp");
                //[-] if (rheingoldApp != null && rheingoldApp.ILogic != null)
                //[-] {
                //[-] ILogic iLogic = rheingoldApp.ILogic;
                //[-] if (iLogic != null)
                //[-] {
                //[-] return iLogic.SWTProcessor;
                //[-] }
                //[-] }
                Log.Warning("ISTAModule.get_SWTProcessor()", "logic was null; maybe your testmodule will die");
                return null;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public override Vehicle Vehicle => vehicle;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public IFFMDynamicResolver FFMDynamicResolver => ffmResolver;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public IVehicle AuthoringVehicle => authoringVehicle?.Value;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public IStartMeasurementServiceServer MeasurementLauncher { get; private set; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override IProtocolBasic FastaProtocoler
        {
            get
            {
                if (base.FastaGrouping != null)
                {
                    IProtocolBasic protocolingInstance = base.FastaGrouping.ProtocolingInstance;
                    if (protocolingInstance != null)
                    {
                        return protocolingInstance;
                    }
                }
                Log.Error("FastaProtocoler_get", "FastaProtocoler is null, returning instance of class Fasta2ServiceNop instead.");
                //[-] return new Fasta2ServiceNop();
                //[+] return null;
                return null;
            }
        }
#if false
        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public IServiceDialogFactory Factory { get; private set; }
#endif
        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public override IFFMDynamicResolver FFMResolver => ffmResolver;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        [Obsolete("ecuKom will be Removed. Please use EcuKom")]
        public override IEcuKom ecuKom => EcuKom;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public new IEcuKom EcuKom
        {
            get
            {
                if (logic != null)
                {
                    return logic.EcuKom;
                }
                //[-] IRheingoldApp rheingoldApp = Application.Current as IRheingoldApp;
                //[-] ProtocolRheingoldApiUsage("IRheingoldApp");
                //[-] if (rheingoldApp != null && rheingoldApp.ILogic != null)
                //[-] {
                //[-] ILogic iLogic = rheingoldApp.ILogic;
                //[-] if (iLogic != null)
                //[-] {
                //[-] return iLogic.EcuKom;
                //[-] }
                //[-] }
                Log.Warning("ISTAModule.get_ecuKom()", "ecuKom used but logic was null; maybe your testmodule will die");
                return null;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public ISfaHandler SfaHandler
        {
            get
            {
                if (sfaHandler == null)
                {
                    //[-] sfaHandler = new SfaHandler(Vehicle, CommonServiceHelper.GetIdentificationStringOfClient(), CommonServiceHelper.GetIdentificationStringOfSystem(), new SfaService(logic.BackendCallWatchDog, new ErrorManager(FastaProtocoler)), EcuKomStatement, FastaProtocoler);
                }
                return sfaHandler;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public IRitaFunctionsProvider RitaFunctionsProvider
        {
            get
            {
                if (ritaFunctionsProvider == null)
                {
                    //[-] ritaFunctionsProvider = new RitaFunctionsProvider();
                }
                return ritaFunctionsProvider;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override IEcuKomStatement EcuKomStatement
        {
            get
            {
                if (ecuKomStatement == null)
                {
                    //[-] ecuKomStatement = new EcuKomStatement(this);
                }
                return ecuKomStatement;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override IInputListener InputListener
        {
            get
            {
                if (inputListener == null)
                {
                    //[-] inputListener = new InputListener(logic as Logic);
                }
                return inputListener;
            }
        }
#if false
        [Obsolete("use new Method over the Authoring BackendCommunication GetServiceRideDataHandler")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected IIPSDataProvider IPSDataProvider => ipsDataProvider.Value;
#endif
        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected ISPEUserInterface SPEUserInterface
        {
            get
            {
                if (_SPEUserInterface == null)
                {
                    //[-] _SPEUserInterface = new SPEUserInterface(this);
                }
                return _SPEUserInterface;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected IVehicleAdapters VehicleAdapters
        {
            get
            {
                if (vehicleAdapters != null)
                {
                    return vehicleAdapters;
                }
                //[-] vehicleAdapters = new VehicleAdapters(Vehicle);
                return vehicleAdapters;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override IVehicleContext VehicleContext => vehicleContext;
#if false
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override IDealerData DealerData => logic.Dealer.DealerData;
#endif
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override ISOCAccessor SOCAccessor => _ISOCAccessor;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override ISOCAccessor Contexts => _ISOCAccessor;

        public override IModule ModuleData => _globalTabModuleISTA.ModuleData;

        protected IProgrammingSession ProgrammingSession
        {
            get
            {
                if (logic != null)
                {
                    //[-] if (programmingSessionProxy == null || programmingSessionProxy.Fasta != FastaProtocoler || logic.ProgrammingSession != programmingSessionProxy.ProgrammingSession)
                    //[-] {
                    //[-] programmingSessionProxy = new ProgrammingSessionProxy(logic.ProgrammingSession, FastaProtocoler);
                    //[-] }
                    //[-] return programmingSessionProxy;
                }
                Log.Warning("ISTAModule.get_ProgrammingSession", "programming session handle in logic or logic was null");
                return null;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected IVehicleStateManager VehicleStateManager => vehicleStateManager;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override IEnergySettings EnergySettings
        {
            get
            {
                if (_energySettings == null)
                {
                    //[-] _energySettings = new EnergySettings();
                }
                return _energySettings;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override IAppSessionContext AppSessionContext
        {
            get
            {
                //[-] ProtocolRheingoldApiUsage("IAppSessionContext");
                return appSessionContext;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override INOPProvisioning NOPProvisioning => nopProvisioning;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override IVPSProvisioning VPSProvisioning => vpsProvisioning;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override IPersistency Persistency => persistency;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override INavigationMapProcessor NavigationMapProcessor
        {
            get
            {
                if (navigationMapProcessor == null)
                {
                    //[-] navigationMapProcessor = new NavigationMapProcessor(EcuKom, logic);
                }
                return navigationMapProcessor;
            }
        }

        IDealerData IAuthoringModule.DealerData
        {
            get
            {
                //[-] return logic.Dealer.DealerData;
                //[+] return null;
                return null;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
#if false
        public override IDatabaseProvider DBProvider => DatabaseProviderFactory.Instance;
#endif
        public IProtocolBasicBase FastaProtocolerBase => FastaProtocoler;

        public SessionInfo SessionInfo => ClientContext.GetClientContext(vehicle)?.SessionInfo;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public ISTAModule()
            : this(null)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public ISTAModule(ITextContentManager textContentManager)
        {
            instances++;
            Log.Info("ISTAModule.ISTAModule()", "called for {0}, total instances: {1}", GetType().Name, instances);
            CallingModule = GetType().Name;
            base.textContentManager = textContentManager;
            //[-] Factory = new ServiceDialogFactory();
            //[-] logger = new Logger(GetType().Name);
            //[-] vehicleStateManager = new VehicleStateManager(this);
            //[-] nopProvisioning = new NOPProvisioning();
            //[-] vpsProvisioning = new VPSProvisioning();
            //[-] persistency = new Persistency();
            navigationMapProcessor = null;
            feedbackViewHeaderHelper = ServiceLocator.Current.GetService<IFeedbackViewHeaderTitleHelper>();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        ~ISTAModule()
        {
            instances--;
            Log.Info("ISTAModule.~ISTAModule()", "called for {0}, total instances: {1}", GetType().Name, instances);
            //[-] Factory = new ServiceDialogFactory();
            InputListener?.Dispose();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public override void Dispose()
        {
            vehicle = null;
            vehicleContext = null;
            vehicleAdapters = null;
            if (SuspiciuosItems != null)
            {
                try
                {
                    SuspiciuosItems.Clear();
                }
                catch (Exception exception)
                {
                    Log.ErrorException("ISTAModule.Dispose()", exception);
                }
            }
            SuspiciuosItems = null;
            logic = null;
            ffmResolver = null;
            //[-] Factory = null;
            dscSynchronResult = null;
            _SPEUserInterface = null;
            _globalTabModuleISTA = null;
            _globalModuleOutParameter = null;
            _globalModuleInParameter = null;
            _globalModuleInAndOutParameter = null;
            //[-] __RheinGoldCoreModuleParameters__ = null;
        }

        private bool HasValue(object valueObject)
        {
            bool flag = true;
            if (valueObject is string)
            {
                return !string.IsNullOrEmpty(valueObject as string);
            }
            return valueObject != null;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected void __MessagePopup(ITextContent text)
        {
            IList<LocalizedText> textForUI = text.GetTextForUI(logic.Lang);
            IList<LocalizedText> list = new FormatedData("#Info").Localize(logic.Lang);
            InteractionMessageModel model = new InteractionMessageModel(list[0].TextItem, textForUI[0].TextItem);
            DateTime now = DateTime.Now;
            logic.Services.InteractionService.Register(model);
            IProtocolBasic fastaProtocoler = FastaProtocoler;
            if (fastaProtocoler != null)
            {
                //[-] IAction<IUiDialog> action = fastaProtocoler.CreateAndAddUiDialogFromServiceProgram("MessagePopup", base.LastCallingMethod);
                //[-] action.StartTime = now;
                //[-] action.SpecialAction.SetTitle(list);
                //[-] action.SpecialAction.CreateAndAddMessageText(textForUI);
            }
            else
            {
                Log.Error("ISTAModule.__MessagePopup", "No FASTA protocoling possible.");
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected void __MessagePopup(ITextLocator text)
        {
            if (text != null)
            {
                __MessagePopup(text.TextContent);
            }
            else
            {
                Log.Warning("ISTAModule.__MessagePopup()", "text was null");
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected string __EnterPopup(ITextLocator pre, ITextLocator post)
        {
            ITextContent textContent;
            if (pre != null)
            {
                textContent = pre.TextContent;
            }
            else
            {
                ITextContent textContent2 = new TextContent(string.Empty);
                textContent = textContent2;
            }
            ITextContent pre2 = textContent;
            ITextContent textContent3;
            if (post != null)
            {
                textContent3 = post.TextContent;
            }
            else
            {
                ITextContent textContent2 = new TextContent(string.Empty);
                textContent3 = textContent2;
            }
            ITextContent post2 = textContent3;
            return __EnterPopup(pre2, post2);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected string __EnterPopup(ITextContent pre, ITextContent post)
        {
            DateTime now = DateTime.Now;
            ITextContent textContent = pre ?? new TextContent(string.Empty);
            ITextContent textContent2 = post ?? new TextContent(string.Empty);
            //[-] InteractionEnterModel interactionEnterModel = new InteractionEnterModel();
            //[-] interactionEnterModel.PriorText = textContent.GetTextForUI(logic.Lang)[0].TextItem;
            //[-] interactionEnterModel.SuccessorText = textContent2.GetTextForUI(logic.Lang)[0].TextItem;
            //[-] logic.Services.InteractionService.Register(interactionEnterModel);
            //[-] string output = interactionEnterModel.Response.UserInput;
            if (FastaProtocoler != null)
            {
                //[-] IAction<IUiDialog> action = FastaProtocoler.CreateAndAddUiDialogFromServiceProgram("EnterPopup", "TODO");
                //[-] action.StartTime = now;
                IList<LocalizedText> messageTextList = ((!string.IsNullOrEmpty(textContent.FormattedText) && textContent.FormattedText.Length != 0) ? __Text().TextContent.Concat(textContent).Concat(textContent2).GetTextForUI(logic.Lang) : textContent2.GetTextForUI(logic.Lang));
                //[-] action.SpecialAction.CreateAndAddMessageText(messageTextList);
                //[-] if (output != null)
                //[-] {
                //[-] List<LocalizedText> list = new List<LocalizedText>();
                //[-] list.AddRange(logic.Lang.Select((string x) => new LocalizedText(output, x)));
                //[-] action.SpecialAction.AddAnswer(list, "UserInput");
                //[-] }
            }
            else
            {
                Log.Error("ISTAModule.__EnterPopup()", "No ModuleStep available in FASTA");
            }
            //[-] return output;
            //[+] return string.Empty;
            return string.Empty;
        }
#if false
        private int AskQuestion(ITextContent question, string answer0, string answer1, string answer2, int result0, int result1, int result2, string title, int size)
        {
            QuestionPopupDialogAnswer answerLeft = null;
            QuestionPopupDialogAnswer answerMiddle = null;
            QuestionPopupDialogAnswer answerRight = null;
            if (result2 >= 0)
            {
                answerRight = new QuestionPopupDialogAnswer(answer2, result2);
                if (result1 >= 0)
                {
                    if (result0 >= 0)
                    {
                        answerLeft = new QuestionPopupDialogAnswer(answer0, result0);
                        answerMiddle = new QuestionPopupDialogAnswer(answer1, result1);
                    }
                    else
                    {
                        answerLeft = new QuestionPopupDialogAnswer(answer1, result1);
                    }
                }
                else if (result0 >= 0)
                {
                    answerLeft = new QuestionPopupDialogAnswer(answer0, result0);
                }
            }
            else if (result1 >= 0)
            {
                answerRight = new QuestionPopupDialogAnswer(answer1, result1);
                if (result0 >= 0)
                {
                    answerLeft = new QuestionPopupDialogAnswer(answer0, result0);
                }
            }
            else if (result0 >= 0)
            {
                answerLeft = new QuestionPopupDialogAnswer(answer0, result0);
            }
            InteractionQuestionPopupModel interactionQuestionPopupModel = new InteractionQuestionPopupModel();
            interactionQuestionPopupModel.Title = title;
            interactionQuestionPopupModel.Question = question.GetTextForUI(logic.Lang)[0].TextItem;
            interactionQuestionPopupModel.AnswerLeft = answerLeft;
            interactionQuestionPopupModel.AnswerMiddle = answerMiddle;
            interactionQuestionPopupModel.AnswerRight = answerRight;
            interactionQuestionPopupModel.DialogSize = size;
            return HandleRegisterInteraction(interactionQuestionPopupModel);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected virtual int HandleRegisterInteractionAdvanced(InteractionQuestionAdvancedPopupModel model)
        {
            logic.Services.InteractionService.Register(model);
            return model.Response.Selection;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected virtual int HandleRegisterInteraction(InteractionQuestionPopupModel m)
        {
            logic.Services.InteractionService.RegisterSync(m);
            return m.Response.Selection;
        }
#endif
        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected int __QuestionPopup(ITextContent text, params ITextContent[] buttonTexts)
        {
            return DoShowQuestionPopup(null, text, 0, "__QuestionPopup", buttonTexts);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected int __QuestionPopup(ITextLocator text, params ITextLocator[] buttonTexts)
        {
            ITextContent text2 = text?.TextContent;
            ITextContent[] buttonTexts2;
            if (buttonTexts == null)
            {
                buttonTexts2 = null;
            }
            else
            {
                buttonTexts2 = new ITextContent[buttonTexts.Length];
                for (int i = 0; i < buttonTexts.Length; i++)
                {
                    _ = buttonTexts[i];
                    text2 = text.TextContent;
                }
            }
            return __QuestionPopup(text2, buttonTexts2);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int ShowQuestionPopup(ITextContent title, ITextContent question, int size = 0, params ITextContent[] buttonTexts)
        {
            return DoShowQuestionPopup(title, question, size, "ShowQuestionPopup", buttonTexts);
        }

        private int DoShowQuestionPopup(ITextContent title, ITextContent text, int size, string callingMethod, params ITextContent[] buttonTexts)
        {
            int num = 0;
            DateTime now = DateTime.Now;
            try
            {
                int result = -9;
                int result2 = -9;
                int result3 = -9;
                string text2 = null;
                string text3 = null;
                string text4 = null;
                if (buttonTexts != null && buttonTexts.Length != 0)
                {
                    if (buttonTexts[0] != null)
                    {
                        text2 = buttonTexts[0].PlainText;
                    }
                    if (buttonTexts.Length > 1)
                    {
                        if (buttonTexts[1] != null)
                        {
                            text3 = buttonTexts[1].PlainText;
                        }
                        if (buttonTexts.Length > 2 && buttonTexts[2] != null)
                        {
                            text4 = buttonTexts[2].PlainText;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(text2))
                {
                    if (!string.IsNullOrEmpty(text3))
                    {
                        result = 0;
                        result2 = 1;
                        if (!string.IsNullOrEmpty(text4))
                        {
                            result3 = 2;
                        }
                    }
                    else
                    {
                        result = 0;
                        if (!string.IsNullOrEmpty(text4))
                        {
                            result3 = 1;
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(text3))
                {
                    result2 = 0;
                    if (!string.IsNullOrEmpty(text4))
                    {
                        result3 = 1;
                    }
                }
                else if (!string.IsNullOrEmpty(text4))
                {
                    result3 = 0;
                }
                if (text == null || string.IsNullOrEmpty(text.PlainText))
                {
                    throw new ArgumentException("Parameter \"text\" must not be null or empty.");
                }
                List<LocalizedText> list = new List<LocalizedText>();
                string title2;
                if (title == null)
                {
                    list.AddRange(new FormatedData("#Question").Localize(logic.Lang));
                    title2 = list[0].TextItem;
                }
                else
                {
                    list.AddRangeIfNotContains(title.GetTextForUI(logic.Lang));
                    title2 = title.PlainText;
                }
                //[-] num = AskQuestion(text, text2, text3, text4, result, result2, result3, title2, size);
                if (FastaProtocoler != null)
                {
                    //[-] IAction<IUiDialog> action = FastaProtocoler.CreateAndAddUiDialogFromServiceProgram("QuestionPopup", callingMethod);
                    //[-] action.StartTime = now;
                    //[-] action.SpecialAction.CreateAndAddMessageText(text.GetTextForUI(logic.Lang));
                    //[-] action.SpecialAction.SetTitle(list);
                    List<LocalizedText> list2 = new List<LocalizedText>();
                    string resultString = num.ToString(CultureInfo.InvariantCulture);
                    list2.AddRange(logic.Lang.Select((string x) => new LocalizedText(resultString, x)));
                    //[-] action.SpecialAction.AddAnswer(list2, "clicked-button");
                }
                else
                {
                    Log.Error("ISTAModule.__QuestionPopup()", "No ModuleStep available in FASTA");
                }
            }
            catch (Exception ex)
            {
                Log.Error("ISTAModule.__QuestionPopup()", "Failed to ask question, returning {0} as result. {1}.", num, ex.ToString());
            }
            return num;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected int ShowQuestionAdvancedPopup(ITextLocator title, ITextLocator heading, ITextLocator suffix, ITextLocator buttonCancelText, ITextLocator buttonNextText, int buttonCount, ITextLocator[] buttonLabels, ITextLocator[] buttonTexts, int size = 2)
        {
            return DoShowQuestionAdvancedPopup(title, heading, suffix, buttonCancelText, buttonNextText, "ShowQuestionAdvancedPopup", buttonCount, buttonLabels, buttonTexts, size);
        }

        private int DoShowQuestionAdvancedPopup(ITextLocator title, ITextLocator heading, ITextLocator suffix, ITextLocator buttonCancelText, ITextLocator buttonNextText, string callingMethod, int buttonCount, ITextLocator[] buttonLabels, ITextLocator[] buttonTexts, int size)
        {
            int num = 0;
            DateTime now = DateTime.Now;
            try
            {
                //[-] InteractionQuestionAdvancedPopupModel newAdvancedPopupModel = GetNewAdvancedPopupModel(title, heading, suffix, buttonCancelText, buttonNextText, buttonCount, buttonLabels, buttonTexts, size);
                //[-] num = HandleRegisterInteractionAdvanced(newAdvancedPopupModel);
                if (FastaProtocoler != null)
                {
                    List<LocalizedText> title2 = new List<LocalizedText>();
                    //[-] IAction<IUiDialog> action = FastaProtocoler.CreateAndAddUiDialogFromServiceProgram("QuestionPopup", callingMethod);
                    //[-] action.StartTime = now;
                    //[-] action.SpecialAction.CreateAndAddMessageText(logic.Lang.Select((string x) => new LocalizedText(heading.Text, x)).ToList());
                    //[-] action.SpecialAction.SetTitle(title2);
                    List<LocalizedText> list = new List<LocalizedText>();
                    string resultString = num.ToString(CultureInfo.InvariantCulture);
                    list.AddRange(logic.Lang.Select((string x) => new LocalizedText(resultString, x)));
                    //[-] action.SpecialAction.AddAnswer(list, "clicked-button");
                }
                else
                {
                    Log.Error("ISTAModule.DoShowQuestionAdvancedPopup()", "No ModuleStep available in FASTA");
                }
            }
            catch (Exception ex)
            {
                Log.Error("ISTAModule.DoShowQuestionAdvancedPopup()", "Failed to ask advanced question, returning {0} as result. {1}.", num, ex.ToString());
            }
            return num;
        }
#if false
        private InteractionQuestionAdvancedPopupModel GetNewAdvancedPopupModel(ITextLocator title, ITextLocator heading, ITextLocator suffix, ITextLocator buttonCancelText, ITextLocator buttonNextText, int buttonCount, ITextLocator[] buttonLabels, ITextLocator[] buttonTexts, int size)
        {
            InteractionQuestionAdvancedPopupModel interactionQuestionAdvancedPopupModel = new InteractionQuestionAdvancedPopupModel();
            InitializeModelSelections(buttonCount, buttonLabels, buttonTexts, interactionQuestionAdvancedPopupModel);
            interactionQuestionAdvancedPopupModel.Title = title?.ToString();
            TextContent textContent = new TextContent(heading?.Text);
            interactionQuestionAdvancedPopupModel.Heading = textContent.GetTextForUI(logic.Lang)[0].TextItem;
            TextContent textContent2 = new TextContent(suffix?.Text);
            interactionQuestionAdvancedPopupModel.Suffix = textContent2.GetTextForUI(logic.Lang)[0].TextItem;
            string text = buttonCancelText?.ToString();
            if (!string.IsNullOrEmpty(text))
            {
                interactionQuestionAdvancedPopupModel.AnswerCancel = new QuestionPopupAdvancedDialogAnswer(string.Empty, text, -1);
            }
            interactionQuestionAdvancedPopupModel.AnswerNext = new QuestionPopupAdvancedDialogAnswer(string.Empty, buttonNextText?.ToString(), -1);
            interactionQuestionAdvancedPopupModel.Answer = new QuestionPopupAdvancedDialogAnswer(string.Empty, string.Empty, -1);
            interactionQuestionAdvancedPopupModel.DialogSize = size;
            return interactionQuestionAdvancedPopupModel;
        }

        private static void InitializeModelSelections(int buttonCount, ITextLocator[] buttonLabels, ITextLocator[] buttonTexts, InteractionQuestionAdvancedPopupModel model)
        {
            model.Selections = new List<QuestionPopupAdvancedDialogAnswer>();
            for (int i = 0; i < buttonCount && !LessTextItemsAvailableThenButtonCount(buttonTexts, i); i++)
            {
                ITextLocator textLocator = buttonTexts[i];
                ITextLocator textLocator2 = new TextLocator(string.Empty);
                if (buttonLabels.Count() > i && buttonLabels[i] != null)
                {
                    textLocator2 = buttonLabels[i];
                }
                model.Selections.Add(new QuestionPopupAdvancedDialogAnswer(textLocator?.ToString(), textLocator2?.ToString(), i));
            }
        }
#endif
        private static bool LessTextItemsAvailableThenButtonCount(ITextLocator[] buttonTexts, int index)
        {
            return buttonTexts.Count() <= index;
        }

        private static void CheckParameters(ITextLocator title, ITextLocator heading, ITextLocator suffix, ITextLocator buttonCancelText, ITextLocator buttonNextText, int buttonCount, ITextLocator[] buttonLabels, ITextLocator[] buttonTexts)
        {
            if (title == null)
            {
                throw new ArgumentException("Parameter title must not be null or empty.");
            }
            if (heading == null)
            {
                throw new ArgumentException("Parameter heading must not be null or empty.");
            }
            if (suffix == null)
            {
                throw new ArgumentException("Parameter suffix must not be null or empty.");
            }
            if (buttonNextText == null)
            {
                throw new ArgumentException("Parameter buttonNextText must not be null or empty.");
            }
            if (buttonCancelText == null)
            {
                throw new ArgumentException("Parameter buttonCancelText must not be null or empty.");
            }
            if (buttonCount < 0 || buttonCount > 20)
            {
                throw new ArgumentException("Parameter buttonCount count must be between 0 and 20.");
            }
            if (buttonLabels.Count() < 0 || buttonLabels.Count() > 20)
            {
                throw new ArgumentException("Parameter buttonLabels count must be between 0 and 20.");
            }
            if (buttonTexts.Count() < 0 || buttonTexts.Count() > 20)
            {
                throw new ArgumentException("Parameter buttonTexts count must be between 0 and 20.");
            }
        }
#if false
        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected void DocumentHandler(DocumentStatementAction action, IDocumentLocator documentLocator)
        {
            try
            {
                if (documentLocator is DocumentLocator documentLocator2)
                {
                    Log.Info("ISTAModule.DocumentHandler(DocumentStatementAction, IDocumentLocator)", "Execute action: {0} document id: {1}", action.ToString(), (documentLocator != null) ? documentLocator.Id : "'null'");
                    DocumentHandler(action, documentLocator2.GetDocument());
                }
                else
                {
                    Log.Warning("ISTAModule.DocumentHandler()", "documentLocator is null!");
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.DocumentHandler(DocumentStatementAction action, DocumentLocator documentLocator, int Slot)", exception);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected void DocumentHandler(DocumentStatementAction action, IList<IDocumentLocator> documentLocators)
        {
            DocumentHandler(action, documentLocators, -1);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected void DocumentHandler(DocumentStatementAction action, IDocumentLocator documentLocator, int slot)
        {
            try
            {
                if (documentLocator is DocumentLocator documentLocator2)
                {
                    Log.Info("ISTAModule.DocumentHandler(DocumentStatementAction, IDocumentLocator, int)", "Execute action: {0} document id: {1} slot: {2}", action.ToString(), (documentLocator2 != null) ? documentLocator2.Id : "'null'", slot);
                    DocumentHandler(action, documentLocator2.GetDocument(), slot);
                }
                else
                {
                    Log.Warning("ISTAModule.DocumentHandler(DocumentStatementAction, IDocumentLocator, int)", "documentLocators was null");
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.DocumentHandler(DocumentStatementAction action, DocumentLocator documentLocator, int Slot)", exception);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected void DocumentHandler(DocumentStatementAction action, IList<IDocumentLocator> documentLocators, int slot)
        {
            if (documentLocators != null)
            {
                List<InfoObject> list = new List<InfoObject>();
                foreach (IDocumentLocator documentLocator2 in documentLocators)
                {
                    if (!(documentLocator2 is DocumentLocator documentLocator))
                    {
                        Log.Warning("ISTAModule.DocumentHandler(DocumentStatementAction, List<IDocumentLocator>, int)", "Documentlocator will be ignored, because it is null.");
                    }
                    else
                    {
                        list.Add(documentLocator.GetDocument());
                    }
                }
                DocumentHandler(action, list, slot);
            }
            else
            {
                Log.Warning("ISTAModule.DocumentHandler(DocumentStatementAction, List<IDocumentLocator>, int)", "documentLocators was null");
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        private void DocumentHandler(DocumentStatementAction action, InfoObject document)
        {
            DocumentHandler(action, document, -1);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        private void DocumentHandler(DocumentStatementAction action, InfoObject document, int slot)
        {
            if (document == null)
            {
                Log.Warning("ISTAModule.DocumentHandler()", "document was null");
                return;
            }
            List<InfoObject> list = new List<InfoObject>();
            if (document != null)
            {
                list.Add(document);
            }
            DocumentHandler(action, list, slot);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        private void DocumentHandler(DocumentStatementAction action, IList<InfoObject> documents, int slot)
        {
            try
            {
                List<InfoObject> list = new List<InfoObject>();
                string text = string.Empty;
                if (documents != null)
                {
                    foreach (InfoObject document in documents)
                    {
                        if (document != null)
                        {
                            text += string.Format(CultureInfo.InvariantCulture, "{0},", document.Id);
                        }
                    }
                    text = text.TrimEnd(',');
                }
                Log.Info("ISTAModule.DocumentHandler(DocumentStatementAction, List<Document>, int)", "execute action: {0} documents: {1} slot: {2}", action.ToString(), text, slot);
                foreach (InfoObject document2 in documents)
                {
                    if (document2 == null || (document2.Content.Doc == null && document2.Content.BinaryDocument == null) || document2.Content.TransformedDocument == null)
                    {
                        Log.Warning("ISTAModule.DocumentHandler(DocumentStatementAction action, Document document, int Slot)", "document was null");
                    }
                    else
                    {
                        list.Add(document2);
                    }
                }
                switch (action)
                {
                    case DocumentStatementAction.Add:
                        if (_globalTabModuleISTA != null)
                        {
                            _globalTabModuleISTA.AddDocInfoObjects(list, slot, FastaProtocoler);
                        }
                        break;
                    case DocumentStatementAction.RemoveAll:
                        if (_globalTabModuleISTA != null)
                        {
                            _globalTabModuleISTA.RemoveDocInfoObjectsAll();
                        }
                        break;
                    case DocumentStatementAction.Remove:
                        if (_globalTabModuleISTA != null)
                        {
                            _globalTabModuleISTA.RemoveDocInfoObjects(list, slot);
                        }
                        break;
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.DocumentHandler(DocumentStatementAction action, Document document, int Slot)", exception);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected void DocumentHandler(DocumentStatementAction action)
        {
            Log.Info("ISTAModule.DocumentHandler(DocumentStatementAction action)", "called with action:{0}", action.ToString());
            if ((uint)action > 1u && action == DocumentStatementAction.RemoveAll && _globalTabModuleISTA != null)
            {
                _globalTabModuleISTA.RemoveDocInfoObjectsAll();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected void DocumentHandler(DocumentStatementAction action, int slot)
        {
            Log.Info("ISTAModule.DocumentHandler(DocumentStatementAction action, int Slot)", "called with action:{0} Slot:{1}", action.ToString(), slot);
            switch (action)
            {
                case DocumentStatementAction.Remove:
                    if (_globalTabModuleISTA != null)
                    {
                        _globalTabModuleISTA.RemoveDocInfoObjects(null, slot);
                    }
                    break;
                case DocumentStatementAction.RemoveAll:
                    if (_globalTabModuleISTA != null)
                    {
                        _globalTabModuleISTA.RemoveDocInfoObjectsAll();
                    }
                    break;
                case DocumentStatementAction.Add:
                    break;
            }
        }
#endif
        private bool __tryQueryValue(ANode node, string nodePath, out Value resultValue)
        {
            bool result = false;
            resultValue = null;
            try
            {
                resultValue = node.Query(nodePath) as Value;
                result = true;
            }
            catch (Exception exception)
            {
                Log.ErrorException("ISTAModule.__tryQueryValue(ANode, string, out Value)", exception);
            }
            return result;
        }

        [PreserveSource(Hint = "XEP_DIAGNOSISOBJECTSEX", Placeholder = true)]
        public override PlaceholderType SelectDiagParentByAskingUser(IList<PlaceholderType> diag, string callingMethod)
        {
            throw new NotImplementedException();
        }

        public override void ShowMessage(string title, string message)
        {
            logic.Services.InteractionService.RegisterMessageAsync(title, message);
        }

        public override void RegisterAndDeregisterInteractionModel(InteractionModel interactionModel, bool register)
        {
            if (register)
            {
                logic.Services.InteractionService.Register(interactionModel);
            }
            else
            {
                logic.Services.InteractionService.Deregister(interactionModel);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected override void __handleInParameter()
        {
            try
            {
                if (_globalModuleInParameter != null)
                {
                    //[-] base.Me = _globalModuleInParameter.getParameter("ISTAModule.Me") as IXepInfoObject;
                    //[-] ModuleParameter moduleParameter = (ModuleParameter)_globalModuleInParameter.getParameter("__RheinGoldCoreModuleParameters__");
                    //[-] if (moduleParameter != null)
                    //[-] {
                    //[-] __RheinGoldCoreModuleParameters__ = moduleParameter;
                    //[-] logic = moduleParameter.getParameter(ModuleParameter.ParameterName.Logic) as ILogic;
                    //[-] vehicle = moduleParameter.getParameter(ModuleParameter.ParameterName.Vehicle) as BMW.Rheingold.CoreFramework.DatabaseProvider.Vehicle;
                    //[-] authoringVehicle = new Lazy<BMW.Authoring.Vehicle.IVehicle>(() => new BMW.Authoring.Vehicle.Vehicle(this));
                    //[-] _ISOCAccessor = new SOCAccessor(vehicle, LicenseHelper.DealerInstance);
                    _globalModuleInParameter.setParameter("__RheinGoldSOCAccessor__", _ISOCAccessor);
                    SetTextContentManager();
                    //[-] if (logic != null)
                    //[-] {
                    //[-] ffmResolver = logic.FFMResolver;
                    //[-] }
                    //[-] vehicleContext = new VehicleContext(vehicle, ffmResolver);
                    //[-] MeasurementLauncher = moduleParameter.getParameter(ModuleParameter.ParameterName.MeasurementLauncher) as IStartMeasurementServiceServer;
                    //[-] }
                    _globalTabModuleISTA = (IModuleExecutionParent)_globalModuleInParameter.getParameter("__RheinGoldTabModuleISTA__");
                    if (_globalTabModuleISTA == null)
                    {
                        Log.Warning("ISTAModule.__handleInParameter()", "failed with no TabModuleISTA handle available - this module will not work.");
                    }
                    base.FastaGrouping = _globalModuleInParameter.getParameter("FASTA") as IFastaGrouping;
                    if (base.FastaGrouping == null)
                    {
                        Log.Warning("ISTAModule.__handleInParameter()", "No FASTA functionality available.");
                    }
                    if (MeasurementLauncher == null)
                    {
                        Log.Warning("ISTAModule.__handleInParameter()", "No measurement service functionality available. Holy crab!");
                    }
                    if (appSessionContext == null)
                    {
                        InitAppSessionContext();
                    }
                    //[-] Logic obj = logic as Logic;
                    //[-] if (obj != null && obj.IsInputListenerActive)
                    {
                        InputListener.StartListening();
                    }
                    Dictionary<string, string> dictionary = _globalModuleInParameter.getParameter("InParameterTestModuleCache") as Dictionary<string, string>;
                    testModuleCache = dictionary ?? new Dictionary<string, string>();
                }
                else
                {
                    Log.Warning("ISTAModule.__handleInParameter()", "_globalModuleInParameter was null.");
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.__handleInParameter()", exception);
            }
        }

        private void InitAppSessionContext()
        {
            ILogic iLogic = logic;
            //[-] if (iLogic == null && Application.Current is IRheingoldApp rheingoldApp)
            //[-] {
            //[-] iLogic = rheingoldApp.ILogic;
            //[-] }
            //[-] appSessionContext = new AppSessionContext(ConfigSettings.OperationalMode, ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.OnlineMode", defaultValue: true), iLogic);
        }

        private void SetTextContentManager()
        {
            if (base.textContentManager == null)
            {
                base.textContentManager = _globalModuleInParameter.getParameter("ISTAModule.TextCollection") as TextContentManager;
                if (base.textContentManager == null)
                {
                    //[-] base.textContentManager = TextContentManager.Create(DatabaseProviderFactory.Instance, logic.Lang, base.Me);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public override void callModuleRef(string refPath, ParameterContainer inParameters, ref ParameterContainer outParameters, ref ParameterContainer inAndOutParameters)
        {
            //[-] IXepInfoObject xepInfoObject = null;
            InteractionMessageModel interactionMessageModel = new InteractionMessageModel();
            Log.Info("ISTAModule.callModuleRef()", "CallStatement with reference path: {0}", refPath);
            try
            {
                //[-] xepInfoObject = DBProvider.GetInfoObjectByControlId(Convert.ToInt64(refPath, CultureInfo.InvariantCulture));
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.callModuleRef()", exception);
            }
            //[-] if (xepInfoObject != null)
            //[-] {
            //[-] if (!string.IsNullOrEmpty(xepInfoObject.Identifikator))
            //[-] {
            //[-] decimal? generell = xepInfoObject.Generell;
            //[-] decimal num = 1;
            //[-] if ((generell.GetValueOrDefault() == num) & generell.HasValue)
            //[-] {
            //[-] feedbackViewHeaderHelper?.SetDocumentSubTitle(xepInfoObject.DocNumber, xepInfoObject.VersionNumber.Value.ToString());
            //[-] }
            //[-] string text = "BMW.Rheingold.Module.ISTA." + IstaModuleBase.ModuleNameTransformator(xepInfoObject.Identifikator);
            //[-] Log.Info("ISTAModule.callModuleRef()", "submodule to call: {0}", text);
            //[-] try
            //[-] {
            //[-] string text2 = text.Replace("BMW.Rheingold.Module.ISTA.", string.Empty);
            //[-] Assembly assembly = PatchLoaderUtility.CheckPatchServiceProgram(text2, logic.VersionInfo.DataBaseDiagDocVersion);
            //[-] if (assembly == null)
            //[-] {
            //[-] assembly = GetModuleAssembly(text2);
            //[-] }
            //[-] IModuleStep moduleStep = null;
            //[-] bool flag = false;
            //[-] if (inParameters.Parameter.ContainsKey("PreventProtocol"))
            //[-] {
            //[-] flag = (bool)inParameters.Parameter["PreventProtocol"];
            //[-] }
            //[-] else if (_globalModuleInParameter.Parameter.ContainsKey("PreventProtocol"))
            //[-] {
            //[-] flag = (bool)_globalModuleInParameter.Parameter["PreventProtocol"];
            //[-] }
            //[-] if (!IsTestModulePreventedFromProtocolling(xepInfoObject.Identifikator) || !flag)
            //[-] {
            //[-] moduleStep = FastaCreateAndAddModuleStepTo(inParameters);
            //[-] }
            //[-] ModuleParameter value = __RheinGoldCoreModuleParameters__.Clone();
            //[-] inParameters.Parameter.Add("__RheinGoldCoreModuleParameters__", value);
            //[-] inParameters.Parameter.Add("__RheinGoldTabModuleISTA__", _globalTabModuleISTA);
            //[-] inParameters.Parameter.Add("__RheinGoldSOCAccessor__", _ISOCAccessor);
            //[-] inParameters.Parameter.Add("ISTAModule.Me", xepInfoObject);
            //[-] inParameters.Parameter.Add("InParameterTestModuleCache", testModuleCache);
            //[-] inParameters.Parameter.Add("PreventProtocol", flag);
            //[-] object obj = assembly.CreateInstance(text, ignoreCase: true, BindingFlags.ExactBinding, null, new object[1] { inParameters }, new CultureInfo(ConfigSettings.CurrentUICulture), null);
            //[-] if (obj != null)
            //[-] {
            //[-] string name = obj.GetType().Name;
            //[-] ISubModule subModule = FastaCreateAndAddSubmodule(moduleStep, inParameters, GetLocalizedInfoObjectTitle(xepInfoObject, name), xepInfoObject.Identifikator);
            //[-] IFastaGrouping fastaGrouping = null;
            //[-] if (_globalTabModuleISTA != null)
            //[-] {
            //[-] fastaGrouping = _globalTabModuleISTA.FastaGrouping;
            //[-] _globalTabModuleISTA.FastaGrouping = subModule;
            //[-] }
            //[-] MethodInfo method = obj.GetType().GetMethod("run");
            //[-] if (method != null)
            //[-] {
            //[-] Log.Info("ISTAModule.callModuleRef()", "executing submodule now...");
            //[-] method.Invoke(obj, new object[3] { inParameters, outParameters, inAndOutParameters });
            //[-] Log.Info("ISTAModule.callModuleRef()", "returned from submodule");
            //[-] if (obj is ISTAModule)
            //[-] {
            //[-] ISTAModule iSTAModule = (ISTAModule)obj;
            //[-] Log.Info("ISTAModule.callModuleRef()", "submodule returned with collective result: {0}", iSTAModule.ResultSet.CollectiveResult);
            //[-] if (subModule != null)
            //[-] {
            //[-] subModule.CollectiveResult = iSTAModule.ResultSet.CollectiveResult;
            //[-] }
            //[-] else
            //[-] {
            //[-] Log.Error("ISTAModule.callModuleRef()", "Failed to set CollectiveResult \"{0}\", because FASTA2 instance of type \"ISubModule\" is null.", base.ResultSet.CollectiveResult);
            //[-] }
            //[-] }
            //[-] if (_globalTabModuleISTA != null)
            //[-] {
            //[-] _globalTabModuleISTA.FastaGrouping = fastaGrouping;
            //[-] }
            //[-] }
            //[-] else
            //[-] {
            //[-] Log.Error("ISTAModule.callModuleRef()", "no run method found!!!");
            //[-] }
            //[-] }
            //[-] else
            //[-] {
            //[-] Log.Warning("ISTAModule.callModuleRef()", "unable to create instance of: {0}", text);
            //[-] if (_globalModuleInParameter != null)
            //[-] {
            //[-] interactionMessageModel.MessageText = string.Format(CultureInfo.InvariantCulture, "System error in testmodule: {0}", GetType().Name);
            //[-] interactionMessageModel.DetailText = string.Format(CultureInfo.InvariantCulture, "Unable to start submodule {0} / {1}", refPath, text);
            //[-] interactionMessageModel.Title = "Error";
            //[-] logic.Services.InteractionService.Register(interactionMessageModel);
            //[-] }
            //[-] }
            //[-] return;
            //[-] }
            //[-] catch (ThreadAbortException ex)
            //[-] {
            //[-] Log.Warning("ISTAModule.callModuleRef()", "Abort: {0}", ex.ToString());
            //[-] return;
            //[-] }
            //[-] catch (Exception exception2)
            //[-] {
            //[-] Log.WarningException("ISTAModule.callModuleRef()", exception2);
            //[-] throw;
            //[-] }
            //[-] }
            //[-] interactionMessageModel.MessageText = new FormatedData("#FailedToResolveSubmoduleMessage", refPath).Localize();
            //[-] interactionMessageModel.DetailText = new FormatedData("#FailedToResolveSubmoduleDetail", base.LastCallingMethod).Localize();
            //[-] interactionMessageModel.Title = FormatedData.Localize("#Error");
            //[-] logic.Services.InteractionService.Register(interactionMessageModel);
            //[-] Log.Warning("ISTAModule.callModuleRef()", "Failed because there the identifikator of info object with ID \"{0}\" is null or empty.", xepInfoObject.Id);
            //[-] }
            //[-] else
            {
                interactionMessageModel.MessageText = new FormatedData("#FailedToResolveSubmoduleMessage", refPath).Localize();
                interactionMessageModel.DetailText = new FormatedData("#FailedToResolveSubmoduleDetail", base.LastCallingMethod).Localize();
                interactionMessageModel.Title = FormatedData.Localize("#Error");
                logic.Services.InteractionService.Register(interactionMessageModel);
                Log.Warning("ISTAModule.callModuleRef()", "Failed because there is no module name known for path \"{0}\".", refPath);
            }
        }

        private bool IsTestModulePreventedFromProtocolling(string moduleName)
        {
            return new List<string> { "ABL-GEN", "ABL_GEN", "ABL-LIF", "ABL_LIF" }.Any((string m) => moduleName.ToUpper().StartsWith(m));
        }

        private Assembly TryGetAssemblyFromAppDomain(string cleanIstaModuleName)
        {
            if (testModuleCache.ContainsKey(cleanIstaModuleName))
            {
                return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly a) => a.GetName().Name == testModuleCache[cleanIstaModuleName]);
            }
            return null;
        }

        private void StoreInCache(Assembly compiledAssembly, string cleanIstaModuleName)
        {
            testModuleCache.Add(cleanIstaModuleName, compiledAssembly.GetName().Name);
        }

        private bool ShouldUseTestmoduleCache()
        {
            if (ConfigSettings.IsVerificationMode)
            {
                return ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Diagnostics.Module.ISTA.UseCompiledTestmodulesCache", defaultValue: false);
            }
            return false;
        }

        private Assembly GetModuleAssembly(string cleanIstaModuleName)
        {
            Assembly assembly = null;
            bool flag = false;
            string configString = ConfigSettings.getConfigString("BMW.Rheingold.Diagnostics.Module.ISTA.ISTATabModuleCore.TestmoduleType", "SingleAssemblyContainer");
            if (!(configString == "OnTheFlyCompiler"))
            {
                if (!(configString == "SingleAssemblyContainer"))
                {
                }
                string text = Path.Combine(GetAssemblyDir(), cleanIstaModuleName + ".dll");
                assembly = Assembly.LoadFrom(text);
                if (assembly == null)
                {
                    Log.Error("ISTAModule.GetModuleAssembly()", "Failed to load Single Assembly Container {0}. Does the given file exist?", text);
                }
            }
            else
            {
                if (ShouldUseTestmoduleCache())
                {
                    assembly = TryGetAssemblyFromAppDomain(cleanIstaModuleName);
                    flag = assembly == null;
                }
                if (assembly == null)
                {
                    assembly = CompileOnTheFly(cleanIstaModuleName);
                    if (flag && assembly != null)
                    {
                        StoreInCache(assembly, cleanIstaModuleName);
                    }
                }
            }
            return assembly;
        }

        private string GetAssemblyDir()
        {
            return Path.GetFullPath(Path.Combine(ConfigSettings.AppBaseDirectory, ConfigSettings.getPathString("BMW.Rheingold.Diagnostics.Module.ISTA.ISTATabModuleCore.SubModulePath", "..\\..\\..\\Testmodule")));
        }

        [PreserveSource(Hint = "IXepInfoObject", Placeholder = true)]
        public override PlaceholderType GetRootModule()
        {
            throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public void Sleep(int millisecondsTimeout)
        {
            if (EcuKom == null || EcuKom.CommunicationMode != CommMode.Simulation || !ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.ISTA.ISTAModule.SleepNoWaitInSimulation", defaultValue: true))
            {
                Thread.Sleep(millisecondsTimeout);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected ConfigurationContainer __ConfigurationContainer(string dscConfig)
        {
            try
            {
                StringReader input = new StringReader("<?xml version=\"1.0\" encoding=\"utf-8\"?><ConfigurationContainer xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" MajorVersion=\"1\" MinorVersion=\"0\" Compression=\"Zip\"><Name>Parametrization tree for EDIABAS</Name>" + dscConfig + "</ConfigurationContainer>");
                return (ConfigurationContainer)new XmlSerializer(typeof(ConfigurationContainer)).Deserialize(XmlReader.Create(input));
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.__ConfigurationContainer()", exception);
                return null;
            }
        }

        private IModuleStep FastaCreateAndAddModuleStepTo(ParameterContainer inParameters)
        {
            IModuleStep moduleStep = null;
            if (base.FastaGrouping != null)
            {
                List<LocalizedText> list = new List<LocalizedText>();
                list.AddRange(logic.Lang.Select((string x) => new LocalizedText(base.LastCallingMethod, x)));
                moduleStep = base.FastaGrouping.CreateSubGroup(GroupingType.Ablaufschritt, list) as IModuleStep;
                if (inParameters.Parameter.ContainsKey("FASTA"))
                {
                    inParameters.Parameter["FASTA"] = moduleStep;
                }
                else
                {
                    inParameters.Parameter.Add("FASTA", moduleStep);
                }
            }
            return moduleStep;
        }

        internal string DetermineLayoutGroup(InfoObject infoObject)
        {
            string empty = string.Empty;
            if (infoObject == null || string.IsNullOrEmpty(infoObject.Identifier))
            {
                return "n/a";
            }
            empty = infoObject.Identifier;
            if (!empty.StartsWith("ABL", StringComparison.Ordinal))
            {
                return "n/a";
            }
            if (empty.StartsWith("ABL-AUS", StringComparison.Ordinal) || empty.StartsWith("ABL-SMP", StringComparison.Ordinal) || empty.StartsWith("ABL-MPB", StringComparison.Ordinal))
            {
                return "PBV";
            }
            if (empty.StartsWith("ABL-MNS", StringComparison.Ordinal) || empty.StartsWith("ABL-MNF", StringComparison.Ordinal) || empty.StartsWith("ABL-MHN", StringComparison.Ordinal))
            {
                return "PAN";
            }
            if (empty.StartsWith("ABL-PRF", StringComparison.Ordinal) || empty.StartsWith("ABL-SMP", StringComparison.Ordinal) || empty.StartsWith("ABL-MPB", StringComparison.Ordinal))
            {
                return "PAP";
            }
            if (empty.StartsWith("ABL-ESK", StringComparison.Ordinal) || empty.StartsWith("ABL-ESK", StringComparison.Ordinal) || empty.StartsWith("ABL-ESK", StringComparison.Ordinal))
            {
                return "PAE";
            }
            if (empty.StartsWith("ABL-MHV", StringComparison.Ordinal) || empty.StartsWith("ABL-MVF", StringComparison.Ordinal) || empty.StartsWith("ABL-MVS", StringComparison.Ordinal))
            {
                return "PAV";
            }
            return "D";
        }

        private ISubModule FastaCreateAndAddSubmodule(IModuleStep moduleStep, ParameterContainer inParameters, IList<LocalizedText> subgroupTitleList, string identifier)
        {
            if (moduleStep != null)
            {
                IFastaGrouping fastaGrouping = moduleStep.CreateSubGroup(GroupingType.Unterablauf, subgroupTitleList);
                if (fastaGrouping != null)
                {
                    fastaGrouping.Identifier = identifier;
                    fastaGrouping.EndTime = DateTime.Now;
                    if (inParameters.Parameter.ContainsKey("FASTA"))
                    {
                        inParameters.Parameter["FASTA"] = fastaGrouping;
                    }
                    else
                    {
                        inParameters.Parameter.Add("FASTA", fastaGrouping);
                    }
                    return fastaGrouping as ISubModule;
                }
                Log.Error("ISTAModule.callModule()", "In FASTA no submodule is created.");
            }
            else
            {
                Log.Warning("ISTAModule.callModule()", "FASTA is not available.");
            }
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected void callModule(string moduleName, ParameterContainer inParameters, ref ParameterContainer outParameters, ref ParameterContainer inAndOutParameters)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                Log.Warning("ISTAModule.callModule()", "Failed because moduleName was empty");
                return;
            }
            try
            {
                string cleanIstaModuleName = moduleName.Replace("BMW.Rheingold.Diagnostics.Module.ISTA.", string.Empty);
                Assembly moduleAssembly = GetModuleAssembly(cleanIstaModuleName);
                IModuleStep moduleStep = FastaCreateAndAddModuleStepTo(inParameters);
                //[-] ModuleParameter value = __RheinGoldCoreModuleParameters__.Clone();
                //[-] inParameters.Parameter.Add("__RheinGoldCoreModuleParameters__", value);
                inParameters.Parameter.Add("__RheinGoldTabModuleISTA__", _globalTabModuleISTA);
                inParameters.Parameter.Add("__RheinGoldSOCAccessor__", _ISOCAccessor);
                inParameters.Parameter.Add("InParameterTestModuleCache", testModuleCache);
                object obj = moduleAssembly.CreateInstance("BMW.Rheingold.Module.ISTA." + moduleName, ignoreCase: true, BindingFlags.ExactBinding, null, new object[1] { inParameters }, new CultureInfo(ConfigSettings.CurrentUICulture), null);
                if (obj != null)
                {
                    MethodInfo method = obj.GetType().GetMethod("run");
                    if (method != null)
                    {
                        string name = obj.GetType().Name;
                        //[-] ISubModule subModule = FastaCreateAndAddSubmodule(moduleStep, inParameters, GetLocalizedInfoObjectTitle(null, name), null);
                        IFastaGrouping fastaGrouping = null;
                        if (_globalTabModuleISTA != null)
                        {
                            fastaGrouping = _globalTabModuleISTA.FastaGrouping;
                            //[-] _globalTabModuleISTA.FastaGrouping = subModule;
                        }
                        Log.Info("ISTAModule.callModule()", "executing submodule now...");
                        method.Invoke(obj, new object[3] { inParameters, outParameters, inAndOutParameters });
                        CallingModule = moduleName;
                        Log.Info("ISTAModule.callModule()", "returned from submodule");
                        if (obj is ISTAModule)
                        {
                            ISTAModule iSTAModule = (ISTAModule)obj;
                            Log.Info("ISTAModule.callModule()", "submodule returned with collective result: {0}", iSTAModule.ResultSet.CollectiveResult);
                            //[-] if (subModule != null)
                            //[-] {
                            //[-] subModule.CollectiveResult = iSTAModule.ResultSet.CollectiveResult;
                            //[-] }
                            //[-] else
                            {
                                Log.Error("ISTAModule.callModule()", "Failed to set CollectiveResult \"{0}\", because FASTA2 instance of type \"ISubModule\" is null.", base.ResultSet.CollectiveResult);
                            }
                        }
                        if (_globalTabModuleISTA != null)
                        {
                            _globalTabModuleISTA.FastaGrouping = fastaGrouping;
                        }
                    }
                    else
                    {
                        Log.Error("ISTAModule.callModule()", "no run method found!!!");
                    }
                }
                else
                {
                    Log.Warning("ISTAModule.callModule()", "unable to create instance of: {0}", moduleName);
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.callModule()", exception);
            }
        }

        private Assembly CompileOnTheFly(string istaModuleName)
        {
            lock (onTheFlyCompileLock)
            {
                Assembly result;
                try
                {
                    string fullPath = Path.GetFullPath(ConfigSettings.getConfigString("BMW.Rheingold.ServiceProgramCompiler", ".\\RGSPC.exe"));
                    Assembly assembly = Assembly.LoadFrom(fullPath);
                    if (!(assembly != null))
                    {
                        Log.Error("ISTAModule.CompileOnTheFly()", "Couldn't find Serviceprogram Compiler {0}", fullPath);
                        return null;
                    }
                    string location = assembly.Location;
                    try
                    {
                        FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(location);
                        FileVersionInfo versionInfo2 = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
                        string fileVersion = versionInfo.FileVersion;
                        string fileVersion2 = versionInfo2.FileVersion;
                        if (!fileVersion.Split('.')[0].Equals(fileVersion2.Split('.')[0]) && !fileVersion.Split('.')[1].Equals(fileVersion2.Split('.')[1]))
                        {
                            Log.Error("ISTAModule.CompileOnTheFly()", "ISTA Version {0} and ServiceProgramCompiler Version {1} are not compatible, therefore compiling ServicePrograms on the fly is not working. Please use a ServiceProgramm compiler of the same Version!", fileVersion2, fileVersion);
                            return null;
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.WarningException("ISTAModule.CompileOnTheFly()", exception);
                    }
                    MethodInfo method = assembly.CreateInstance("BMW.Rheingold.ServiceProgramCompiler.Program").GetType().GetMethod("CompileModuleOnTheFly");
                    if (!(method != null))
                    {
                        Log.Error("ISTAModule.CompileOnTheFly()", "Couldn't find Method {0} in {1}", "CompileModuleOnTheFly", "BMW.Rheingold.ServiceProgramCompiler.Program");
                        return null;
                    }
                    result = (Assembly)method.Invoke(null, new object[1] { istaModuleName });
                }
                catch (Exception exception2)
                {
                    Log.ErrorException("ISTAModule.CompileOnTheFly()", exception2);
                    return null;
                }
                return result;
            }
        }
#if false
        private List<LocalizedText> GetLocalizedInfoObjectTitle(IXepInfoObject xepInfoObject, string fastaTitle)
        {
            List<LocalizedText> list = new List<LocalizedText>();
            list.AddRange(logic.Lang.Select((string x) => new LocalizedText((xepInfoObject != null) ? xepInfoObject.GetLocalizedInfoObjectTitle(x) : fastaTitle, x)));
            return list;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public static void Serialize(string filename, Flow flow, Encoding encType)
        {
            Log.Info("ISTAModule.Serialize()", "called");
            try
            {
                XmlTextWriter xmlTextWriter = new XmlTextWriter(filename, encType);
                new XmlSerializer(typeof(Flow)).Serialize(xmlTextWriter, flow);
                xmlTextWriter.Close();
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.Serialize()", exception);
            }
            Log.Info("ISTAModule.Serialize()", "successfully done");
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public static Flow DeSerialize(string filename)
        {
            Log.Debug("ISTAModule.DeSerialize()", "called");
            Flow result;
            try
            {
                XmlTextReader xmlTextReader = new XmlTextReader(filename);
                result = (Flow)new XmlSerializer(typeof(Flow)).Deserialize(xmlTextReader);
                xmlTextReader.Close();
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.DeSerialize()", exception);
                return null;
            }
            Log.Debug("ISTAModule.DeSerialize()", "successfully done");
            return result;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public IDiagnosticDeviceResult DscSynchron(bool standardErrorHandling, ConfigurationContainer cfgContainer)
        {
            if (cfgContainer != null && cfgContainer.Header != null && cfgContainer.Header.Adapter != null)
            {
                switch (cfgContainer.Header.Adapter.Name)
                {
                    case "BMW IMIB":
                        {
                            MeasuringConfigurationAdapter measuringConfigurationAdapter = new MeasuringConfigurationAdapter(cfgContainer);
                            measuringConfigurationAdapter.ParseParametrization();
                            dscSynchronResult = measuringConfigurationAdapter.Execute();
                            return dscSynchronResult;
                        }
                    case "BMW-EDIABAS-Adapter":
                        {
                            EDIABASAdapter eDIABASAdapter = new EDIABASAdapter(standardErrorHandling, EcuKom, cfgContainer);
                            eDIABASAdapter.DoParameterization();
                            dscSynchronResult = eDIABASAdapter.Execute();
                            return dscSynchronResult;
                        }
                    case "VehicleConfiguration":
                        Log.Info("ISTAModule.DscSynchron()", "VehicleConfiguration used");
                        dscSynchronResult = new VehicleConfigurationAdapterDeviceResult(Vehicle);
                        return dscSynchronResult;
                    default:
                        Log.Warning("ISTAModule.DscSynchron()", "unknown adapter type used: {0}", cfgContainer.Header.Adapter.Name);
                        dscSynchronResult = null;
                        return dscSynchronResult;
                }
            }
            Log.Warning("ISTAModule.DscSynchron()", "cfgContainer or header was null");
            dscSynchronResult = null;
            return null;
        }
#endif
        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        private static Assembly TypeResolver(string typeName)
        {
            try
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly assembly in assemblies)
                {
                    if (assembly.GetType(typeName) != null)
                    {
                        Log.Info("TabModuleCore.TypeResolver()", "found {0} in {1}", typeName, assembly.FullName);
                        return assembly;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("TabModuleCore.TypeResolver()", exception);
            }
            return Assembly.GetCallingAssembly();
        }

        public override InfoObject GetInfoObjStarted()
        {
            //[-] InfoObject infoObject = __RheinGoldCoreModuleParameters__.getParameter(ModuleParameter.ParameterName.InfoObjStarted) as InfoObject;
            //[-] if (infoObject == null)
            //[-] {
            //[-] Log.Info("ISTAModule.GetInfoObjStarted()", "InfoObjStarted is null.");
            //[-] if (!(__RheinGoldCoreModuleParameters__.getParameter(ModuleParameter.ParameterName.XepInfoObjectStarted) is XepInfoObject xep))
            //[-] {
            //[-] Log.Info("ISTAModule.GetInfoObjStarted()", "XepInfoObjectStarted is null.");
            //[-] }
            //[-] else if (logic == null || logic.Factory == null)
            //[-] {
            //[-] Log.Error("ISTAModule.GetInfoObjStarted()", "Logic or Logic.Factory is null.");
            //[-] }
            //[-] else
            //[-] {
            //[-] Log.Info("ISTAModule.GetInfoObjStarted()", "Create info object from XepInfoObjectStarted.");
            //[-] infoObject = logic.Factory.CreateInfoObject(xep);
            //[-] if (infoObject != null)
            //[-] {
            //[-] infoObject.ParentDiagnosisObject = null;
            //[-] __RheinGoldCoreModuleParameters__.setParameter(ModuleParameter.ParameterName.InfoObjStarted, infoObject);
            //[-] }
            //[-] }
            //[-] }
            //[-] if (infoObject == null)
            {
                Log.Error("ISTAModule.GetInfoObjStarted()", "Failed to get info object. Returning null.");
            }
            //[-] return infoObject;
            //[+] return null;
            return null;
        }

        public override void AddSuspiciousItemToServiceProgram(IDiagnosticObjectLocator diagObjLocator)
        {
            try
            {
                string dataValue = diagObjLocator.GetDataValue("GROBZEICHEN");
                if (!string.IsNullOrEmpty(dataValue))
                {
                    if (_globalTabModuleISTA != null)
                    {
                        _globalTabModuleISTA.AddSuspiciousObject(dataValue);
                        Log.Info("ISTAModule.__SetSuspiciousItem", "Diagnositic object with Grobzeichen {0} set as suspicious item to service program execution", dataValue);
                    }
                }
                else
                {
                    Log.Info("ISTAModule.__SetSuspiciousItem", "Do not set suspicious item to service program execution because Grobzeichen is null.");
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException("ISTAModule.__SetSuspiciousItem", exception);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override void SetProperty(string propertyName, object data)
        {
            Log.Info("ISTAModule.SetProperty()", "set property {0}", propertyName);
            if (Vehicle.SessionDataStore != null)
            {
                Vehicle.SessionDataStore.setParameter(propertyName, data);
            }
            else
            {
                Log.Error("ISTAModule.SetProperty()", "session data store in vehicle context was null!");
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override void SetPersistentProperty(string propertyName, object data)
        {
            Log.Info("ISTAModule.SetPersistentProperty()", "set perisistent property {0}", propertyName);
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                new BinaryFormatter().Serialize(memoryStream, data);
                byte[] array = memoryStream.GetBuffer();
                Log.Info("ISTAModule.SetPersistentProperty()", "array to store: {0} for key: {1}/{2}", FormatConverter.ByteArray2String(array, (uint)memoryStream.Length), Vehicle.VIN7, propertyName);
                Array.Resize(ref array, (int)memoryStream.Length);
                ConfigSettings.PutPersistencyData(PropertyEnum.PersistentProperty, $"{Vehicle.VIN7}/{propertyName}", array);
                memoryStream.Close();
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.SetPersistentProperty()", exception);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override void SetAcrossSessionProperty(string propertyName, object data)
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                new BinaryFormatter().Serialize(memoryStream, data);
                byte[] array = memoryStream.GetBuffer();
                Log.Info("ISTAModule.SetAcrossSessionProperty()", "array to store: {0} for key: {1}", FormatConverter.ByteArray2String(array, (uint)memoryStream.Length), propertyName);
                Array.Resize(ref array, (int)memoryStream.Length);
                ConfigSettings.PutPersistencyData(PropertyEnum.AcrossSessionProperty, propertyName, array);
                memoryStream.Close();
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.SetAcrossSessionProperty()", exception);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override void SetDealerSessionProperty(string propertyName, string propertyValue)
        {
            try
            {
                if (!string.IsNullOrEmpty(propertyName) && !string.IsNullOrEmpty(propertyValue))
                {
                    Log.Info("ISTAModule.SetDealerSessionProperty()", "set property {0}", propertyName);
                    DealerSessionProperty dealerSessionProperty = vehicle.DealerSessionProperties.Find((DealerSessionProperty x) => string.Compare(x.SessionPropertyName, propertyName, ignoreCase: true) == 0);
                    if (dealerSessionProperty != null)
                    {
                        dealerSessionProperty.SessionPropertyValue = propertyValue;
                    }
                    else
                    {
                        vehicle.DealerSessionProperties.Add(new DealerSessionProperty(propertyName, propertyValue));
                    }
                }
                else
                {
                    Log.Warning("ISTAModule.SetDealerSessionProperty()", "Property name and property value must be different from null and string empty Name : {0} Value : {1}", propertyName, propertyValue);
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.SetDealerSessionProperty()", exception);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override object GetAcrossSessionProperty(string propertyName)
        {
            Log.Info("ISTAModule.GetAcrossSessionProperty()", "get perisistent property {0} ", propertyName);
            try
            {
                byte[] persistencyData = ConfigSettings.GetPersistencyData(PropertyEnum.AcrossSessionProperty, $"{propertyName}");
                if (persistencyData != null)
                {
                    MemoryStream memoryStream = new MemoryStream(persistencyData);
                    object result = new BinaryFormatter().Deserialize(memoryStream);
                    memoryStream.Close();
                    return result;
                }
                Log.Info("ISTAModule.GetAcrossSessionProperty()", "no perisistent property \"{0}\" found", propertyName);
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.GetAcrossSessionProperty()", exception);
            }
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override object GetPersistentProperty(string propertyName)
        {
            Log.Info("ISTAModule.GetPersistentProperty()", "get perisistent property {0} for key: {1}/{2}", propertyName, Vehicle.VIN7, propertyName);
            try
            {
                byte[] persistencyData = ConfigSettings.GetPersistencyData(PropertyEnum.PersistentProperty, $"{Vehicle.VIN7}/{propertyName}");
                if (persistencyData != null)
                {
                    MemoryStream memoryStream = new MemoryStream(persistencyData);
                    object result = new BinaryFormatter().Deserialize(memoryStream);
                    memoryStream.Close();
                    return result;
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.GetPersistentProperty()", exception);
            }
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override object GetProperty(string propertyName)
        {
            object obj = null;
            Log.Info("ISTAModule.GetProperty()", "trying to get property {0}", propertyName);
            if (Vehicle.SessionDataStore != null)
            {
                obj = Vehicle.SessionDataStore.getParameter(propertyName);
                if (obj == null)
                {
                    Log.Warning("ISTAModule.GetProperty()", "property {0} not found in session data store of current vehicle context", propertyName);
                }
            }
            else
            {
                Log.Error("ISTAModule.GetProperty()", "session data store in vehicle context was null!");
            }
            return obj;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override T GetPersistentProperty<T>(string propertyName)
        {
            try
            {
                Type typeFromHandle = typeof(T);
                return (T)Convert.ChangeType(GetPersistentProperty(propertyName), typeFromHandle, CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.GetPersistentProperty<T>()", exception);
            }
            return default(T);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override T GetProperty<T>(string propertyName)
        {
            try
            {
                Type typeFromHandle = typeof(T);
                return (T)Convert.ChangeType(GetProperty(propertyName), typeFromHandle, CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.GetProperty<T>()", exception);
            }
            return default(T);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override string GetDealerSessionProperty(string propertyName)
        {
            try
            {
                Log.Info("ISTAModule.GetDealerSessionProperty", "trying to get outlet property {0}", propertyName);
                DealerSessionProperty dealerSessionProperty = vehicle.DealerSessionProperties.Find((DealerSessionProperty y) => string.Compare(y.SessionPropertyName, propertyName, ignoreCase: true) == 0);
                if (dealerSessionProperty != null)
                {
                    return dealerSessionProperty.SessionPropertyValue;
                }
                Log.Warning("ISTAModule.GetDealerSessionProperty()", "property {0} not found in current vehicle", propertyName);
                return null;
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.GetDealerSessionProperty()", exception);
                return null;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        protected void DoAsyncExecution(Action action)
        {
            if (action != null)
            {
                Task.Factory.StartNew(action);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override async void DoBackgroundProgressbarExecution(Action action)
        {
            //[-] InteractionModel progress = new InteractionProgressModel();
            try
            {
                //[-] logic.Services.InteractionService.Register(progress);
                await Task.Run(action);
            }
            finally
            {
                //[-] logic.Services.InteractionService.Deregister(progress);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [AuthorAPIHidden]
        public override void LogStatement(string headlineValue, params object[] paramList)
        {
            try
            {
                Log.Info(Log.CurrentMethod(), "Enter Method");
                DateTime now = DateTime.Now;
                string method = string.Format(CultureInfo.InvariantCulture, "{0}.{1}()", GetType().Name, base.LastCallingMethod);
                IProtocolBasic fastaProtocoler = FastaProtocoler;
                if (fastaProtocoler != null)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    if (paramList != null)
                    {
                        Dictionary<string, string> dictionary = new Dictionary<string, string>();
                        for (int i = 0; i < paramList.Length - 1; i += 2)
                        {
                            string text = ((paramList[i] == null) ? "" : paramList[i].ToString());
                            string text2 = ((paramList[i + 1] == null) ? "" : paramList[i + 1].ToString());
                            Log.Info(Log.CurrentMethod(), "Add key value pair from paramList: Key: {0} - Value: {1}", text, text2);
                            stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0}:{1} ", text, text2));
                            dictionary.Add(text, text2);
                        }
                        //[-] fastaProtocoler.AddLogStatement(headlineValue, dictionary, now);
                    }
                    else
                    {
                        stringBuilder.Append("Empty parameter list added");
                    }
                    Log.Info(method, string.Format(CultureInfo.InvariantCulture, "LogStatement. StructuredString: {0}. StructureElementValues: {1}", headlineValue, stringBuilder));
                    Log.Info(Log.CurrentMethod(), "End Method");
                }
                else
                {
                    Log.Error(Log.CurrentMethod(), "No FASTA protocoling possible.");
                    Log.Info(Log.CurrentMethod(), "End Method");
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
            }
        }

        public override FcFnActivationResult StoreAndActivateFcFn(int appNo, int upgradeIdx, byte[] fsc)
        {
            if (logic == null)
            {
                Log.Warning("ISTAModule.StoreAndActivateFcFn()", "Logic must not be null!");
                return FcFnActivationResult.ErrorUnexpected;
            }
            IProgrammingService programmingService = logic.ProgrammingService;
            if (programmingService == null)
            {
                Log.Warning("ISTAModule.StoreAndActivateFcFn()", "ProgrammingService must not be null!");
                return FcFnActivationResult.ErrorUnexpected;
            }
            try
            {
                //[-] IICOMHandler iICOMHandler = null;
                //[-] iICOMHandler = new ICOMHandler(logic);
                //[-] return programmingService.StoreAndActivateFcFn(vehicle, appNo, upgradeIdx, fsc, EcuKom, FastaProtocoler, iICOMHandler);
                //[+] return FcFnActivationResult.ErrorUnexpected;
                return FcFnActivationResult.ErrorUnexpected;
            }
            catch (Exception exception)
            {
                Log.WarningException("ISTAModule.StoreAndActivateFcFn()", exception);
                return FcFnActivationResult.ErrorUnexpected;
            }
        }
#if false
        public override string DeactivateOtdLscCalls()
        {
            IMethodCall methodCall = FastaProtocoler.AddMethodCall("DeactivateOtdLscCalls");
            string vIN = logic.VecInfo.VIN17;
            long km = -1L;
            try
            {
                if (logic.VecInfo.Gwsz.HasValue)
                {
                    decimal value = logic.VecInfo.Gwsz.Value;
                    km = ((logic.VecInfo.GwszUnit != GwszUnitType.miles) ? Convert.ToInt32(value) : Convert.ToInt32(UnitConverter.ConvertUnit(LengthUnit.Mile, LengthUnit.Kilometer, (double)value)));
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException("ISTAModule.DeactivateOtdLscCalls()", exception);
            }
            PDIRequest data = (logic as Logic).CreatePDIRequest(vIN, km);
            HttpStatusCode httpStatusCode = EDGEProcessorFactory.Create(logic.ProgrammingSession?.BackendCallsWatchDogProgramming).SendDataToBackend(vIN, data, BackendServiceType.EDGEPDI);
            Log.Info("ISTAModule.DeactivateOtdLscCalls()", "Sending cleaning data: ISTAEdge returned status {0}", httpStatusCode);
            string result = (methodCall.ReturnValue = httpStatusCode.ToString());
            methodCall.EndTime = DateTime.Now;
            return result;
        }

        private void ProtocolRheingoldApiUsage(string interfaceName)
        {
            string aNA08_TestmodulesUsingNotAllowedIstaFunctions_nu_LF = ServiceCodes.ANA08_TestmodulesUsingNotAllowedIstaFunctions_nu_LF;
            string callingModule = CallingModule;
            FastaProtocoler.AddServiceCode(aNA08_TestmodulesUsingNotAllowedIstaFunctions_nu_LF, "Module: " + callingModule + ", interface: " + interfaceName, LayoutGroup.D, allowMultipleEntries: true);
        }
#endif
        public virtual Vehicle VehicleDeepClone(Vehicle vehicle)
        {
            return vehicle.DeepClone();
        }

        Type IHideObjectMembers.GetType()
        {
            return GetType();
        }
    }
}
